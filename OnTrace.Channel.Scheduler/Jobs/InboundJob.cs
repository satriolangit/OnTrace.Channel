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
            var twitterHelper = (TwitterHelper)schedulerContext.Get("TwitterHelper");
            var ipAddress = schedulerContext.Get("IPAddress").ToString();

            var repo = new AdoInboundQueueRepository(connectionString);
            var repoMaster = new AdoMasterDataRepository(connectionString);

            //email
            CreateMailQueue(mailAccount, fileProcessor, repo);
            
            //twitter
            CreateTwitterQueue(twitterHelper, repo, repoMaster, ipAddress);
               
            //sms
            //CreateSmsQueue(modemSetting, repoMaster, repo);
           
        }

        private static void CreateSmsQueue(ModemSetting modemSetting, AdoMasterDataRepository repoMaster,
            AdoInboundQueueRepository repo)
        {
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
        }

        private static void CreateMailQueue(MailAccount mailAccount, FileProcessor fileProcessor, AdoInboundQueueRepository repo)
        {
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
        }

        private void CreateTwitterQueue(TwitterHelper twitterHelper, AdoInboundQueueRepository repo, AdoMasterDataRepository repoMaster, string ipAddress)
        {
            try
            {
                var syncTime = repo.GetTwitterSyncTime();

                Logger.Write($"Retrieve twitter data betweeen {syncTime.ActivityTime} and {DateTime.Now}", EventSeverity.Information);

                var channelType = repoMaster.GetChannelType("twitter");

                #region Tweets
                var tweets = twitterHelper.GetMentionsTimeline(syncTime.ActivityTime, DateTime.Now);

                foreach (var tweet in tweets)
                {
                    var queue = new InboundQueue();

                    var media = tweet.Media.Select(item => new InboundQueueFile()
                    {
                        Filename = item.Filename,
                        Url = item.Url,
                        FileData = item.Filedata,
                        FileType = item.Type,
                        IsAttachment = true,
                        QueueID = queue.QueueId
                    }).ToList();

                    string mediaType = media.Select(x => x.FileType).FirstOrDefault();

                    queue.Subject = "tweet";
                    queue.AccountName = "@" + tweet.CreatedBy;
                    queue.InteractionChannelTypeID = channelType.InteractionChannelTypeId;
                    queue.LastAgentID = 0;
                    queue.Message = tweet.Text;
                    queue.MediaFiles = media;
                    queue.MessageType = media.Count == 0 ? 0 : mediaType != null && mediaType.Contains("mp4") ? 2 : 1;

                    Logger.Write($"Create twitter inbound queue, [address={queue.AccountName}, text={queue.Message}]", EventSeverity.Information);

                    repo.CreateInboundQueue(queue);
                }
                #endregion

                #region Private Messages
                var messages = twitterHelper.GetPrivateMessages();
                foreach (var message in messages)
                {
                    var queue = new InboundQueue()
                    {
                        Subject = "Private Message",
                        AccountName = "@" + message.Sender,
                        InteractionChannelTypeID = channelType.InteractionChannelTypeId,
                        LastAgentID = 0,
                        Message = message.Text,
                        MediaFiles = new List<InboundQueueFile>(),
                        MessageType = 2
                    };

                    Logger.Write($"Create twitter inbound queue, [address={queue.AccountName}, text={queue.Message}]",EventSeverity.Information);
                    repo.CreateInboundQueue(queue);
                    
                    //delete server message 
                    twitterHelper.DestroyMessage(message.Id);
                    
                }
                #endregion

                //update sync time
                repo.UpdateTwitterSyncTime(Guid.NewGuid().ToString("N"), DateTime.Now, ipAddress);
            }
            catch (Exception ex)
            {
                Logger.Write($"Failed to create twitter inbound queue.", ex, EventSeverity.Error);
            }
        }
    }
}
