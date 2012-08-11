using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace Pivot.Update
{
    [Serializable]
    public class Hash : IXmlSerializable
    {
        private byte[] m_Bytes = null;

        private Hash(byte[] bytes)
        {
            if (bytes.Length != 20)
                throw new ArgumentOutOfRangeException("bytes");
            this.m_Bytes = bytes;
        }

        public static Hash Empty
        {
            get
            {
                return new Hash(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            }
        }

        public static Hash FromFile(string path)
        {
            SHA1 sha = new SHA1Managed();
            byte[] arr = FileUtils.GetAllBytes(path);
            byte[] result = sha.ComputeHash(arr);
            return new Hash(result);
        }

        public static Hash FromString(string str)
        {
            if (str.Length != 40)
                throw new ArgumentOutOfRangeException("str");
            byte[] bytes = new byte[20];
            for (int i = 0; i < str.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }
            return new Hash(bytes);
        }

        public override string ToString()
        {
            return BitConverter.ToString(this.m_Bytes).Replace("-", string.Empty);
        }

        public static bool operator ==(Hash a, Hash b)
        {
            if (object.ReferenceEquals(a, b))
                return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;
            for (int i = 0; i < a.m_Bytes.Length; i++)
                if (b.m_Bytes[i] != a.m_Bytes[i])
                    return false;
            return true;
        }

        public static bool operator !=(Hash a, Hash b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
                return false;
            if (!(obj is Hash))
                return false;
            return (obj as Hash == this);
        }

        #region IXmlSerializable Members

        private Hash()
        {
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            Hash other = Hash.FromString(reader.ReadElementContentAsString());
            this.m_Bytes = other.m_Bytes;
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteString(this.ToString());
        }

        #endregion
    }
}
