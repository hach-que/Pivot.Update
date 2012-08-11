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
    public class GetPatchesOperation : IOperation
    {
        #region IOperation Members

        public bool Handles(string query)
        {
            return (query == "patch");
        }

        public void Handle(string appname, string[] components, HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response)
        {
            // Look inside the cache for the specified file.
            Cache c = new Cache(false);
            string path = HttpUtility.UrlDecode(components.Where((value, row) => row >= 2).Aggregate((a, b) => a + "/" + b));
            if (!c.Exists("server/" + appname + "/store/" + path))
            {
                response.OnResponse(HttpErrorResponseHead.Get(), new HttpErrorDataProducer());
                return;
            }

            // Calculate patch path from source to destination.
            string result = "";
            Hash source = Hash.FromString(components[0]);
            Hash destination = Hash.FromString(components[1]);

            while (source != destination)
            {
                // Find the patch in the patches that will turn source
                // into the next patch.
                IEnumerable<string> patches = c.List("server/" + appname + "/patches/" + path).Where(v => v.StartsWith(source.ToString() + "-"));
                if (patches.Count() != 1)
                {
                    response.OnResponse(HttpErrorResponseHead.Get(), new HttpErrorDataProducer());
                    return;
                }
                string next = patches.First();
                source = Hash.FromString(next.Substring((source.ToString() + "-").Length));
                using (StreamReader reader = new StreamReader(c.GetFilePath("server/" + appname + "/patches/" + path + "/" + next)))
                {
                    result += "--- NEXT PATCH (" + reader.BaseStream.Length + ") ---\r\n";
                    result += reader.ReadToEnd();
                    result += "\r\n";
                }
            }
            result += "--- END OF PATCHES ---\r\n";

            // Return data.
            response.OnResponse(new HttpResponseHead()
            {
                Status = "200 OK",
                Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", result.Length.ToString() },
                        { "Connection", "close" }
                    }
            }, new BufferedProducer(result));
        }

        #endregion
    }
}
