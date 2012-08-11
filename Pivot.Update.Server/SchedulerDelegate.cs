using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using System.Diagnostics;

namespace Pivot.Update.Server
{
    public class SchedulerDelegate : ISchedulerDelegate
    {
        #region ISchedulerDelegate Members

        public void OnException(IScheduler scheduler, Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }

        public void OnStop(IScheduler scheduler)
        {
        }

        #endregion
    }
}
