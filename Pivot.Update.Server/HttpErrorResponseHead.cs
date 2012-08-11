using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Http;

namespace Pivot.Update.Server
{
    public static class HttpErrorResponseHead
    {
        public static HttpResponseHead Get()
        {
            return new HttpResponseHead
            {
                Status = "400 Bad Request",
                Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "text/plain" },
                        { "Connection", "close" }
                    }
            };
        }
    }
}
