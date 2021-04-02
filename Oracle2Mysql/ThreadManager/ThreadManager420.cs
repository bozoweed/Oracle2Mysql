using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Oracle2Mysql.ThreadManager
{
    class ThreadManager420
    {
        Program Base;
        public List<Thread420> AllThread = new List<Thread420>();
        public List<ThreadFunction420> AllTask = new List<ThreadFunction420>();
        public int RequetteId = -1;
        public bool Stopped = false;
        public bool Adding = false;
        Timer Debug = new Timer(1000);
        double MinQueueTime = 9;
        double MaxQueueTime = 0;
        int drawed = 0;
        int execS = 0;
        public void StartServerThread(Program ba)//fonction pour tester la connection
        {
            Base = ba;
            Debug.Elapsed += DrawThreadManagerInfo;
            Debug.Start();
            Base.Log("--------[Thread Pool Stared]----------");
        }

        private void DrawThreadManagerInfo(object sender, ElapsedEventArgs e)
        {
            if (Base.Config.Config().Debug && drawed > 4)
            {
                drawed = 0;
                /*Base.Log("---------[Server Thread Executed in Minimum " + MinExecTime + " | Max " + MaxExecTime + "]---------");
                Base.Log("---------[Server Thread Time In Queue Minimum " + MinQueueTime + " | Max " + MaxQueueTime + "]---------");*/


            }
            drawed++;

            MinQueueTime = 9;
            MaxQueueTime = 0;
        }
        public void TaskStarted(double timeInQueue)
        {

            if (MinQueueTime > timeInQueue)
                MinQueueTime = timeInQueue;
            if (timeInQueue > MaxQueueTime)
                MaxQueueTime = timeInQueue;
        }
        /*public void TaskEnded(double ExecTime)
        {
            if (MinExecTime > ExecTime)
                MinExecTime = ExecTime;
            if (ExecTime > MaxExecTime)
                MaxExecTime = ExecTime;
            execS++;
        }*/
        public void TaskEnded()
        {
            execS++;
        }

        public void AddAsyncTask(Action func)
        {


            lock (AllTask)
                lock (AllThread)
                {

                    Adding = true;
                    RequetteId++;
                    AllTask.Add(new ThreadFunctionAsync420(Base, RequetteId, func));
                    if (AllThread.Count < Base.Config.Config().Thread)
                        AllThread.Add(new Thread420(Base));

                }

            Adding = false;
        }

        public void AddAsyncCallBackTask(Action func, Action callBack)
        {
            lock (AllTask)
                lock (AllThread)
                {
                    Adding = true;
                    RequetteId++;
                    AllTask.Add(new ThreadFunctionAsyncCallBack420(Base, RequetteId, func, callBack));
                    if (AllThread.Count < Base.Config.Config().Thread)
                        AllThread.Add(new Thread420(Base));

                }
            Adding = false;
        }

        public void RemoveKilledThread(Thread420 thread)
        {
            lock (AllTask)
                lock (AllThread)
                {
                    AllThread.Remove(thread);
                    if (AllThread.Count == 0 && AllTask.Count > 0 && !Adding)
                        Base.LogWarning("All Thread are stopped but " + AllTask.Count + " execution left");
                    while (AllTask.Count > AllThread.Count && AllThread.Count < Base.Config.Config().Thread)
                        AllThread.Add(new Thread420(Base));
                }



        }

        public void WaitForEnd(long Totalcount = 0)
        {
            while (AllThread.Count > 0 || AllTask.Count > 0 || Adding)
            {
                Console.Title = (Totalcount > 0 ? "Queu Left : " + (Totalcount) + " | " : "") + execS + " Request Seconde | Thread Running " + AllThread.Count + " | Thread Task Queu " + AllTask.Count;
                Totalcount -= execS;
                execS = 0;
                System.Threading.Thread.Sleep(1000);
            }

            Console.Title = (Totalcount > 0 ? "Queu Left : " + (Totalcount) + " | " : "") + execS + " Request Seconde | Thread Running " + AllThread.Count + " | Thread Task Queu " + AllTask.Count;
        }
    }
}
