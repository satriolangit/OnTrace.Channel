using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class MailAttachment
    {
        public string Filename { get; set; }
        public string FileType { get; set; }
        public string Path { get; set; }
    }
}
