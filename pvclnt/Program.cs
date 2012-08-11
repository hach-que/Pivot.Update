using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Pivot.Update.Patching;
using System.Threading;

namespace Pivot.Update.Client.Control
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("must have at least 1 argument.");
                return;
            }

            switch (args[0])
            {
                case "update-local":
                    {
                        if (args.Length < 2)
                        {
                            Console.WriteLine("must have at least 2 arguments (patch <uri>).");
                            return;
                        }

                        Uri uri = new Uri(args[1]);
                        Cache c = new Cache(true);

                        Application a = new Application(c, uri, Environment.CurrentDirectory);
                        int opers = 0;
                        a.Update((status, info) =>
                            {
                                switch (status)
                                {
                                    case Application.UpdateStatus.RetrievingFileList:
                                        Console.Write("requesting current state.. ");
                                        break;
                                    case Application.UpdateStatus.RetrievedFileList:
                                        Console.WriteLine(info + " files to scan.");
                                        break;
                                    case Application.UpdateStatus.DeletionStart:
                                        Console.Write("deleting " + info + ".. ");
                                        break;
                                    case Application.UpdateStatus.PatchStart:
                                        Console.Write("patching " + info + ".. ");
                                        break;
                                    case Application.UpdateStatus.PatchApplied:
                                        Console.Write(a + ". ");
                                        break;
                                    case Application.UpdateStatus.DownloadNewStart:
                                    case Application.UpdateStatus.DownloadFreshStart:
                                        Console.Write("downloading " + info + ".. ");
                                        break;
                                    case Application.UpdateStatus.DeletionFinish:
                                    case Application.UpdateStatus.PatchFinish:
                                    case Application.UpdateStatus.DownloadNewFinish:
                                    case Application.UpdateStatus.DownloadFreshFinish:
                                        Console.WriteLine("done.");
                                        opers++;
                                        break;
                                    case Application.UpdateStatus.Complete:
                                        Console.WriteLine("patch complete (" + opers + " operations).");
                                        break;
                                }
                            });

                        return;
                    }
                case "check-local":
                    {
                        if (args.Length < 2)
                        {
                            Console.WriteLine("must have at least 2 arguments (patch <uri>).");
                            return;
                        }

                        Uri uri = new Uri(args[1]);
                        Cache c = new Cache(true);

                        Application a = new Application(c, uri, Environment.CurrentDirectory);
                        if (a.Check())
                            Console.WriteLine("there are updates available.");
                        else
                            Console.WriteLine("there are no updates.");

                        return;
                    }
                case "register":
                    {
                        if (args.Length < 2)
                        {
                            Console.WriteLine("must have at least 2 arguments (register <uri>).");
                            return;
                        }
                        
                        Uri uri = new Uri(args[1]);
                        if (API.Register(uri))
                            Console.WriteLine("registration complete.");
                        else
                            Console.WriteLine("registration failed.");

                        return;
                    }
                case "check":
                    {
                        if (args.Length != 1)
                        {
                            Console.WriteLine("must have 1 argument (check).");
                            return;
                        }

                        if (API.HasUpdates())
                            Console.WriteLine("there are updates available.");
                        else
                            Console.WriteLine("there are no updates.");

                        return;
                    }
                case "update":
                    {
                        if (args.Length != 1)
                        {
                            Console.WriteLine("must have 1 argument (update).");
                            return;
                        }

                        if (API.ScheduleUpdate(0))
                            Console.WriteLine("an update has been scheduled.");
                        else
                            Console.WriteLine("the update could not be scheduled.");

                        return;
                    }
                default:
                    Console.WriteLine("invalid command (must be 'update', 'check', 'register', 'update-local' or 'check-local').");
                    return;
            }
        }

        static IEnumerable<string> GetRecursiveFilesInCwd(string path = null)
        {
            if (path == null)
                path = Environment.CurrentDirectory;
            DirectoryInfo current = new DirectoryInfo(path);

            foreach (DirectoryInfo di in current.GetDirectories())
                foreach (string s in GetRecursiveFilesInCwd(path + "/" + di.Name))
                    yield return (path.Substring(Environment.CurrentDirectory.Length).Replace("\\", "/").Trim('/') + "/" + di.Name + "/" + s).Trim('/');
            foreach (FileInfo fi in current.GetFiles())
                yield return fi.Name;
        }
    }
}
