using System;
using System.Threading;
using OnTrace.Channel.Core.Domain;
using OnTrace.Channel.Infrastructure.Services;
using Quartz;

namespace OnTrace.Channel.Scheduler
{
    [DisallowConcurrentExecution]
    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                var schedulerContext = context.Scheduler.Context;
                var connectionString = schedulerContext.Get("ConnectionString").ToString();
                
                //var modemSetting = (ModemSetting)schedulerContext.Get("ModemSetting");
                // var fileProcessor = (FileProcessor)schedulerContext.Get("FileProcessor");
                //var mailAccount = (MailAccount)schedulerContext.Get("MailAccount");
                //var outboundTempPath = schedulerContext.Get("OutboundTempPath").ToString();

                Console.WriteLine("Hello greetings from hellojob ! with key : {0} connstring : {1}", context.JobDetail.Key, connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            
            
            
        }
    }
}