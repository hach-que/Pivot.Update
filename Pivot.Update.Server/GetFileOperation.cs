using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using Kayak.Http;
using System.IO;
using System.Web;

namespace Pivot.Update.Server
{
    public class GetFileOperation : IOperation
    {
        #region IOperation Members

        public bool Handles(string query)
        {
            return (query == "file");
        }

        public void Handle(string appname, string[] components, HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
        {
            // Look inside the cache for the specified file.
            Cache c = new Cache(false);
            string path = HttpUtility.UrlDecode(components.Aggregate((a, b) => a + "/" + b));
            if (!c.Exists("server/" + appname + "/store/" + path))
            {
                response.OnResponse(HttpErrorResponseHead.Get(), new HttpErrorDataProducer());
                return;
            }

            response.OnResponse(new HttpResponseHead()
            {
                Status = "200 OK",
                Headers = new Dictionary<string, string>
                {
                    { "Connection", "close" }
                }
            }, new StreamReaderProducer(c.GetFilePath("server/" + appname + "/store/" + path)));
        }

        #endregion
    }
}
