using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class TwitterMessage
    {
        public long Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TweetMedia> Media { get; set; }
    }
}
