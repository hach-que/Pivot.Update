using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.IO;
using System.Xml.Serialization;

namespace Pivot.Update
{
    public class Cache
    {
        private bool m_Local;
        private SecurityIdentifier m_SID;
        private string m_Storage;

        public Cache(bool local)
        {
            this.m_Local = local;
            if (local)
            {
                this.m_SID = WindowsIdentity.GetCurrent().User;
                this.m_Storage = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            else
            {
                this.m_SID = WindowsIdentity.GetAnonymous().User;
                this.m_Storage = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }
            this.m_Storage = Path.Combine(this.m_Storage, "Pivot Cache");
            if (!Directory.Exists(this.m_Storage))
                Directory.CreateDirectory(this.m_Storage);
        }

        /// <summary>
        /// Returns whether the specified path exists in the cache.
        /// </summary>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        /// <returns>Whether the specified path exists.</returns>
        public bool Exists(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                if (i == components.Length - 1)
                {
                    DirectoryInfo[] results = current.GetDirectories(components[i]);
                    FileInfo[] fResults = current.GetFiles(components[i]);
                    if (results.Length == 0 && fResults.Length == 0)
                        return false;
                    else if (results.Length + fResults.Length > 1)
                        throw new InvalidOperationException();
                    else
                        return true;
                }
                else
                {
                    DirectoryInfo[] results = current.GetDirectories(components[i]);
                    if (results.Length == 0)
                        return false;
                    else if (results.Length > 1)
                        throw new InvalidOperationException();
                    else
                        current = results[0];
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the value stored in the cache.
        /// </summary>
        /// <typeparam name="T">The type of object stored.</typeparam>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the path does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        /// <returns>The object stored in the cache.</returns>
        public T Get<T>(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                if (i == components.Length - 1)
                {
                    FileInfo[] fResults = current.GetFiles(components[i]);
                    if (fResults.Length == 0)
                        throw new KeyNotFoundException();
                    else if (fResults.Length > 1)
                        throw new InvalidOperationException();
                    else
                    {
                        // Deserialize and return data.
                        using (StreamReader reader = new StreamReader(fResults[0].FullName))
                        {
                            if (typeof(T) == typeof(string))
                                return (T)(object)reader.ReadToEnd();
                            else
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(T));
                                return (T)serializer.Deserialize(reader);
                            }
                        }
                    }
                }
                else
                {
                    DirectoryInfo[] results = current.GetDirectories(components[i]);
                    if (results.Length == 0)
                        throw new KeyNotFoundException();
                    else if (results.Length > 1)
                        throw new InvalidOperationException();
                    else
                        current = results[0];
                }
            }

            throw new KeyNotFoundException();
        }
        
        /// <summary>
        /// Yields an enumeration listing all of the keys in the specified path.
        /// </summary>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the path does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        public IEnumerable<string> List(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                DirectoryInfo[] results = current.GetDirectories(components[i]);
                if (results.Length == 0)
                    throw new KeyNotFoundException();
                else if (results.Length > 1)
                    throw new InvalidOperationException();
                else
                    current = results[0];
            }

            foreach (FileInfo fi in current.GetFiles())
                yield return fi.Name;
        }

        /// <summary>
        /// Yields an enumeration recursively listing all of the keys in the specified path.
        /// </summary>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the path does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        public IEnumerable<string> ListRecursive(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                DirectoryInfo[] results = current.GetDirectories(components[i]);
                if (results.Length == 0)
                    throw new KeyNotFoundException();
                else if (results.Length > 1)
                    throw new InvalidOperationException();
                else
                    current = results[0];
            }

            foreach (DirectoryInfo di in current.GetDirectories())
                foreach (string s in this.ListRecursive(path + "/" + di.Name))
                    yield return di.Name + "/" + s;
            foreach (FileInfo fi in current.GetFiles())
                yield return fi.Name;
        }

        /// <summary>
        /// Returns the filesystem path to the specified key, ensuring that the path exists (except for the last component).
        /// </summary>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique, or there was not a last component in the path.</exception>
        /// <returns>The filesystem path pointing to this directory.</returns>
        public string GetFilePath(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                if (i == components.Length - 1)
                    return Path.Combine(current.FullName, components[i]);
                else
                {
                    DirectoryInfo[] results = current.GetDirectories(components[i]);
                    if (results.Length == 0)
                        current = current.CreateSubdirectory(components[i]);
                    else if (results.Length > 1)
                        throw new InvalidOperationException();
                    else
                        current = results[0];
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns the filesystem path to the specified key, ensuring that the path exists.
        /// </summary>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        /// <returns>The filesystem path pointing to this directory.</returns>
        public string GetDirectoryPath(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                DirectoryInfo[] results = current.GetDirectories(components[i]);
                if (results.Length == 0)
                    current = current.CreateSubdirectory(components[i]);
                else if (results.Length > 1)
                    throw new InvalidOperationException();
                else
                    current = results[0];
            }

            return current.FullName;
        }

        /// <summary>
        /// Sets a value in the cache.
        /// </summary>
        /// <typeparam name="T">The type of object to store.</typeparam>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the any component of the path other than the last, does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        public void Set<T>(string path, T value)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                if (i == components.Length - 1)
                {
                    FileInfo[] fResults = current.GetFiles(components[i]);
                    if (fResults.Length > 1)
                        throw new InvalidOperationException();
                    else
                    {
                        // Serialize and store data.
                        string valuePath;
                        if (fResults.Length > 0)
                            valuePath = fResults[0].FullName;
                        else
                            valuePath = Path.Combine(current.FullName, components[i]);
                        using (StreamWriter writer = new StreamWriter(valuePath))
                        {
                            if (typeof(T) == typeof(string))
                                writer.Write(value);
                            else
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(T));
                                serializer.Serialize(writer, value);
                            }
                            return;
                        }
                    }
                }
                else
                {
                    DirectoryInfo[] results = current.GetDirectories(components[i]);
                    if (results.Length == 0)
                        current = current.CreateSubdirectory(components[i]);
                    else if (results.Length > 1)
                        throw new InvalidOperationException();
                    else
                        current = results[0];
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Deletes a value from the cache.
        /// </summary>
        /// <param name="path">A unique path seperated by /, used as a key in the cache.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the path does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the path is not unique.</exception>
        public void Delete(string path)
        {
            string[] components = path.Trim('/').Split('/');

            DirectoryInfo current = new DirectoryInfo(this.m_Storage);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].Trim() == "")
                    continue;
                if (i == components.Length - 1)
                {
                    FileInfo[] fResults = current.GetFiles(components[i]);
                    if (fResults.Length > 1)
                        throw new InvalidOperationException();
                    else
                    {
                        File.Delete(fResults[0].FullName);
                        return;
                    }
                }
                else
                {
                    DirectoryInfo[] results = current.GetDirectories(components[i]);
                    if (results.Length == 0)
                        throw new KeyNotFoundException();
                    else if (results.Length > 1)
                        throw new InvalidOperationException();
                    else
                        current = results[0];
                }
            }

            throw new InvalidOperationException();
        }
    }
}
