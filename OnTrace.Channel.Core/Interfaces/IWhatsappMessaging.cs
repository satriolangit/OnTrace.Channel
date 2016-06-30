using OnTrace.Channel.Core.Domain;

namespace OnTrace.Channel.Core.Interfaces
{
    public interface IWhatsappMessaging
    {
        WhatsappInboundResult GetMessage();
        void PostMessage(WhatsappMessage message);

    }
}