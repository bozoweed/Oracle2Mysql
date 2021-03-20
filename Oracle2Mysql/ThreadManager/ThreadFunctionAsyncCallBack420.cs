
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oracle2Mysql.ThreadManager
{
    class ThreadFunctionAsyncCallBack420 : ThreadFunction420
    {

        Action Func { get; set; }
        Action CallBack { get; set; }
        Program Base;
        public ThreadFunctionAsyncCallBack420(Program Bas, int id, Action func, Action callBack): base(Bas, id)
        {
            Func = func;
            Base = Bas;
            CallBack = callBack;
        }

        public override void Start()
        {
            DateTime beginTime420 = DateTime.Now;
            Base.ThreadManager.TaskStarted(DateTime.Now.Subtract(base.CreatedAt).TotalMilliseconds);
            Func();
            CallBack();
            Base.ThreadManager.TaskEnded(DateTime.Now.Subtract(beginTime420).TotalMilliseconds);
        }


    }
}
