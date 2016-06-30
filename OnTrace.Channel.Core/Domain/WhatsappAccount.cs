using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class WhatsappAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public bool Status { get; set; }

        public WhatsappAccount()
        {
            Username = "6289603750296";
            Password = "hudKkl9N4osN47evWChEgNWXQK4=";
            Nickname = "SMI DEV";
            Status = true;
        }

    }
}
