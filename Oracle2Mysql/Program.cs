using Oracle2Mysql.Classes;
using Oracle2Mysql.ConfigManager;
using Oracle2Mysql.Logger;
using Oracle2Mysql.Reader;
using Oracle2Mysql.ThreadManager;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Oracle2Mysql
{
    class Program
    {
        public bool Stopped = false;
        internal LoggerManager LogManage;
        internal ThreadManager420 ThreadManager = new ThreadManager420();
        internal Config420 Config;
        internal ReaderManage Reader;
        Timer Debug = new Timer(1000);

        static void Main(string[] args)
        {

            new Program().start();
        }

        public void Log(string msg)
        {
            LogManage.AddCustomLog("System", msg, ConsoleColor.White);
        }

        public void LogError(Exception msg)
        {
            LogManage.AddCustomLog("System", msg.ToString(), ConsoleColor.Red);
        }

        public void LogWarning(string msg)
        {
            LogManage.AddCustomLog("System", msg, ConsoleColor.Yellow);
        }
        public MemoryCleanUp Mem = new MemoryCleanUp();
        public void start()
        {


            Config = new Config420(this);
            Config.SaveConfig();
            LogManage = new LoggerManager(this);
            ThreadManager.StartServerThread(this);
            Reader = new ReaderManage(this);
            Debug.Elapsed += cleanup;
            Debug.Start();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;// Si on ferme le serveur on appel la fonction
            Console.CancelKeyPress += OnProcessExit;
            while (!Stopped)
            {
                Log("Enter file_name.sql or Exit:");
                string read = Console.ReadLine();
                switch (read)
                {
                    case "Exit":
                        Log("Process kill");
                        ThreadManager.Stopped = true;
                        ThreadManager.WaitForEnd();
                        LogManage.Stop = true;
                        Stopped = true;
                        break;
                    default:
                        Log("Start Reading Folder");
                        Reader.ProcesseFile(@".\" + read);
                        Mem.EmptyWorkingSetFunction();
                        Mem.ClearFileSystemCache(false);
                        break;
                }
            }

        }

        private async void cleanup(object sender, ElapsedEventArgs e)
        {
            Mem.EmptyWorkingSetFunction();
            Mem.ClearFileSystemCache(false);
            await Task.Yield();
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            ThreadManager.Stopped = true;
            ThreadManager.WaitForEnd();
            LogManage.Stop = true;
            Stopped = true;
        }
    }
}
