using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

namespace Pivot.Update.Service
{
    public class Manager
    {
        private Timer m_Timer = null;
        private DualLog m_DualLog = null;
        private bool m_Initial = true;
        private TcpServerChannel m_IPC = null;
        private InterProcess m_InterProcess = null;

        public Manager(DualLog log)
        {
            this.m_DualLog = log;
            this.m_Initial = true;
        }

        #region Service Control

        public void Start()
        {
            this.m_Timer = new Timer(1000); // First one should trigger in 1 second.
            this.m_Timer.Elapsed += new ElapsedEventHandler(m_Timer_Elapsed);
            this.m_Timer.AutoReset = true;
            this.m_Timer.Start();
            this.m_InterProcess = new InterProcess();
            InterProcess.RegisterRequestedEvent += new ApplicationInstanceEventHandler(m_InterProcess_RegisterRequestedEvent);
            InterProcess.ScheduledUpdateRequestedEvent += new ApplicationInstanceEventHandler(m_InterProcess_ScheduledUpdateRequestedEvent);
            InterProcess.HasUpdatesRequestedEvent += new ApplicationInstanceEventHandler(m_InterProcess_HasUpdatesRequestedEvent);
            Dictionary<object, object> simple = new Dictionary<object, object>();
            simple.Add("port", 38088);
            simple.Add("bindTo", "127.0.0.1");
            this.m_IPC = new TcpServerChannel(simple, null);
            ChannelServices.RegisterChannel(this.m_IPC, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(InterProcess), "interprocess", WellKnownObjectMode.Singleton);
        }

        public void Stop()
        {
            this.m_Timer.Stop();
        }

        #endregion

        #region Interprocess API

        bool m_InterProcess_RegisterRequestedEvent(object sender, ApplicationInstanceEventArgs e)
        {
            Cache c = new Cache(false);
            if (c.Exists("locked") && c.Get<bool>("locked"))
                return false;

            if (!c.Exists("instances"))
                c.Set<ApplicationInstanceList>("instances", new ApplicationInstanceList());
            ApplicationInstanceList list = c.Get<ApplicationInstanceList>("instances");
            if (list.Count(v => v.FilesystemPath == e.LocalPath) > 0)
                return false;
            list.Add(new ApplicationInstance
            {
                LastCheckTime = new DateTime(1970, 1, 1),
                FilesystemPath = e.LocalPath,
                UpdateUrl = e.UpdateURI.ToString()
            });
            c.Set<ApplicationInstanceList>("instances", list);
            return true;
        }

        bool m_InterProcess_HasUpdatesRequestedEvent(object sender, ApplicationInstanceEventArgs e)
        {
            return this.HasUpdateSingle(e.LocalPath);
        }

        bool m_InterProcess_ScheduledUpdateRequestedEvent(object sender, ApplicationInstanceEventArgs e)
        {
            if (e.Time > 0)
            {
                Timer t = new Timer(e.Time * 1000); // Schedule an update in a certain number of seconds.
                t.Elapsed += new ElapsedEventHandler((a, b) =>
                {
                    this.m_DualLog.WriteEntry("Requested scheduled update check has started.");
                    this.PerformUpdateSingle(e.LocalPath, e.RestartPath);
                });
                t.AutoReset = false;
                t.Start();
                return true;
            }
            else
            {
                this.m_DualLog.WriteEntry("Requested scheduled update check has started.");
                this.PerformUpdateSingle(e.LocalPath, e.RestartPath);
                return true;
            }
        }

        #endregion

        #region Periodic Scheduling

        private void m_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.m_Timer.Interval = 5 * 60 * 1000; // Every 5 minutes.
            this.PerformUpdate(this.m_Initial);
            this.m_Initial = false;
        }

        #endregion

        #region Update Mechanisms

        private bool HasUpdateSingle(string localPath)
        {
            this.m_DualLog.WriteEntry("Checking whether updates exist for " + localPath + ".");
            Cache c = new Cache(false);
            ApplicationInstanceList list = c.Get<ApplicationInstanceList>("instances");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].FilesystemPath == localPath)
                {
                    // Perform update check.
                    Application a = new Application(c, new Uri(list[i].UpdateUrl), list[i].FilesystemPath);
                    list[i].LastCheckTime = DateTime.Now;
                    try
                    {
                        if (a.Check())
                        {
                            this.m_DualLog.WriteEntry("Updates exist for " + localPath + ".");
                            return true;
                        }
                        else
                        {
                            this.m_DualLog.WriteEntry("No updates available for " + localPath + ".");
                            return false;
                        }
                    }
                    catch (WebException)
                    {
                        this.m_DualLog.WriteEntry("Unable to check " + list[i].UpdateUrl + " for updates.  The server is not responding.", EventLogEntryType.Warning);
                    }
                }
            }
            return false;
        }

        private void PerformUpdateSingle(string localPath, string restartPath = null)
        {
            Cache c = new Cache(false);
            c.Set<bool>("locked", true);
            ApplicationInstanceList list = c.Get<ApplicationInstanceList>("instances");
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].FilesystemPath == localPath)
                {
                    this.m_DualLog.WriteEntry("Now checking " + list[i].UpdateUrl + " for updates.");

                    // Perform update check.
                    string result = "";
                    Application a = new Application(c, new Uri(list[i].UpdateUrl), list[i].FilesystemPath);
                    list[i].LastCheckTime = DateTime.Now;
                    try
                    {
                        a.Update((status, info) =>
                        {
                            switch (status)
                            {
                                case Application.UpdateStatus.Starting:
                                    result += "Update is starting...\r\n";
                                    break;
                                case Application.UpdateStatus.RetrievingFileList:
                                    result += "Retrieving file list... ";
                                    break;
                                case Application.UpdateStatus.RetrievedFileList:
                                    result += "done (" + info + " files to scan).\r\n";
                                    break;
                                case Application.UpdateStatus.DeletionStart:
                                    result += "Deleting " + info + "... ";
                                    break;
                                case Application.UpdateStatus.DownloadNewStart:
                                case Application.UpdateStatus.DownloadFreshStart:
                                    result += "Downloading " + info + "... ";
                                    break;
                                case Application.UpdateStatus.PatchStart:
                                    result += "Patching " + info + "... ";
                                    break;
                                case Application.UpdateStatus.PatchApplied:
                                    result += info + ". ";
                                    break;
                                case Application.UpdateStatus.Complete:
                                    result += "Update is complete.\r\n";
                                    break;
                                case Application.UpdateStatus.DeletionFinish:
                                case Application.UpdateStatus.DownloadNewFinish:
                                case Application.UpdateStatus.DownloadFreshFinish:
                                case Application.UpdateStatus.PatchFinish:
                                    result += "done.\r\n";
                                    break;
                            }
                        });

                        this.m_DualLog.WriteEntry(list[i].UpdateUrl + " has been checked for updates.\r\n\r\n" + result);
                    }
                    catch (WebException)
                    {
                        this.m_DualLog.WriteEntry("Unable to check " + list[i].UpdateUrl + " for updates.  The server is not responding.", EventLogEntryType.Warning);
                    }
                }
            }
            c.Set<ApplicationInstanceList>("instances", list);
            c.Set<bool>("locked", false);

            // Restart game if possible.
            if (restartPath != null)
                WindowsNative.LaunchPathAsUser(restartPath);
        }

        private void PerformUpdate(bool initial)
        {
            Cache c = new Cache(false);
            this.m_DualLog.WriteEntry("Periodic update check has started.");

            // Find all application instances.
            try
            {
                if (!c.Exists("instances"))
                    c.Set<ApplicationInstanceList>("instances", new ApplicationInstanceList());
                ApplicationInstanceList list = c.Get<ApplicationInstanceList>("instances");
                this.m_DualLog.WriteEntry("Application instance list retrieved with " + list.Count + " applications to check.");
                for (int i = 0; i < list.Count; i++)
                {
                    if (initial || (DateTime.Now - list[i].LastCheckTime).TotalHours >= 3)
                        this.PerformUpdateSingle(list[i].FilesystemPath);
                    else
                        this.m_DualLog.WriteEntry("The application at " + list[i].UpdateUrl + " has been checked in the last 3 hours.");
                }

                this.m_DualLog.WriteEntry("Periodic update check has finished.");
            }
            catch (Exception e)
            {
                this.m_DualLog.WriteException("Periodic update check failed with exception:", e);
            }
        }

        #endregion
    }
}
