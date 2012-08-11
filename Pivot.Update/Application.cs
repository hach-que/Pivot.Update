using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pivot.Update.Patching;
using System.Net;
using System.IO;

namespace Pivot.Update
{
    internal class Application
    {
        private Uri m_UpdateUri;
        private string m_LocalPath;

        /// <summary>
        /// Creates a new application instance with the specified update URI
        /// and the specified local path.
        /// </summary>
        /// <param name="update">The URI to the update server.</param>
        /// <param name="localPath">The local path.</param>
        public Application(Cache c, Uri update, string localPath)
        {
            this.m_UpdateUri = update;
            this.m_LocalPath = localPath;
        }

        /// <summary>
        /// The current update operation status.
        /// </summary>
        public enum UpdateStatus
        {
            Starting,
            RetrievingFileList,
            RetrievedFileList,
            DeterminingOperation,
            DeletionStart,
            DeletionFinish,
            PatchStart,
            PatchApplied,
            PatchFinish,
            DownloadNewStart,
            DownloadNewFinish,
            DownloadFreshStart,
            DownloadFreshFinish,
            Complete
        }

        /// <summary>
        /// Updates the application.
        /// </summary>
        public void Update(Action<UpdateStatus, object> callback)
        {
            callback(UpdateStatus.Starting, null);
            diff_match_patch dmf = new diff_match_patch();
            WebClient client = new WebClient();
            callback(UpdateStatus.RetrievingFileList, null);
            string state = client.DownloadString(this.m_UpdateUri + "/initial");
            string[] lines = state.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            callback(UpdateStatus.RetrievedFileList, lines.Length);
            int opers = 0;
            foreach (string line in lines)
            {
                Hash hash = Hash.FromString(line.Split(new char[] { ' ' }, 2)[0]);
                string file = line.Split(new char[] { ' ' }, 2)[1];
                callback(UpdateStatus.DeterminingOperation, file);

                // See if file already exists.
                if (File.Exists(Path.Combine(this.m_LocalPath, file)) || hash == Hash.Empty)
                {
                    if (hash == Hash.Empty)
                    {
                        // Delete the local file if it exists.
                        if (File.Exists(Path.Combine(this.m_LocalPath, file)))
                        {
                            callback(UpdateStatus.DeletionStart, file);
                            File.Delete(Path.Combine(this.m_LocalPath, file));
                            callback(UpdateStatus.DeletionFinish, file);
                            opers++;
                        }
                    }
                    else
                    {
                        // Compare hashes, ignore if same.
                        Hash current = Hash.FromFile(Path.Combine(this.m_LocalPath, file));
                        if (current == hash)
                            continue;

                        // Download patch.
                        string patchstream;
                        try
                        {
                            patchstream = client.DownloadString(this.m_UpdateUri + "/patch/" + current + "/" + hash + "/" + file);
                            callback(UpdateStatus.PatchStart, file);
                            int a = 1;
                            using (StringReader reader = new StringReader(patchstream))
                            {
                                while (true)
                                {
                                    string header = reader.ReadLine();
                                    if (header == "--- END OF PATCHES ---")
                                        break;
                                    else if (header.StartsWith("--- NEXT PATCH ("))
                                    {
                                        int count = Convert.ToInt32(header.Substring("--- NEXT PATCH (".Length, header.Length - "--- NEXT PATCH () ---".Length));
                                        char[] data = new char[count];
                                        reader.ReadBlock(data, 0, count);
                                        List<Patch> patches = dmf.patch_fromText(new string(data));
                                        string newText;
                                        using (StreamReader stream = new StreamReader(Path.Combine(this.m_LocalPath, file)))
                                        {
                                            newText = (string)dmf.patch_apply(patches, stream.ReadToEnd())[0];
                                        }
                                        using (StreamWriter stream = new StreamWriter(Path.Combine(this.m_LocalPath, file)))
                                        {
                                            stream.Write(newText);
                                        }
                                        callback(UpdateStatus.PatchApplied, a);
                                        a++;
                                        // Read the empty terminator line.
                                        reader.ReadLine();
                                    }
                                    else
                                        throw new DataMisalignedException();
                                }
                            }
                            callback(UpdateStatus.PatchFinish, file);
                            opers++;
                        }
                        catch (WebException)
                        {
                            // There's no patch list for this file, redownload.
                            string[] coms = file.Split('/');
                            DirectoryInfo di = new DirectoryInfo(this.m_LocalPath);
                            for (int i = 0; i < coms.Length - 1; i++)
                            {
                                DirectoryInfo[] dis = di.GetDirectories(coms[i]);
                                if (dis.Length == 0)
                                    di = di.CreateSubdirectory(coms[i]);
                                else
                                    di = dis[0];
                            }
                            callback(UpdateStatus.DownloadFreshStart, file);
                            client.DownloadFile(this.m_UpdateUri + "/file/" + file, Path.Combine(this.m_LocalPath, file));
                            callback(UpdateStatus.DownloadFreshFinish, file);
                            opers++;
                        }
                    }
                }
                else
                {
                    // File does not exist, download fresh.
                    string[] coms = file.Split('/');
                    DirectoryInfo di = new DirectoryInfo(this.m_LocalPath);
                    for (int i = 0; i < coms.Length - 1; i++)
                    {
                        DirectoryInfo[] dis = di.GetDirectories(coms[i]);
                        if (dis.Length == 0)
                            di = di.CreateSubdirectory(coms[i]);
                        else
                            di = dis[0];
                    }
                    callback(UpdateStatus.DownloadNewStart, file);
                    client.DownloadFile(this.m_UpdateUri + "/file/" + file, Path.Combine(this.m_LocalPath, file));
                    callback(UpdateStatus.DownloadNewFinish, file);
                    opers++;
                }
            }
            callback(UpdateStatus.Complete, opers);

            return;
        }

        /// <summary>
        /// Checks the application to see if there are updates.
        /// </summary>
        public bool Check()
        {
            diff_match_patch dmf = new diff_match_patch();
            WebClient client = new WebClient();
            string state = client.DownloadString(this.m_UpdateUri + "/initial");
            string[] lines = state.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int opers = 0;
            foreach (string line in lines)
            {
                Hash hash = Hash.FromString(line.Split(new char[] { ' ' }, 2)[0]);
                string file = line.Split(new char[] { ' ' }, 2)[1];

                // See if file already exists.
                if (File.Exists(Path.Combine(this.m_LocalPath, file)) || hash == Hash.Empty)
                {
                    if (hash == Hash.Empty)
                    {
                        // File needs to be deleted.
                        if (File.Exists(Path.Combine(this.m_LocalPath, file)))
                            opers++;
                    }
                    else
                    {
                        // Compare hashes, ignore if same.
                        Hash current = Hash.FromFile(Path.Combine(this.m_LocalPath, file));
                        if (current == hash)
                            continue;

                        // File needs to be patched.
                        opers++;
                    }
                }
                else
                {
                    // File needs to be downloaded.
                    opers++;
                }
            }

            return opers > 0;
        }
    }
}
