using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseCreator.Logic
{
    internal class Logger
    {
        internal static List<Log> Logs = new List<Log>();
        internal static void AddLog(string log, bool isDebug)
        {
            string data = DateTime.Now.ToShortDateString() + " | " + DateTime.Now.ToLongTimeString() + " : " + log;
            Logs.Add(new Log(data, isDebug));
            OnLogAdded(null, EventArgs.Empty);
        }

        public static event EventHandler OnLogAdded = delegate { };
    }

    internal class Log
    {
        internal string LogData { get; }
        internal bool IsDebug { get; }

        internal Log(string log, bool isDebug)
        {
            LogData = log;
            IsDebug = isDebug;
        }
    }
}
