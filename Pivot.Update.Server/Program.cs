using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using Kayak.Http;
using System.Net;

namespace Pivot.Update.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Cache c = new Cache(false);
            Console.WriteLine("Using cache stored at: " + c.GetFilePath("") + ".");
            IScheduler scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
            IServer server = KayakServer.Factory.CreateHttp(new RequestDelegate(), scheduler);

            using (server.Listen(new IPEndPoint(IPAddress.Any, 38080)))
            {
                scheduler.Start();
            }
        }
    }
}
