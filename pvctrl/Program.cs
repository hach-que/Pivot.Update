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
                    {
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

                        if (!File.Exists(".pvdeploy"))
                        {
                            Console.WriteLine("no .pvdeploy file found; please create one.");
                            return;
                        }

                        FileFilter filter = FileFilterParser.Parse(".pvdeploy", GetRecursiveFilesInCwd());
                        foreach (KeyValuePair<string, string> kv in filter)
                        {
                            // Key is original filename.
                            // Value is filename to store as.
                            Hash hash = Hash.FromFile(Path.Combine(Environment.CurrentDirectory, kv.Key));
                            Console.WriteLine(Hash.Empty.ToString() + " => " + hash.ToString() + " " + kv.Value);
                            c.Set<Hash>("server/" + appname + "/hashes/" + kv.Value, hash);
                            if (c.Exists("server/" + appname + "/store/" + kv.Value))
                                c.Delete("server/" + appname + "/store/" + kv.Value);
                            File.Copy(Path.Combine(Environment.CurrentDirectory, kv.Key), c.GetFilePath("server/" + appname + "/store/" + kv.Value));
                        }

                        return;
                    }
                case "test-pvdeploy":
                    {
                        if (args.Length < 1)
                        {
                            Console.WriteLine("must have at least 1 argument (test-pvdeploy).");
                            return;
                        }

                        if (!File.Exists(".pvdeploy"))
                        {
                            Console.WriteLine("no .pvdeploy file found; please create one.");
                            return;
                        }

                        FileFilter filter = FileFilterParser.Parse(".pvdeploy", GetRecursiveFilesInCwd());
                        foreach (KeyValuePair<string, string> kv in filter)
                        {
                            // Key is original filename.
                            // Value is filename to store as.
                            Console.WriteLine(kv.Key + " => " + kv.Value);
                        }

                        return;
                    }
                case "flash":
                    {
                        if (args.Length < 2)
                        {
                            Console.WriteLine("must have at least 2 arguments (flash <appname>).");
                            return;
                        }

                        string appname = args[1];
                        Cache c = new Cache(false);

                        if (!c.Exists("server/" + appname))
                        {
                            Console.WriteLine("use create to create this app first.");
                            return;
                        }

                        if (!File.Exists(".pvdeploy"))
                        {
                            Console.WriteLine("no .pvdeploy file found; please create one.");
                            return;
                        }

                        // Find all files in the current working directory.
                        diff_match_patch dmf = new diff_match_patch();
                        FileFilter filter = FileFilterParser.Parse(".pvdeploy", GetRecursiveFilesInCwd());
                        foreach (KeyValuePair<string, string> kv in filter)
                        {
                            if (c.Exists("server/" + appname + "/store/" + kv.Value))
                            {
                                // File is being updated, get a hash for the current version
                                // and for the stored version.
                                Hash oldHash = Hash.FromFile(c.GetFilePath("server/" + appname + "/store/" + kv.Value));
                                Hash newHash = Hash.FromFile(Path.Combine(Environment.CurrentDirectory, kv.Key));
                                if (oldHash != newHash)
                                {
                                    // Files are different, produce a diff.
                                    byte[] oldData = FileUtils.GetAllBytes(c.GetFilePath("server/" + appname + "/store/" + kv.Value));
                                    byte[] newData = FileUtils.GetAllBytes(Path.Combine(Environment.CurrentDirectory, kv.Key));
                                    List<Patch> patches = dmf.patch_make(Encoding.ASCII.GetString(oldData), Encoding.ASCII.GetString(newData));
                                    string result = dmf.patch_toText(patches);
                                    c.Set<string>("server/" + appname + "/patches/" + kv.Value + "/" + oldHash + "-" + newHash, result);
                                    c.Set<Hash>("server/" + appname + "/hashes/" + kv.Value, newHash);
                                    if (c.Exists("server/" + appname + "/store/" + kv.Value))
                                        c.Delete("server/" + appname + "/store/" + kv.Value);
                                    File.Copy(Path.Combine(Environment.CurrentDirectory, kv.Key), c.GetFilePath("server/" + appname + "/store/" + kv.Value));
                                    if (!c.Exists("server/" + appname + "/store/" + kv.Value))
                                        throw new InvalidOperationException("Unable to copy file to server store.");
                                    Console.WriteLine(oldHash + " => " + newHash + " " + kv.Value);
                                }
                            }
                            else
                            {
                                // A new file is being stored.
                                Hash newHash = Hash.FromFile(Path.Combine(Environment.CurrentDirectory, kv.Key));
                                byte[] newData = FileUtils.GetAllBytes(Path.Combine(Environment.CurrentDirectory, kv.Key));
                                List<Patch> patches = dmf.patch_make("", Encoding.ASCII.GetString(newData));
                                string result = dmf.patch_toText(patches);
                                c.Set<string>("server/" + appname + "/patches/" + kv.Value + "/" + Hash.Empty + "-" + newHash, result);
                                c.Set<Hash>("server/" + appname + "/hashes/" + kv.Value, newHash);
                                if (c.Exists("server/" + appname + "/store/" + kv.Value))
                                    c.Delete("server/" + appname + "/store/" + kv.Value);
                                File.Copy(Path.Combine(Environment.CurrentDirectory, kv.Key), c.GetFilePath("server/" + appname + "/store/" + kv.Value));
                                if (!c.Exists("server/" + appname + "/store/" + kv.Value))
                                    throw new InvalidOperationException("Unable to copy file to server store.");
                                Console.WriteLine(Hash.Empty + " => " + newHash + " " + kv.Value);
                            }
                        }
                        foreach (string s in c.ListRecursive("server/" + appname + "/store"))
                        {
                            if (filter.Count(v => v.Value == s) == 0)
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
                                if (c.Exists("server/" + appname + "/store/" + s))
                                    throw new InvalidOperationException("Unable to delete file from server store.");
                                Console.WriteLine(oldHash + " => " + Hash.Empty + " " + s);
                            }
                        }

                        Console.WriteLine("flash complete.");
                        return;
                    }
                case "state":
                    {
                        if (args.Length < 2)
                        {
                            Console.WriteLine("must have at least 2 arguments (state <appname>).");
                            return;
                        }

                        string appname = args[1];
                        Cache c = new Cache(false);

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
                    }
                default:
                    Console.WriteLine("invalid command (must be 'flash', 'create', 'state' or 'test-pvdeploy').");
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
                    yield return (di.Name + "/" + s).Trim('/');
            foreach (FileInfo fi in current.GetFiles())
                yield return fi.Name;
        }
    }
}
