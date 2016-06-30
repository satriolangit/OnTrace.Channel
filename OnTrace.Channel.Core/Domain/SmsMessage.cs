using System;


namespace OnTrace.Channel.Core.Domain
{
    public class SmsMessage
    {
        public string Address { get; set; }
        public string Message { get; set; }
        public DateTime MessageTime { get; set; }
    }
}
