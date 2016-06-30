using System;
using System.Collections.Generic;


namespace OnTrace.Channel.Core.Entities
{
    public class InboundQueue
    {
        public InboundQueue()
        {
            QueueId = Guid.NewGuid().ToString("N");
        }

        public string QueueId { get; private set; }
        public int? AgentId { get; set; }
        public string AccountName { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        public string MessageStatus { get; set; }
        public int InteractionChannelTypeID { get; set; }
        public DateTime LastDistributedTime { get; set; }
        public int LastAgentID { get; set; }
        public ICollection<InboundQueueFile> MediaFiles { get; set; }
    }
}
