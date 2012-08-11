using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Pivot.Update.Service
{
    public class DualLog
    {
        private EventLog m_EventLog;

        public DualLog(EventLog eventLog)
        {
            this.m_EventLog = eventLog;
        }

        public void WriteEntry(string msg, EventLogEntryType eventType = EventLogEntryType.Information)
        {
            this.m_EventLog.WriteEntry(msg, eventType);
            Console.WriteLine(msg);
        }

        internal void WriteException(string msg, Exception e)
        {
            this.WriteEntry(msg + "\r\n" + e.Message + "\r\n" + e.StackTrace, EventLogEntryType.Error);
        }
    }
}
