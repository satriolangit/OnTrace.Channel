using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using OnTrace.Channel.Core.Domain;
using OnTrace.Channel.Core.Entities;

namespace OnTrace.Channel.WebUI.Controllers.api
{
    public class WhatsappController : ApiController
    {
        public string Get()
        {
            var queues = new List<InboundQueue>();

            return "";
        }
    }
}
