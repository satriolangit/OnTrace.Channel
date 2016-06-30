using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GsmComm.GsmCommunication;
using GsmComm.PduConverter;
using OnTrace.Channel.Core.Domain;

namespace OnTrace.Channel.Infrastructure.Services
{
    public class SmsSender
    {
      
        private readonly ModemSetting _setting;
       
        public SmsSender(ModemSetting modemSetting)
        {
            _setting = modemSetting;
        }

        public void SendMessage(string message, string number)
        {
            
            var comm = new GsmCommMain(_setting.Port, _setting.BaudRate, _setting.Timeout);
            try
            {
                comm.Open();
                SmsSubmitPdu pdu;
                pdu = new SmsSubmitPdu(message, number);
                comm.SendMessage(pdu);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send sms message.", ex);
            }
            finally
            {
                comm.Close();
            }
        }
    }
}
