using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
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
                Console.WriteLine("0. Stop");
                Console.WriteLine("1. Start");
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
                            Stop();
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
                            Start();
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
            var repo = new AdoMasterDataRepository(GetDbConstring());
            var fileProcessor = new FileProcessor();
            var account = repo.GetTwitterAccount();

            var helper = new TwitterHelper(account, fileProcessor);
            //helper.PublishTweet("Morning guys...today we gonna do some test tweet, be brave :)");
            //helper.PublishTweet("@wahyuhari check this out bro...");
            //helper.PublishMessage("bro, we need your response regarding this message test :)", "@wahyuhari");
            //helper.PublishTweet("@wahyuhari, @idris_maulana check this out guys...");
            //string imagePath = OutboundTempPath + "kresna.png";
            //helper.PublishTweetWithImage("@idris_maulana, @wahyuhari.. sebutkan 3 dari sekian dasanama tokoh ini !", imagePath);
            string videoPath = OutboundTempPath + "ionic_introduction.1.mp4";
            try
            {
                //helper.PublishTweetWithVideo("Sample video", videoPath);
                //helper.PublishTweetWithVideo("binary video", fileProcessor.StreamToBytes(videoPath));
            }
            catch (Exception ex)
            {
                Logger.Write("Failed to publish video.", ex, EventSeverity.Error);
            }
            
            //helper.GetMentionsTimeline(DateTime.MinValue, DateTime.Now);
            //helper.GetPrivateMessages();
            //            helper.SearchMentionsTimeline();

            helper.SearchTimeline(new DateTime(1753,1,1), new DateTime(2016,7,28));

            Console.WriteLine("Download image...");
            string url = "https://pbs.twimg.com/media/CoXDCI8UsAA1Pbq.jpg";
            string extension = Path.GetExtension(url);
            string filename = $"download_test_{Guid.NewGuid().ToString("N")}{extension}";

            fileProcessor.DownloadAndWriteMedia("https://pbs.twimg.com/media/CoXDCI8UsAA1Pbq.jpg", InboundTempPath + filename);

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
                var repo = new AdoMasterDataRepository(GetDbConstring());
                var twitterHelper = new TwitterHelper(repo.GetTwitterAccount(), new FileProcessor())
                {
                    MediaStorageLocationPath = OutboundTempPath
                };

                OutboundScheduler.Context.Put("ModemSetting", repo.GetModemSetting());
                OutboundScheduler.Context.Put("FileProcessor", new FileProcessor());
                OutboundScheduler.Context.Put("ConnectionString", GetDbConstring());
                OutboundScheduler.Context.Put("OutboundTempPath", OutboundTempPath);
                OutboundScheduler.Context.Put("MailSender", new MailSender(repo.GetMailAccount("outgoing"), new FileProcessor()));
                OutboundScheduler.Context.Put("TwitterHelper", twitterHelper);
                
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
                string connectionString = GetDbConstring();
                var repo = new AdoMasterDataRepository(connectionString);

                var twitterHelper = new TwitterHelper(repo.GetTwitterAccount(), new FileProcessor())
                {
                    MediaStorageLocationPath = InboundTempPath
                };
                InboundScheduler.Context.Put("ModemSetting", repo.GetModemSetting());
                InboundScheduler.Context.Put("MailAccount", repo.GetMailAccount("incoming"));
                InboundScheduler.Context.Put("FileProcessor", new FileProcessor());
                InboundScheduler.Context.Put("ConnectionString", connectionString);
                InboundScheduler.Context.Put("TwitterHelper", twitterHelper);
                InboundScheduler.Context.Put("IPAddress", GetIPAddress());
                
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

        private static string InboundTempPath
        {
            get
            {
                return CurrentPath + "/files/temp_inbound/";
            }

        }

        private static string GetIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

    }
}
