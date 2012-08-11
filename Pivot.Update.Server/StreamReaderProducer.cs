using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using System.IO;

namespace Pivot.Update.Server
{
    class StreamReaderProducer : IDataProducer
    {
        private string m_Path;

        public StreamReaderProducer(string path)
        {
            this.m_Path = path;
        }

        public IDisposable Connect(IDataConsumer channel)
        {
            // null continuation, consumer must swallow the data immediately.
            byte[] storage = new byte[4096];
            using (FileStream stream = new FileStream(this.m_Path, FileMode.Open, FileAccess.Read))
            {
                while (stream.Position < stream.Length)
                {
                    int count = stream.Read(storage, 0, 4096);
                    channel.OnData(new ArraySegment<byte>(storage, 0, count), null);
                }
                channel.OnEnd();
            }
            return null;
        }
    }
}
