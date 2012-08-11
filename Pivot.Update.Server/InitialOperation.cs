using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using Kayak.Http;

namespace Pivot.Update.Server
{
    public class InitialOperation : IOperation
    {
        #region IOperation Members

        public bool Handles(string query)
        {
            return (query == "initial");
        }

        public void Handle(string appname, string[] components, HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
        {
            // Look inside the cache for a list of files.
            Cache c = new Cache(false);
            string s = "";
            foreach (string key in c.ListRecursive("server/" + appname + "/hashes"))
                s += c.Get<Hash>("server/" + appname + "/hashes/" + key) + " " + key + "\r\n";
            response.OnResponse(new HttpResponseHead()
            {
                Status = "200 OK",
                Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", s.Length.ToString() },
                        { "Connection", "close" }
                    }
            }, new BufferedProducer(s));
        }

        #endregion
    }
}
