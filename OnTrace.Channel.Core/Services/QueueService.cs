using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Domain;

namespace OnTrace.Channel.Core.Services
{
    public class QueueService
    {
        private ResultMessage _resultMessage;
        public QueueService()
        {
            
        }

        public ResultMessage GetResultMessage()
        {
            return _resultMessage;
        }

        public void CreateMailInboundQueue()
        {
            //TODO: Check channel type
            //TODO: GetMessage based on channel type
            //TODO: Check message type
            //TODO: If type is media or have attachments, download file(s)
            //TODO: Create Inbound Queue, get QueueID
            //TODO: Insert File

        }
    }
}
