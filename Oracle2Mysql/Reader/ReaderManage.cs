using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Oracle2Mysql.Reader
{
    class ReaderManage
    {
        Program Base;
        Dictionary<int, List<string>> allText = new Dictionary<int, List<string>>();
        Dictionary<string, string> AllReplace = new Dictionary<string, string>()
        {
            [" CHAR("] = " VARCHAR(",
            ["LOGGING"] = "/*LOGGING",
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
            ["COMMENT ON TABLE"] = "-- COMMENT ON TABLE",
        };

        Dictionary<string, string> AllFixReplace = new Dictionary<string, string>()
        {
            [" DOUBLE("] = " ,0)",
            ["/*DEFAULT SUBSTR"] = " )*/,",
        };
        public ReaderManage(Program ba)
        {
            Base = ba;
        }

        public void ProcesseFile(string file)
        {
            int current_index = 0;
            Base.ThreadManager.AddAsyncTask(() => {
                if (Directory.Exists(@".\AllSql"))
                    Directory.Delete(@".\AllSql", true);
                Directory.CreateDirectory(@".\AllSql");
                int current_lines = 0;
                List<string> LinesReade = File.ReadAllLines(file).ToList();
                List<int> readed = new List<int>();
                bool table = false;
                foreach (string line in LinesReade)
                {
                    if (!allText.ContainsKey(current_index))
                        allText.Add(current_index, new List<string>() { "SET SQL_MODE = ORACLE;" });

                    if (line.Contains("DROP TABLE") || line.Contains("CREATE TABL"))
                    {
                        table = true;
                    }
                    if (table)
                    {
                        readed.Add(current_lines);
                        allText[current_index].Add(line);
                        if (line.Contains(";"))
                            table = false;
                    }
                    current_lines++;
                }
                MadeFile(current_index);
                current_index++;
                current_lines = 0;
                foreach (string line in LinesReade)
                {
                    if (!readed.Contains(current_lines))
                    {
                        if (!allText.ContainsKey(current_index))
                            allText.Add(current_index, new List<string>() { "SET SQL_MODE = ORACLE;" });
                        allText[current_index].Add(line);
                        if ((line.Length == 0 || line.Contains(";")) && allText[current_index].Count > Base.Config.Config().LinePerSql)
                        {
                            MadeFile(current_index);
                            current_index++;
                        }
                    }
                    current_lines++;
                }

                LinesReade.ToList().Clear();
                readed.Clear();
            });
            Base.ThreadManager.WaitForEnd(current_index);
        }

        public void MadeFile(int index)
        {
            Base.ThreadManager.AddAsyncTask(() => {
                List<string> Converted = new List<string>();
                foreach (string line in allText[index])
                    Converted.Add( ConvertLine(line));
                File.AppendAllLines(@".\AllSql\Sql" + index + ".sql", Converted);
                allText[index].Clear();
                Converted.Clear();
            });
        }

        private string ConvertLine(string line)
        {
            string ret = line;
            foreach (string key in AllReplace.Keys)
            {
                ret = ret.Replace(key, AllReplace[key]);                
            }
            foreach (string key in AllFixReplace.Keys)
            {
                if(ret.Contains(key))
                    switch (key)
                    {
                        case " DOUBLE(":
                            ret = ret.Replace(")", AllFixReplace[key]);
                            break;
                        case "/*DEFAULT SUBSTR":
                            ret = ret.Replace("),", AllFixReplace[key]);
                            break;

                    }
            }
            return ret;
        }
        
    }
}
