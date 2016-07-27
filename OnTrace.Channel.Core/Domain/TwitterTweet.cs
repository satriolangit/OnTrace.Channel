using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class TwitterTweet
    {
        public long Id { get; set; }
        public string CreatedBy{ get; set; }
        public DateTime CreatedAt { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
        public List<TweetMedia> Media { get; set; }

    }
}
