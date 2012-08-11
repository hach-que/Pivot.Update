using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pivot.Update
{
    public class ApplicationInstance
    {
        public string FilesystemPath;
        public string UpdateUrl;
        public DateTime LastCheckTime;
    }

    public class ApplicationInstanceList : List<ApplicationInstance>
    {
    }
}
