using System;
using OnTrace.Channel.Core.Domain;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Newtonsoft.Json;
using WhatsAppApi;

namespace OnTrace.Channel.Infrastructure.Services
{
    public class WhatsappService
    {
        private readonly WhatsappAccount _account;
        private readonly WhatsAppApi.WhatsApp _wa;
        public WhatsappService(WhatsappAccount account)
        {
            _account = account;
            try
            {
                _wa = new WhatsApp(account.Username, account.Password, account.Nickname);
                _wa.Connect();
                _wa.Login();
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to connect to whatsapp, error : " + ex.Message, ex);
            }
            
        }

        public WhatsappInboundResult GetMessage()
        {

            var result = new WhatsappInboundResult();
            //get message from server
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("http://192.168.1.146/");
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = http.GetAsync("whatsapper/api/message/get").Result;

                if (response.IsSuccessStatusCode)
                {

                    var content = response.Content.ReadAsStringAsync().Result;
                    result = JsonConvert.DeserializeObject<WhatsappInboundResult>(content);                    
                }
                else
                {
                    result = new WhatsappInboundResult {
                           Status = "ERROR",
                           State = "FAILED",
                           Data = null,
                           Message = "Failed retrieve whatsapp message"
                    };
                } 
            }

            return result;
        }

        //TODO: PostMessage
        public void SendMessage(string message, string to)
        {
            try
            {
                _wa.SendMessage(to, message);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send whatsapp message, error : " + ex.Message, ex);
            }
        }

    }
}