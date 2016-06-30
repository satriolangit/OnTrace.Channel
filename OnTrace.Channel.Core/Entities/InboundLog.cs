using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Entities
{
    public class InboundLog
    {
        public InboundLog()
        {
            LogId = Guid.NewGuid().ToString("N");
        }

        public string LogId { get; private set; }
        public int? AgentId { get; set; }
        public string AccountName { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        public string MessageStatus { get; set; }
        public int InteractionChannelTypeId { get; set; }
        public DateTime LastDistributedTime { get; set; }
        public int LastAgentId { get; set; }
        public ICollection<InboundFileLog> MediaFiles { get; set; }
    }
}
