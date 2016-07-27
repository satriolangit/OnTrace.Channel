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
    public class OutboundJob : IJob
    {

        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {

            //get datamap
            var schedulerContext = context.Scheduler.Context;
            var modemSetting = (ModemSetting)schedulerContext.Get("ModemSetting");
            var fileProcessor = (FileProcessor)schedulerContext.Get("FileProcessor");
            var outboundTempPath = schedulerContext.Get("OutboundTempPath").ToString();
            var connectionString = schedulerContext.Get("ConnectionString").ToString();
            var mailSender = (MailSender)schedulerContext.Get("MailSender");
            var twitterHelper = (TwitterHelper) schedulerContext.Get("TwitterHelper");

            //get queues
            var repo = new AdoOutboundQueueRepository(connectionString);
            var log = new AdoOutboundLogRepository(connectionString);
            var outboundQueues = repo.GetOutboundQueues(5);

            foreach (OutboundQueue queue in outboundQueues)
            {
                //send messages(1:email, 2:message, 3:twitter, 4:facebook, 5:whatsapp)

                int channelId = queue.InteractionChannelTypeID;
                if (channelId == 1)
                {
                   ProcessMail(queue, fileProcessor, outboundTempPath, mailSender);
                }
                else if (channelId == 2)
                {
                    ProcessSms(queue, modemSetting);
                }
                else if (channelId == 5)
                {
                    //twitter
                    ProcessTwitter(queue, twitterHelper);
                }
                else if (channelId == 4)
                {
                    //facebook
                }
                else
                {
                    //not implemented
                }

                //Create Outbound Log
                CreateLog(queue, log, repo);
            }
       }

        private void ProcessMail(OutboundQueue queue, FileProcessor fileProcessor, string tempPath, MailSender mailSender)
        {
            string to = queue.AccountName;
            string subject = queue.Subject;
            string message = queue.Message;

            //get attachments
            var attachments = new List<MailAttachment>();
            if (queue.MediaFiles != null)
            {
                foreach (var file in queue.MediaFiles)
                {
                    //write byte to file
                    string[] filenames = file.Filename.Split('.');
                    string fileType = filenames[filenames.Length - 1];
                    string path = tempPath + file.Filename;
                    fileProcessor.BytesToFile(path, file.FileData);

                    var attachment = new MailAttachment
                    {
                        Filename = file.Filename,
                        Path = path,
                        FileType = fileType
                    };

                    attachments.Add(attachment);
                }
            }

            //send email
            Logger.Write($"Sending mail to=[{to}], subject=[{subject}]", EventSeverity.Information);
            mailSender.SendEmail(to, subject, message, attachments);
        }

        private void ProcessSms(OutboundQueue queue, ModemSetting modemSetting)
        {
            Logger.Write($"Sending sms to=[{queue.AccountName}], message=[{queue.Message}]", EventSeverity.Information);

            var sender = new SmsSender(modemSetting);
            //sender.SendMessage(queue.Message, queue.AccountName);
        }

        private void ProcessTwitter(OutboundQueue queue, TwitterHelper helper)
        {
            if (queue.MessageType == 0)
            {
                Logger.Write($"Publish tweet, {queue.Message}", EventSeverity.Information);
                helper.PublishTweet(queue.Message);
            }
            else if (queue.MessageType == 1)
            {
                var images = (from file in queue.MediaFiles where file.FileType.ToLower() != ".mp4" select file.FileData).ToList();

                Logger.Write($"Publish tweet with image, {queue.Message}", EventSeverity.Information);
                helper.PublishTweetWithImage(queue.Message, images);
            }
            else if (queue.MessageType == 2)
            {
                var video = (from file in queue.MediaFiles where file.FileType.ToLower() == ".mp4" select file.FileData).FirstOrDefault();

                Logger.Write($"Publish tweet with video, {queue.Message}", EventSeverity.Information);
                helper.PublishTweetWithVideo(queue.Message, video);
            }
            else if(queue.MessageType == 3)
            {
                Logger.Write($"Send private message to {queue.AccountName}, {queue.Message}", EventSeverity.Information);
                helper.PublishMessage(queue.Message, queue.AccountName);
            }
         
        }

        private void CreateLog(OutboundQueue queue, AdoOutboundLogRepository logRepo, AdoOutboundQueueRepository repo)
        {
            //create log
            var newLog = new OutboundLog
            {
                AccountName = queue.AccountName,
                AgentId = queue.AgentId,
                InteractionChannelTypeId = queue.InteractionChannelTypeID,
                LastAgentId = queue.LastAgentID,
                LastDistributedTime = queue.LastDistributedTime ?? DateTime.Now,
                MediaFiles = queue.MediaFiles,
                Message = queue.Message,
                MessageStatus = queue.MessageStatus,
                MessageTime = DateTime.Now,
                Subject = queue.Subject
            };

            Logger.Write($"Create new outbound log, id=[{newLog.LogId}]", EventSeverity.Information);
            logRepo.CreateLog(newLog);

            //remove queue
            Logger.Write($"Remove outbound queue, id=[{queue.QueueId}]", EventSeverity.Information);
            repo.Remove(queue.QueueId);
        }
    }
}
