using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class WhatsappMessage
    {
        public string Account { get; set; }
        public string From { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public string Name { get; set; }
        public string Data { get; set; }
        public string Url { get; set; }
        public string MimeType { get; set; }

    }
}
