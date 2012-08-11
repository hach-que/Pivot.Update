using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pivot.Update
{
    internal class InterProcess : MarshalByRefObject
    {
        public static readonly string CHANNEL_NAME = "pivotupdate";

        public static event ApplicationInstanceEventHandler RegisterRequestedEvent;
        public static event ApplicationInstanceEventHandler HasUpdatesRequestedEvent;
        public static event ApplicationInstanceEventHandler ScheduledUpdateRequestedEvent;

        public bool Register(string uri, string path)
        {
            if (RegisterRequestedEvent != null)
                return RegisterRequestedEvent(this, new ApplicationInstanceEventArgs(uri, path, 0, null));
            return false;
        }

        public bool HasUpdates(string path)
        {
            if (HasUpdatesRequestedEvent != null)
                return HasUpdatesRequestedEvent(this, new ApplicationInstanceEventArgs(null, path, 0, null));
            return false;
        }

        public bool ScheduleUpdate(string path, int time, string restartPath = null)
        {
            if (ScheduledUpdateRequestedEvent != null)
                return ScheduledUpdateRequestedEvent(this, new ApplicationInstanceEventArgs(null, path, time, restartPath));
            return false;
        }
    }

    internal delegate bool ApplicationInstanceEventHandler(object sender, ApplicationInstanceEventArgs e);

    internal class ApplicationInstanceEventArgs : EventArgs
    {
        public ApplicationInstanceEventArgs(string uri, string path, int time, string restartPath)
        {
            this.UpdateURI = uri;
            this.LocalPath = path;
            this.Time = time;
        }

        public string LocalPath
        {
            get;
            private set;
        }

        public string UpdateURI
        {
            get;
            private set;
        }

        public int Time
        {
            get;
            private set;
        }

        public string RestartPath
        {
            get;
            private set;
        }
    }
}
