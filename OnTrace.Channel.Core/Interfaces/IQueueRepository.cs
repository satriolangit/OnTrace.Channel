using OnTrace.Channel.Core.Entities;

namespace OnTrace.Channel.Core.Interfaces
{
    public interface IQueueRepository
    {
        void CreateInboundQueue(InboundQueue model);
        void CreateOutboundQueue(OutboundQueue model);
        void InsertInboundQueueFile(InboundQueueFile file);
        void InsertOutboundQueueFile(OutboundQueueFile file);

    }
}