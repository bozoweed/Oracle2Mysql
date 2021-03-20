
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oracle2Mysql.ThreadManager
{
    class Thread420
    {
        Program Base;
        Thread Thread;
        public Thread420(Program bas)
        {
            Base = bas;
            Thread = new Thread(() =>
            {

                //while ( Base.ThreadManager.AllTask.Count > 0 || (!Base.ThreadManager.Stopped && Base.ThreadManager.AllThread.Count < 2  )  )
                while (Base.ThreadManager.AllTask.Count > 0)
                {
                    ThreadFunction420 task = null;
                        lock (Base.ThreadManager.AllTask)
                        {
                            task = Base.ThreadManager.AllTask.Count > 0 ?  Base.ThreadManager.AllTask[0] : null;
                            if (task != null)
                                Base.ThreadManager.AllTask.Remove(task);
                        }
                  
                    try
                    {
                       
                            task?.Start();
                    }
                    catch (Exception e)
                    {
                        Base.LogError(e);
                    }
                    /*
                    int check = 0;
                    if (Base.ThreadManager.AllTask.Count == 0)
                        while (Base.ThreadManager.AllTask.Count == 0 && check < 3)
                        {
                            Thread.Sleep(2);
                            check++;
                        }*/
                    Thread.Sleep(Base.Config.Config().ThreadSleep);
                }

                Base.ThreadManager.RemoveKilledThread(this);
            });

            Thread.Start();
        }


    }
}
