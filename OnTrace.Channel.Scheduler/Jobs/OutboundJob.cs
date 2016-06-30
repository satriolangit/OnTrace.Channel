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
           try
           {
               //get datamap
               var schedulerContext = context.Scheduler.Context;
               var modemSetting = (ModemSetting) schedulerContext.Get("ModemSetting");
               var fileProcessor = (FileProcessor) schedulerContext.Get("FileProcessor");
               var mailAccount = (MailAccount) schedulerContext.Get("MailAccount");
               var outboundTempPath = schedulerContext.Get("OutboundTempPath").ToString();
               var connectionString = schedulerContext.Get("ConnectionString").ToString();


               //get queues
               var repo = new AdoOutboundQueueRepository(connectionString);
               var log = new AdoOutboundLogRepository(connectionString);

               var outboundQueues = repo.GetOutboundQueues(5);

               foreach (OutboundQueue queue in outboundQueues)
               {
                   //send messages(1:email, 2:message, 3:whatsapp, 4:facebook, 5:twitter)

                   int channelId = queue.InteractionChannelTypeID;
                   if (channelId == 1)
                   {
                       var sender = new MailSender(mailAccount, fileProcessor);

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
                               string path = outboundTempPath + file.Filename;
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
                       sender.SendEmail(to, subject, message, attachments);
                   }
                   else if (channelId == 2)
                   {

                        Logger.Write($"Sending sms to=[{queue.AccountName}], message=[{queue.Message}]", EventSeverity.Information);

                        var sender = new SmsSender(modemSetting);
                       sender.SendMessage(queue.Message, queue.AccountName);
                   }
                   else if (channelId == 3)
                   {
                       //not implemented
                   }
                   else if (channelId == 4)
                   {
                       //not implemented
                   }
                   else
                   {
                       //not implemented
                   }

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

                    Logger.Write($"Create new outbound log, id=[{newLog.LogId}], agentId=[{queue.AgentId}], channel=[{queue.InteractionChannelTypeID}], subject=[{queue.Subject}]", EventSeverity.Information);
                   log.CreateLog(newLog);

                    //remove queue
                    Logger.Write($"Remove outbound queue, id=[{queue.QueueId}]", EventSeverity.Information);
                    repo.Remove(queue.QueueId);
               }
           }
           catch (Exception ex)
           {
                Logger.Write("Failed to execute outbound job.", ex, EventSeverity.Error);
           }

       }
    }
}
