using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using Kayak.Http;

namespace Pivot.Update.Server
{
    public interface IOperation
    {
        bool Handles(string query);
        void Handle(string appname, string[] components, HttpRequestHead head, IDataProducer body, IHttpResponseDelegate response);
    }
}
