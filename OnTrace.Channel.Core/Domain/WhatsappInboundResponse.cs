using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using OnTrace.Channel.Core.Entities;


namespace OnTrace.Channel.Core.Domain
{
    public class WhatsappInboundResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public IEnumerable<InboundQueue> Data { get; set; } 
    }
}
