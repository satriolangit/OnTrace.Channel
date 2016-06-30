using System.Collections.Generic;
using OnTrace.Channel.Core.Domain;
using WhatsappMessage = OnTrace.Channel.Core.Domain.WhatsappMessage;

namespace OnTrace.Channel.Core.Interfaces
{
    public interface IWhatsappRepository
    {
        void CreateInboundQueue(WhatsappMessage message);
        void CreateOutboundQueue(WhatsappMessage message);

    }
}