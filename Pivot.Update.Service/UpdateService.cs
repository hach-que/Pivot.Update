using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Pivot.Update.Service
{
    public partial class UpdateService : ServiceBase
    {
        private Manager m_Manager;

        public UpdateService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("Pivot Update Service"))
            {
                System.Diagnostics.EventLog.CreateEventSource("Pivot Update Service", "Application");
            }
            this.c_EventLog.Source = "Pivot Update Service";
            this.c_EventLog.Log = "Application";
            this.m_Manager = new Manager(new DualLog(this.c_EventLog));
        }

        protected override void OnStart(string[] args)
        {
            this.m_Manager.Start();
        }

        protected override void OnStop()
        {
            this.m_Manager.Stop();
        }
    }
}
