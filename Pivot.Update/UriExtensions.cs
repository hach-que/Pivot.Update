using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pivot.Update
{
    internal static class UriExtensions
    {
        public static string ToCachePath(this Uri uri)
        {
            return uri.Host + "/" + uri.AbsolutePath;
        }
    }
}
