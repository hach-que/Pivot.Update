using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;

namespace Pivot.Update.Server
{
    class HttpErrorDataProducer : BufferedProducer
    {
        public HttpErrorDataProducer()
            : base("The server did not understand your request.")
        {
        }
    }
}
