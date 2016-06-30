using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class WhatsappInboundResult
    {
        public string Status { get; set; }
        public string State { get; set; }
        public string Message { get; set; }
        public virtual ICollection<WhatsappMessage> Data { get; set; }

    }
}
