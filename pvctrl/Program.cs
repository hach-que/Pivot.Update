using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Pivot.Update.Patching;

namespace Pivot.Update.Server.Control
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
                case "create":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("must have at least 2 arguments (create <appname>).");
                        return;
                    }

                    string appname = args[1];
                    Cache c = new Cache(false);

                    if (c.Exists("server/" + appname))
                    {
                        Console.WriteLine("this app is already created.");
                        return;
                    }

                    foreach (string s in GetRecursiveFilesInCwd())
                    {
                        Hash hash = Hash.FromFile(Environment.CurrentDirectory + "\\" + s);
                        Console.WriteLine(Hash.Empty.ToString() + " => " + hash.ToString() + " " + s);
                        c.Set<Hash>("server/" + appname + "/hashes/" + s, hash);
                        if (c.Exists("server/" + appname + "/store/" + s))
                            c.Delete("server/" + appname + "/store/" + s);
                        File.Copy(Environment.CurrentDirectory + "\\" + s, c.GetFilePath("server/" + appname + "/store/" + s));
                    }

                    return;
                case "flash":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("must have at least 2 arguments (flash <appname>).");
                        return;
                    }

                    appname = args[1];
                    c = new Cache(false);

                    if (!c.Exists("server/" + appname))
                    {
                        Console.WriteLine("use create to create this app first.");
                        return;
                    }
                    
                    // Find all files in the current working directory.
                    diff_match_patch dmf = new diff_match_patch();
                    IEnumerable<string> envs = GetRecursiveFilesInCwd();
                    foreach (string s in envs)
                    {
                        if (c.Exists("server/" + appname + "/store/" + s))
                        {
                            // File is being updated, get a hash for the current version
                            // and for the stored version.
                            Hash oldHash = Hash.FromFile(c.GetFilePath("server/" + appname + "/store/" + s));
                            Hash newHash = Hash.FromFile(Environment.CurrentDirectory + "\\" + s);
                            if (oldHash != newHash)
                            {
                                // Files are different, produce a diff.
                                byte[] oldData = FileUtils.GetAllBytes(c.GetFilePath("server/" + appname + "/store/" + s));
                                byte[] newData = FileUtils.GetAllBytes(Environment.CurrentDirectory + "\\" + s);
                                List<Patch> patches = dmf.patch_make(Encoding.ASCII.GetString(oldData), Encoding.ASCII.GetString(newData));
                                string result = dmf.patch_toText(patches);
                                c.Set<string>("server/" + appname + "/patches/" + s + "/" + oldHash + "-" + newHash, result);
                                c.Set<Hash>("server/" + appname + "/hashes/" + s, newHash);
                                if (c.Exists("server/" + appname + "/store/" + s))
                                    c.Delete("server/" + appname + "/store/" + s);
                                File.Copy(Environment.CurrentDirectory + "\\" + s, c.GetFilePath("server/" + appname + "/store/" + s));
                                Console.WriteLine(oldHash + " => " + newHash + " " + s);
                            }
                        }
                        else
                        {
                            // A new file is being stored.
                            Hash newHash = Hash.FromFile(Environment.CurrentDirectory + "\\" + s);
                            byte[] newData = FileUtils.GetAllBytes(Environment.CurrentDirectory + "\\" + s);
                            List<Patch> patches = dmf.patch_make("", Encoding.ASCII.GetString(newData));
                            string result = dmf.patch_toText(patches);
                            c.Set<string>("server/" + appname + "/patches/" + s + "/" + Hash.Empty + "-" + newHash, result);
                            c.Set<Hash>("server/" + appname + "/hashes/" + s, newHash);
                            if (c.Exists("server/" + appname + "/store/" + s))
                                c.Delete("server/" + appname + "/store/" + s);
                            File.Copy(Environment.CurrentDirectory + "\\" + s, c.GetFilePath("server/" + appname + "/store/" + s));
                            Console.WriteLine(Hash.Empty + " => " + newHash + " " + s);
                        }
                    }
                    foreach (string s in c.ListRecursive("server/" + appname + "/store"))
                    {
                        if (!envs.Contains(s))
                        {
                            // A file is being deleted.
                            Hash oldHash = Hash.FromFile(c.GetFilePath("server/" + appname + "/store/" + s));
                            byte[] oldData = FileUtils.GetAllBytes(c.GetFilePath("server/" + appname + "/store/" + s));
                            List<Patch> patches = dmf.patch_make(Encoding.ASCII.GetString(oldData), "");
                            string result = dmf.patch_toText(patches);
                            c.Set<string>("server/" + appname + "/patches/" + s + "/" + oldHash.ToString() + "-" + Hash.Empty.ToString(), result);
                            c.Set<Hash>("server/" + appname + "/hashes/" + s, Hash.Empty);
                            if (c.Exists("server/" + appname + "/store/" + s))
                                c.Delete("server/" + appname + "/store/" + s);
                            Console.WriteLine(oldHash + " => " + Hash.Empty + " " + s);
                        }
                    }

                    Console.WriteLine("flash complete.");
                    return;
                case "state":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("must have at least 2 arguments (state <appname>).");
                        return;
                    }

                    appname = args[1];
                    c = new Cache(false);

                    if (!c.Exists("server/" + appname))
                    {
                        Console.WriteLine("this app does not exist.");
                        return;
                    }

                    foreach (string s in c.ListRecursive("server/" + appname + "/hashes"))
                    {
                        Console.WriteLine(c.Get<Hash>("server/" + appname + "/hashes/" + s) + " " + s);
                    }

                    return;
                default:
                    Console.WriteLine("invalid command (must be 'flash' or 'create').");
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
