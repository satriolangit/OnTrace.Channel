using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class MailMessage
    {
        public MailMessage()
        {
            MailId = Guid.NewGuid().ToString("N");
        }

        private string MailId { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public ICollection<MediaFile> MediaFiles { get; set; }


    }
}
