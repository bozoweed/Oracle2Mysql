using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oracle2Mysql.Reader
{
    class ReaderManage
    {
        Program Base;
        Dictionary<int, List<string>> allText = new Dictionary<int, List<string>>();
        Dictionary<string, string> AllReplace = new Dictionary<string, string>()
        {
            [" CHAR("] = " VARCHAR(",
            ["/**"] = "/*",
            ["**/"] = "*/",
            ["**/"] = "*/",
            ["DROP SEQUENCE"] = "-- DROP SEQUENCE",
            ["CREATE SEQUENCE"] = "-- CREATE SEQUENCE",
            ["LOGGING"] = "/*LOGGING",
            ["USABLE;"] = "USABLE;*/",
            ["DISABLE ROW MOVEMENT"] = "DISABLE ROW MOVEMENT*/",
            ["DROP TABLE"] = "DROP TABLE IF EXISTS",
            [" VARCHAR2("] = " VARCHAR(",
            ["HEXTORAW("] = "HEX(",
            [" LONG RAW"] = " LONGBLOB",
            [" LONG"] = " LONGTEXT",
            [" LONGTEXTBLOB"] = " LONGBLOB",
            ["DEFAULT SYSDATE"] = "",
            ["DEFAULT sysdate"] = "",
            ["DEFAULT Sysdate"] = "",
            ["DEFAULT Sysdate"] = "",
            [@"\'"] = @"\\'",
            ["DEFAULT SUBSTR"] = "/*DEFAULT SUBSTR",
            ["SYSDATE"] = "SYSDATE()",
            [" CHARACTER("] = " VARCHAR(",
            [" FLOAT("] = " DOUBLE(",
            [" NUMBER("] = " DECIMAL(",
            [" TO_DATE("] = " STR_TO_DATE(",
            ["(TO_DATE("] = "(STR_TO_DATE(",
            [" NUMBER"] = " INT",
            [" BYTE)"] = ")",
            [" VISIBLE"] = "",
            [" NOT NULL"] = "",
            ["\" DATE"] = "\" DATETIME",
            ["COMMENT ON COLUMN"] = "-- COMMENT ON COLUMN",
            ["COMMENT ON TABLE"] = "ALTER TABLE",
        };

        Dictionary<string, string> AllFixReplace = new Dictionary<string, string>()
        {
            [" DOUBLE("] = " ,0)",
            ["/*DEFAULT SUBSTR"] = @")*/,",
            ["ADD CONSTRAINT"] = "-- ALTER TABLE",
            ["ALTER TABLE"] = " COMMENT ",
            ["STR_TO_DATE"] = "%Y-%m-%d %h:%i:%s",
            ["NOT DEFERRABLE INITIALLY IMMEDIATE NORELY VALIDATE"] = "-- ",
        };
        public ReaderManage(Program ba)
        {
            Base = ba;
        }

        public void ProcesseFile(string file)
        {
            Base.ThreadManager.AddAsyncTask(() =>{
                ResetDumpDirectory();
                List<string> LinesReade = File.ReadAllLines(file).ToList();
                GenerateTable(LinesReade);



            });
            Base.ThreadManager.WaitForEnd();
            allText.Clear();
        }


        private List<string> RemoveCreatView(List<string> LinesReade)
        {
            bool creatview = false;
            for (int i = 0; i < LinesReade.Count; i++)
            {
                string line = LinesReade[i];
                if (line.Contains("CREATE OR REPLACE VIEW"))
                {
                    creatview = true;
                    line = line.Replace("CREATE OR REPLACE VIEW", "/* CREATE OR REPLACE VIEW");
                }

                if (creatview )
                {
                    if (line.Contains("*/"))
                        line = line.Replace("*/", "");
                    if (line.Contains(";"))
                    {
                        line = line.Replace(";", "; */");
                        creatview = false;
                    }
                }
                LinesReade[i] = line;
            }
            return RemoveCreatTrigger(LinesReade);
        }

        private List<string> RemoveCreatTrigger(List<string> LinesReade)
        {
            bool creatview = false;
            for (int i = 0; i < LinesReade.Count; i++)
            {
                string line = LinesReade[i];
                if (line.Contains("CREATE TRIGGER"))
                {
                    creatview = true;
                    line = line.Replace("CREATE TRIGGER", "/* CREATE TRIGGER");
                }

                if (creatview)
                {
                    if (line.Contains("*/"))
                        line = line.Replace("*/", "");
                    if (line.Length == 1 && line.Contains("/") && !line.Contains("/*"))
                    {
                        
                        
                            line = line.Replace("/", "*/");
                            creatview = false;
                        
                    }
                }
                LinesReade[i] = line;
            }
            return FixCreatIndex(LinesReade);
        }

        private List<string> FixCreatIndex(List<string> LinesReade)
        {
            bool creatview = false;
            for (int i = 0; i < LinesReade.Count; i++)
            {
                string line = LinesReade[i];
                if (line.Contains("CREATE INDEX") || line.Contains("CREATE UNIQUE INDEX"))
                {
                    creatview = true;
                }

                if (creatview && line.Contains("ON "))
                {

                   
                    line = line.Replace("ASC", "").Replace("DESC", "").Replace(")", ");");
                    creatview = false;
                   
                }
                LinesReade[i] = line;
            }
            return LinesReade;
        }


        private void GenerateTable(List<string> LinesReade)
        {
            bool table = false;
            string current_table = "";
            string db_name = "";
            int current_lines = 0;
            LinesReade = RemoveCreatView(LinesReade);
            List<int> readed = new List<int>();
            List<Task<string>> TaskQueu = new List<Task<string>>();
            for (int i= 0; i<LinesReade.Count; i++)
            {
                string line = LinesReade[i];
                if (!allText.ContainsKey(0))
                    allText.Add(0, new List<string>() { "SET SQL_MODE = ORACLE;" });

                if (line.Contains("DROP TABLE") || line.Contains("CREATE TABLE"))
                {
                    if (db_name == "")
                        db_name = line.Split("\".\"")[0].Split("\"")[1].Trim();
                    if(line.Contains("DROP TABLE"))
                        current_table = line.Replace("DROP TABLE ", "").Replace(";", "");
                    table = true;
                }
                line = line.Replace("\"" + db_name + "\".","");
                LinesReade[i] = line;
                if (table)
                {
                    readed.Add(current_lines);
                    TaskQueu.Add(BuildColumn(line, current_table, LinesReade, i));
                    if (line.Contains(";"))
                        table = false;
                }
                current_lines++;
            }
            Task.WhenAll(TaskQueu);
            foreach (Task<string> task in TaskQueu)
                allText[0].Add(task.Result);
            MadeFile(0);
            TaskQueu.Clear();
            ReadInsert(LinesReade, readed);
        }

        private Task<string> BuildColumn(string line, string table, List<string> allLines , int current_pos)
        {
            TaskCompletionSource<string> s_tcs = new TaskCompletionSource<string>();
            Base.ThreadManager.AddAsyncTask(() => {

                s_tcs.SetResult( AddComment(line, table, allLines, current_pos));
            });
            return s_tcs.Task;
        }

        private void ReadInsert(List<string> LinesReade, List<int> readed)
        {
            int current_index = 1;
            int current_lines = 0;
            int last_index_donne = 0;
            foreach (string line in LinesReade)
            {
                if (!readed.Contains(current_lines))
                {
                    if (!allText.ContainsKey(current_index))
                        allText.Add(current_index, new List<string>() { "SET SQL_MODE = ORACLE;" });
                    allText[current_index].Add(line);
                    if ((line.Length == 0 || line.Contains(";")) && allText[current_index].Count > Base.Config.Config().LinePerSql)
                    {
                        last_index_donne = current_index;
                        MadeFile(current_index);
                        current_index++;
                    }
                }
                current_lines++;
            }
            if(last_index_donne != current_index)
                MadeFile(current_index);
            readed.Clear();
            LinesReade.Clear();
        }
        
        private void ResetDumpDirectory()
        {
            if (Directory.Exists(@".\AllSql"))
                Directory.Delete(@".\AllSql", true);
            Directory.CreateDirectory(@".\AllSql");
        }

        private string AddComment(string line, string table, List<string> allLines, int current_pos)
        {
            if (!line.Contains("\"") || (line.Contains("DROP TABLE") || line.Contains("CREATE TABLE")))
                return line;
            string comment = GetComment(line, table, allLines, current_pos);
            if (comment == "''")
                return line;
            return line[line.Length-1].ToString() == "," ? line.Remove(line.Length - 1)+ " COMMENT " + comment + ","  : line+ " COMMENT " + comment;
        }

        private string GetComment(string line, string table, List<string> allLines, int current_pos)
        {            
            string column = line.Split("\"")[1];
            return LookForComment(table + ".\"" + column, allLines, current_pos);
        } 

        private string LookForComment(string column, List<string> allLines, int current_pos)
        {
            for (int i = current_pos; i < allLines.Count; i++)
            {
                string line = allLines[i];
                if (line.Contains(column))
                {
                    return line.Split("IS ")[1].Replace(";", "");

                }
                if (line.Contains("DROP TABLE") || line.Contains("CREATE TABLE") || line.Contains("INSERT "))
                    return "''";
            }
            return "''";
        }

        private void MadeFile(int index)
        {
            Base.ThreadManager.AddAsyncTask(() => {
                List<Task<string>> Converted = new List<Task<string>>();
                foreach (string line in allText[index])
                    Converted.Add( ConvertLine(line));
                Task.WhenAll(Converted);
                List<string> conv = new List<string>();
                foreach (Task<string> line in Converted)
                    conv.Add(line.Result);
                File.AppendAllLines(@".\AllSql\Sql" + index + ".sql", conv);
                allText[index].Clear();
                Converted.Clear();
                conv.Clear();
            });
        }

        private Task<string> ConvertLine(string line)
        {
            TaskCompletionSource<string> s_tcs = new TaskCompletionSource<string>();
            Base.ThreadManager.AddAsyncTask(() =>
            {

                string ret = line;
                foreach (string key in AllReplace.Keys)
                {
                    ret = ret.Replace(key, AllReplace[key]);
                }
                foreach (string key in AllFixReplace.Keys)
                {
                    if (ret.Contains(key))
                        switch (key)
                        {
                            case " DOUBLE(":
                                ret = ret.Replace(")", AllFixReplace[key]);
                                break;
                            case "/*DEFAULT SUBSTR":
                                ret = ret.Replace("),", AllFixReplace[key]).Replace(") COMMENT", @")*/ COMMENT");
                                break;
                            case "ALTER TABLE":
                                ret = ret.Replace(" IS ", AllFixReplace[key]);
                                break;
                            case "STR_TO_DATE":
                                ret = ret.Replace("SYYYY-MM-DD HH24:MI:SS", AllFixReplace[key]);
                                break;
                            case "ADD CONSTRAINT":
                                ret = ret.Replace("ALTER TABLE", AllFixReplace[key]);
                                break;
                            case "NOT DEFERRABLE INITIALLY IMMEDIATE NORELY VALIDATE":
                                ret =  AllFixReplace[key] + ret;
                                break;

                        }
                }
                s_tcs.SetResult(ret);
            });
            return s_tcs.Task;
        }
        
    }
}
