using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Domain;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using OnTrace.Channel.Core.Entities;
using OnTrace.Channel.Core.Interfaces;
using OnTrace.Channel.Infrastructure.Data;
using OnTrace.Channel.Infrastructure.Logger;
using OnTrace.Channel.Infrastructure.Services;

namespace OnTrace.Channel.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class InboundJob : IJob
    {
        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public InboundJob()
        {
            
        }

       public void Execute(IJobExecutionContext context)
       {
            //get datamap
            var schedulerContext = context.Scheduler.Context;
            var connectionString = schedulerContext.Get("ConnectionString").ToString();
            var modemSetting = (ModemSetting) schedulerContext.Get("ModemSetting");
            var fileProcessor = (FileProcessor) schedulerContext.Get("FileProcessor");
            var mailAccount = (MailAccount) schedulerContext.Get("MailAccount");
            
            var repo = new AdoInboundQueueRepository(connectionString);
            var repoMaster = new AdoMasterDataRepository(connectionString);

            try
            {
                Logger.Write($"Retrieve mail messages, Account=[{mailAccount.Username}]", EventSeverity.Information);

                MailRetriever mailRetriever = new MailRetriever(mailAccount, fileProcessor);
                var messages = mailRetriever.GetMailMessages(5);

                foreach (var message in messages)
                {
                    Logger.Write($"Create mail inbound queue, [address={message.From}]", EventSeverity.Information);
                    repo.CreateMailInboundQueue(message);
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"Failed to create mail inbound queue.", ex, EventSeverity.Error);
            }

            /*
           try
           {
               
               Logger.Write("Retrieving sms message..", EventSeverity.Information); 
               var smsRetriever = new SmsRetriever(modemSetting.Port, modemSetting.BaudRate, modemSetting.Timeout);
               var messages = smsRetriever.GetMessages();
               var channelType = repoMaster.GetChannelType("sms");

               foreach (var message in messages)
               {
                    var queue = new InboundQueue();
                    queue.AccountName = message.Address;
                    queue.Message = message.Message;
                    queue.InteractionChannelTypeID = channelType.InteractionChannelTypeId;

                    repo.CreateSmsInboundQueue(queue);
               }
            }
           catch (Exception ex)
           {
               Logger.Write($"Failed to create sms inbound queue", ex, EventSeverity.Error);
           }
           */
        }
    }
}
