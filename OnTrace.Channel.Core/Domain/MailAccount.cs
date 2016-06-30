using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class MailAccount
    {
        public string SettingType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string SecurityType { get; set; }
        public bool EnableSsl { get; set; }

    }
}
