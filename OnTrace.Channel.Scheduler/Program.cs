using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Domain;
using OnTrace.Channel.Infrastructure.Services;
using OnTrace.Channel.Infrastructure.Data;
using OnTrace.Channel.Infrastructure.Logger;
using OnTrace.Channel.Core.Entities;
using OnTrace.Channel.Core.Interfaces;
using OnTrace.Channel.Scheduler.Jobs;
using Quartz;
using Quartz.Impl;


namespace OnTrace.Channel.Scheduler
{
    class Program
    {
        private static bool _start;
        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly IScheduler OutboundScheduler = StdSchedulerFactory.GetDefaultScheduler();
        private static readonly IScheduler InboundScheduler = StdSchedulerFactory.GetDefaultScheduler();
        

        static void Main(string[] args)
        {
            _start = false;
            ConsoleKeyInfo cki;
            while (!_start)
            {
                Console.WriteLine("OnTrace Channel Core Scheduler 1.0.0 - SMI 2016 ");
                Console.WriteLine();
                Console.Write("Press Enter to start the application...");
                cki = Console.ReadKey();

                if (cki.Key == ConsoleKey.Enter)
                {
                    _start = true;
                    break;
                }
            }

       
            while (_start)
            {
                Console.WriteLine("Select one of the following options : ");
                Console.WriteLine("0. Start");
                Console.WriteLine("1. Stop");
                Console.WriteLine("2. Start outbound only");
                Console.WriteLine("3. Stop outbound");
                Console.WriteLine("4. Start inbound only");
                Console.WriteLine("5. Stop inbound");
                Console.WriteLine("x. Exit");
                Console.WriteLine();
                
                string entry = Console.ReadLine();

                if (entry != string.Empty)
                {
                    switch (entry)
                    {
                        case "0":
                            Start();
                            break;
                        case "2":
                            StartOutbound();
                            break;
                        case "3":
                            StopOutbound();
                            break;
                        case "4":
                            StartInbound();
                            break;
                        case "5":
                            StopInbound();
                            break;
                        case "1":
                            Stop();
                            break;
                        case "6":
                            TwitterTest();
                            break;
                        case "x":
                            Exit();
                         break;
                    }
                }
            }
        }

        private static void TwitterTest()
        {
            var helper = new TwitterHelper();
            helper.TweetTest();
        }


        private static void Start()
        {
            StartOutbound();
            StartInbound();
        }

        private static void Stop()
        {
            StopOutbound();
            StopInbound();
        }

        private static void Exit()
        {
            _start = false;
            if(!OutboundScheduler.IsShutdown) OutboundScheduler.Shutdown(true);
            if (!InboundScheduler.IsShutdown) InboundScheduler.Shutdown(true);
        }

        private static void StartOutbound()
        {
            try
            {
                Logger.Write("Processing outbound...", EventSeverity.Information);

                var repo = new AdoMasterDataRepository(GetDbConstring());

                if(!OutboundScheduler.IsStarted || OutboundScheduler.IsShutdown) OutboundScheduler.Start();

                IJobDetail job = JobBuilder.Create<OutboundJob>()
                    .WithIdentity("Outbound", "job")
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("outboundTrigger", "trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                    .Build();

                //pass services to job 
                OutboundScheduler.Context.Put("ModemSetting", repo.GetModemSetting());
                OutboundScheduler.Context.Put("FileProcessor", new FileProcessor());
                OutboundScheduler.Context.Put("ConnectionString", GetDbConstring());
                OutboundScheduler.Context.Put("OutboundTempPath", OutboundTempPath);
                OutboundScheduler.Context.Put("MailSender", new MailSender(repo.GetMailAccount("outgoing"), new FileProcessor()));

                //start trigger
                OutboundScheduler.ScheduleJob(job, trigger);
                
            }
            catch (Exception ex)
            {
                Logger.Write("Failed to process outbound, please contact your Administrator !", ex, EventSeverity.Error);
            }
        }
       
        private static void StopOutbound()
        {
            try
            {
                Logger.Write("Stoping outbound scheduler...", EventSeverity.Information);
                if(OutboundScheduler.IsStarted) OutboundScheduler.Standby();
                Logger.Write("Outbound scheduler stopped.", EventSeverity.Information);
            }
            catch (Exception ex)
            {
                Logger.Write("Failed to stop outbound scheduler, please contact your Administrator !", ex, EventSeverity.Error);
            }
        }

        private static void StartInbound()
        {
            try
            {
                Logger.Write("Processing inbound..", EventSeverity.Information);
                string connectionString = GetDbConstring();
                var repo = new AdoMasterDataRepository(connectionString);

                if (!InboundScheduler.IsStarted || InboundScheduler.InStandbyMode) InboundScheduler.Start();

                IJobDetail job = JobBuilder.Create<InboundJob>()
                    .WithIdentity("Inbound", "job")
                    .Build();

                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("inboundTrigger", "trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)
                        .RepeatForever())
                    .Build();

                //pass services to job 
                InboundScheduler.Context.Put("ModemSetting", repo.GetModemSetting());
                InboundScheduler.Context.Put("MailAccount", repo.GetMailAccount("incoming"));
                InboundScheduler.Context.Put("FileProcessor", new FileProcessor());
                InboundScheduler.Context.Put("ConnectionString", connectionString);

                //run schedule
                InboundScheduler.ScheduleJob(job, trigger);

            }
            catch (SchedulerException se)
            {
                Logger.Write("Failed to process inbound, please contact your Administrator !", se, EventSeverity.Error);
            }
        }

        private static void StopInbound()
        {
            try
            {
                Logger.Write("Stoping inbound scheduler...", EventSeverity.Information);
                if (InboundScheduler.IsStarted) InboundScheduler.Standby();
                Logger.Write("Inbound scheduler stopped.", EventSeverity.Information);
            }
            catch (Exception ex)
            {
                Logger.Write("Failed to stop inbound scheduler, please contact your Administrator !", ex, EventSeverity.Error);
            }
        }


        private static void LogTest()
        {
            var repo = new AdoOutboundQueueRepository(GetDbConstring());
            var query = repo.GetQueueFiles("45a0c232a59d42da9da151fc36052bb7");
        }

        private static string GetDbConstring()
        {
            return ConfigurationManager.AppSettings["DbConnectionString"];
        }
        
        private static string CurrentPath
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string OutboundTempPath
        {
            get
            {
                return CurrentPath + "/files/temp_outbound/";
            }
            
        }
      
    }
}
