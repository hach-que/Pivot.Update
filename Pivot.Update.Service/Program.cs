using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;

namespace Pivot.Update.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static int Main(string[] args)
        {
            bool install = false, uninstall = false, console = false, rethrow = false;
            try
            {
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "-i":
                        case "-install":
                            install = true; break;
                        case "-u":
                        case "-uninstall":
                            uninstall = true; break;
                        case "-c":
                        case "-console":
                            console = true; break;
                        default:
                            Console.Error.WriteLine("Argument not expected: " + arg);
                            break;
                    }
                }

                if (uninstall)
                {
                    Install(true, args);
                }
                if (install)
                {
                    Install(false, args);
                    ServiceController service = new ServiceController("Pivot Update Service");
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromMilliseconds(3000);
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                        return 0;
                    }
                    catch
                    {
                        return -1;
                    }
                }
                if (console)
                {
                    Console.WriteLine("Update service running; press any key to stop.");
                    ConsoleInit();
                    m_Manager.Start();
                    Console.ReadKey(true);
                    m_Manager.Stop();
                    Console.WriteLine("Update service stopped.");
                }
                else if (!(install || uninstall))
                {
                    rethrow = true; // so that windows sees error...
                    ServiceBase[] services = { new UpdateService() };
                    ServiceBase.Run(services);
                    rethrow = false;
                }
                return 0;
            }
            catch (Exception ex)
            {
                if (rethrow) throw;
                Console.Error.WriteLine(ex.Message);
                return -1;
            }
        }

        private static Manager m_Manager;
        private static EventLog c_EventLog;

        static void ConsoleInit()
        {
            if (!System.Diagnostics.EventLog.SourceExists("Pivot Update Service"))
            {
                System.Diagnostics.EventLog.CreateEventSource("Pivot Update Service", "Application");
            }
            c_EventLog = new EventLog();
            c_EventLog.Source = "Pivot Update Service";
            c_EventLog.Log = "Application";
            m_Manager = new Manager(new DualLog(c_EventLog));
        }

        static void Install(bool undo, string[] args)
        {
            try
            {
                Console.WriteLine(undo ? "uninstalling" : "installing");
                using (AssemblyInstaller inst = new AssemblyInstaller(typeof(Program).Assembly, args))
                {
                    IDictionary state = new Hashtable();
                    inst.UseNewContext = true;
                    try
                    {
                        if (undo)
                        {
                            inst.Uninstall(state);
                        }
                        else
                        {
                            inst.Install(state);
                            inst.Commit(state);
                        }
                    }
                    catch
                    {
                        try
                        {
                            inst.Rollback(state);
                        }
                        catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }
    }
}
