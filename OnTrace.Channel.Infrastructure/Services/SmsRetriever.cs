using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using OnTrace.Channel.Core.Domain;

namespace OnTrace.Channel.Infrastructure.Services
{
  
    public class SmsRetriever
    {
        private readonly int _port;
        private readonly int _baudRate;
        private readonly int _timeout;

        public SmsRetriever(int port, int baudRate, int timeout)
        {
            _port = port;
            _baudRate = baudRate;
            _timeout = timeout;
        }

        public ICollection<SmsMessage> GetMessages()
        {
            var comm = new GsmCommMain(_port, _baudRate, _timeout);
            var result = new List<SmsMessage>();
            try
            {
                comm.Open();

                int messageIndex = 0;
                
                //get sim messages
                DecodedShortMessage[] messages = comm.ReadMessages(PhoneMessageStatus.ReceivedUnread, PhoneStorageType.Sim);
                foreach (DecodedShortMessage message in messages)
                {
                    SmsDeliverPdu data = (SmsDeliverPdu)message.Data;

                    var sms = new SmsMessage
                    {
                        Address = data.OriginatingAddress,
                        Message = data.UserDataText,
                        MessageTime = DateTime.Now
                    };
                    result.Add(sms);
                    messageIndex = message.Index;
                    //delete message
                    comm.DeleteMessage(messageIndex, PhoneStorageType.Sim);
                }

                messages = comm.ReadMessages(PhoneMessageStatus.ReceivedUnread, PhoneStorageType.Phone);
                foreach (DecodedShortMessage message in messages)
                {
                    SmsDeliverPdu data = (SmsDeliverPdu)message.Data;

                    var sms = new SmsMessage
                    {
                        Address = data.OriginatingAddress,
                        Message = data.UserDataText,
                        MessageTime = DateTime.Now
                    };

                    result.Add(sms);
                    messageIndex = message.Index;
                    //delete message
                    comm.DeleteMessage(messageIndex, PhoneStorageType.Sim);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve sms messages.", ex);
            }
            finally
            {
                comm.Close();
            }
          
        }

      
    }
}
