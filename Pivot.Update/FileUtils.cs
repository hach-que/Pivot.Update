using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pivot.Update
{
    public class FileUtils
    {
        public static byte[] GetAllBytes(string path)
        {
            List<byte> bytes = new List<byte>();
            using (StreamReader stream = new StreamReader(path))
            {
                using (BinaryReader reader = new BinaryReader(stream.BaseStream))
                {
                    bytes.AddRange(reader.ReadBytes(4096));
                }
            }
            return bytes.ToArray();
        }
    }
}
