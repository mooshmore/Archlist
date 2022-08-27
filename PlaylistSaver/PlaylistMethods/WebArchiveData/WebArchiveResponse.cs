using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebArchiveData
{
#pragma warning disable IDE1006 // Naming Styles
    public class WebArchiveResponse
    {
        public string url { get; set; }
        public ArchivedSnapshots archived_snapshots { get; set; }

        public class ArchivedSnapshots
        {
            public Closest closest { get; set; }
        }

        public class Closest
        {
            public string status { get; set; }
            public bool available { get; set; }
            public string url { get; set; }
            public string timestamp { get; set; }
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
