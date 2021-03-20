using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Oracle2Mysql.Logger
{
    class LogInfo
    {
        public string LogType { get; set; } = "Info";
        public string Content { get; set; } = "Content";

        public ConsoleColor ForeColor { get; set; } = ConsoleColor.White;

        public LogInfo(string type , string content)
        {
            LogType = type;
            Content = content;
        }
        public LogInfo(string type , string content, ConsoleColor fore)
        {
            LogType = type;
            Content = content; 
            ForeColor = fore;
        }
    }
}
