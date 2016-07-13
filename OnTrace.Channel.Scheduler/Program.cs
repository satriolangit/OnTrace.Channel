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
        private static ModemSetting _modemSetting;
        
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

            _modemSetting = new ModemSetting();
            _modemSetting.Port = 6;
            _modemSetting.BaudRate = 460800;
            _modemSetting.Timeout = 300;

            while (_start)
            {
                Console.WriteLine("Select one of the following options : ");
                Console.WriteLine("0. Start all scheduler");
                Console.WriteLine("1. Start outbound scheduler");
                Console.WriteLine("2. Stop outbound scheduler");
                Console.WriteLine("3. Start inbound scheduler");
                Console.WriteLine("4. Stop inbound scheduler");
                Console.WriteLine("5. Stop all scheduler");
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
                        case "1":
                            StartOutbound();
                            break;
                        case "2":
                            StopOutbound();
                            break;
                        case "3":
                            StartInbound();
                            break;
                        case "4":
                            StopInbound();
                            break;
                        case "5":
                            Stop();
                            break;
                        case "6":
                            TwitterTest();
                            break;
                        case "x":
                            _start = false;
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
                OutboundScheduler.Context.Put("MailAccount", repo.GetMailAccount("outgoing"));
                OutboundScheduler.Context.Put("FileProcessor", new FileProcessor());
                OutboundScheduler.Context.Put("ConnectionString", GetDbConstring());
                OutboundScheduler.Context.Put("OutboundTempPath", OutboundTempPath);

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
                if(OutboundScheduler.IsStarted) OutboundScheduler.Shutdown(true);
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

                IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                scheduler.Start();

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
                scheduler.Context.Put("ModemSetting", repo.GetModemSetting());
                scheduler.Context.Put("MailAccount", repo.GetMailAccount("incoming"));
                scheduler.Context.Put("FileProcessor", new FileProcessor());
                scheduler.Context.Put("ConnectionString", connectionString);

                //run schedule
                scheduler.ScheduleJob(job, trigger);
                

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
                Logger.Write("Stoping outbound scheduler...", EventSeverity.Information);
                if (InboundScheduler.IsStarted) InboundScheduler.Shutdown(true);
                Logger.Write("Outbound scheduler stopped.", EventSeverity.Information);
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
