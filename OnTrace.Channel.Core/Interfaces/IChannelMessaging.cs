using OnTrace.Channel.Core.Entities;

namespace OnTrace.Channel.Core.Interfaces
{
    public interface IChannelMessaging
    {
        InboundQueue GetMessage();
    }
}