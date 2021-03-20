using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace Oracle2Mysql.Logger
{
    class LoggerManager
    {
        Thread LoggerThread;
        public List<LogInfo> AllLogs = new List<LogInfo>();
        public bool Stop = false;
        public LoggerManager(Program Base)
        {
            LoggerThread = new Thread(() =>
            {
                while (!Stop || AllLogs.Count >0 )
                {
                    int wait = 1;
                    if (AllLogs.Count > 0)
                    {
                        wait = 0;
                        LogInfo log = null;
                        lock (AllLogs)
                            log = AllLogs.FirstOrDefault();
                        if (log != null)
                        {
                            switch (log.LogType)
                            {
                                case "System":                                    
                                    Console.ForegroundColor = log.ForeColor;
                                    Console.WriteLine(log.Content);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                            }
                            lock(AllLogs)
                            AllLogs.Remove(log);
                        }
                    }
                    Thread.Sleep(wait);
                }
            });
            LoggerThread.Start();
        }

        public void AddLog(string type, string content ){
                AllLogs.Add(new LogInfo(type, content));
        }

        public void AddCustomLog(string type, string content, ConsoleColor fore ){
            lock (AllLogs)
                AllLogs.Add(new LogInfo(type, content, fore));
        }
}
}
