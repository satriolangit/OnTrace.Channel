using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class TwitterSyncTime
    {
        public TwitterSyncTime()
        {
            RecordId = Guid.NewGuid().ToString("N");
        }

        public string RecordId { get; set; }
        public DateTime ActivityTime { get; set; }
        public string Source { get; set; }

    }
}
