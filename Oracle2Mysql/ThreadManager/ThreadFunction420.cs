using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Oracle2Mysql.ThreadManager
{
    class ThreadFunction420
    {

        public int Id { get; set; } = -1;
        Program Base;
        public DateTime CreatedAt = DateTime.Now;

        public ThreadFunction420(Program bas, int id) 
        {
            Id = id;
            Base = bas;
        }

        public virtual void Start()
        {
            Base.LogWarning("Thread Request Was Empty");
        } 
    }
}
