using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Oracle2Mysql.ConfigManager
{
    class ConfigInfo
    {        
        public int Thread { set; get; } = Environment.ProcessorCount;
        public int ThreadSleep { set; get; } = 0;
        public long LinePerSql { set; get; } = 30000;
        public bool Debug { set; get; } = false;
        public ConfigInfo()
        {

        }
    }
}
