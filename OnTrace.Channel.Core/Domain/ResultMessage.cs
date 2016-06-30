using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTrace.Channel.Core.Domain
{
    public class ResultMessage
    {
        public enum ResultMessageStatus
        {
            Ok,
            Error
        }

        public ResultMessageStatus Status { get; set; }
        public string Message { get; set; }

    }
}
