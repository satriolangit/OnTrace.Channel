using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class MediaFile
    {
        public string Filename { get; set; }
        public byte[] FileData { get; set; }
        public bool IsAttachment { get; set; }
    }
}
