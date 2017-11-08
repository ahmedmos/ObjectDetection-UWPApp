using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelligentKioskSample.Helper
{
    public class PendingChats
    {
        public List<QAs> qa { get; set; }
        public List<Syncmessages> sync { get; set; }
    }

    public class QAs
    {
        public long Id { get; set; }
        public string Tag { get; set; }
        public bool IsAnswered { get; set; }
        public string Type { get; set; }
        public System.DateTime createddatetime { get; set; }
    }

    public class Syncmessages
    {
        public long Id { get; set; }
        public string Question { get; set; }
        public bool isSynced { get; set; }
        public System.DateTime createddatetime { get; set; }
    }

}
