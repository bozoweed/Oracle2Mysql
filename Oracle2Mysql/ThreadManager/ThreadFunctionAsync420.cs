
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oracle2Mysql.ThreadManager
{
    class ThreadFunctionAsync420 : ThreadFunction420
    {

        Action Func { get; set; }
        Program Base;
        public ThreadFunctionAsync420(Program Bas, int id, Action func) : base(Bas, id)
        {
            Func = func;
            Base = Bas;
        }

        public override void Start()
        {
            DateTime beginTime420 = DateTime.Now;
            Base.ThreadManager.TaskStarted(DateTime.Now.Subtract(base.CreatedAt).TotalMilliseconds);
            Func();
            Base.ThreadManager.TaskEnded(DateTime.Now.Subtract(beginTime420).TotalMilliseconds); 
        }
    }

    
}
