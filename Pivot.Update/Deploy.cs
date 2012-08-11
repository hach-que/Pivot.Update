using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.IO;
using Pivot.Update;
using System.Diagnostics;
using System.Threading;

namespace Pivot.Update
{
    public static class Deploy
    {
        public static bool IsDeployed
        {
            get
            {
                ServiceController ctl = ServiceController.GetServices().Where(s => s.ServiceName == "Pivot Update Service").FirstOrDefault();
                return (ctl != null);
            }
        }

        public static bool PerformDeploy()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                path = Path.Combine(path, "Pivot Update Service");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                Application a = new Application(new Cache(false), new Uri("http://update.redpointsoftware.com.au/pivot.update/"), path);
                a.Update((status, info) => { });
                string exe = Path.Combine(path, "Pivot.Update.Service.exe");
                if (!File.Exists(exe))
                    return false;
                if (!UAC.IsAdmin())
                {
                    UAC.RunElevated(exe, "-i");
                    Thread.Sleep(4000);
                    return IsDeployed;
                }
                else
                {
                    Process p = Process.Start(exe, "-i");
                    p.WaitForExit();
                    return IsDeployed;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
