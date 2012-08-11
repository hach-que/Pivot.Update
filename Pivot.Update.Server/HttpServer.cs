using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Pivot.Update.Server
{
    public class HttpServer
    {
        private HttpListener m_Listener;

        public HttpServer()
        {
            this.m_Listener = new HttpListener();
            this.m_Listener.Start();
        }
    }
}
