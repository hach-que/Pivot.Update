using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Http;
using Kayak;

namespace Pivot.Update.Server
{
    public class RequestDelegate : IHttpRequestDelegate
    {
        private List<IOperation> m_Operations;

        public RequestDelegate()
        {
            this.m_Operations = new List<IOperation>
            {
                new InitialOperation(),
                new GetFileOperation(),
                new GetPatchesOperation(),
                new GetZipOperation()
            };
        }

        #region IHttpRequestDelegate Members

        public void OnRequest(HttpRequestHead head, Kayak.IDataProducer body, IHttpResponseDelegate response)
        {
            string[] components = head.Uri.Trim('/').Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length <= 1)
            {
                response.OnResponse(HttpErrorResponseHead.Get(), new HttpErrorDataProducer());
                return;
            }

            foreach (IOperation o in this.m_Operations)
            {
                if (o.Handles(components[1].ToLowerInvariant()))
                {
                    o.Handle(components[0], components.Where((value, row) => row >= 2).ToArray(), head, body, response);
                    return;
                }
            }

            // If all else fails..
            response.OnResponse(HttpErrorResponseHead.Get(), new HttpErrorDataProducer());
        }

        #endregion
    }
}
