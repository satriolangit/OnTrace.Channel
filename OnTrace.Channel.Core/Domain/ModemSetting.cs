using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class ModemSetting
    {
        public int Port { get; set; }
        public int BaudRate { get; set; }
        public int Timeout { get; set; }

    }
}
