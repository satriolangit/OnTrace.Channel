using System;

namespace OnTrace.Channel.Core.Domain
{
    public class WhatsappLog
    {
        public int LogId { get; set; }
        public int? AgentId { get; set; }
        public int? CampaignId { get; set; }
        public int?  CustomerId { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime MessageTime { get; set; }
        public int Type { get; set; }
        public string MessageText { get; set; }
    }
}
