using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonDataAccess;
using OnTrace.Channel.Core.Domain;
using OnTrace.Channel.Core.Entities;
using OnTrace.Channel.Core.Interfaces;
using OnTrace.Channel.Infrastructure.Logger;

namespace OnTrace.Channel.Infrastructure.Data
{
    public class AdoInboundQueueRepository
    {
        private readonly CDA _cda;
        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AdoInboundQueueRepository(string connectionString)
        {
            _cda = new CDA(connectionString);
        }
        
        public void CreateMailInboundQueue(List<MailMessage> messages)
        {
            foreach (var message in messages)
            {
                var queue = new InboundQueue();
                var cmd = new SqlCommand("sp_OC_CreateInboundQueue") { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@QueueID", queue.QueueId);
                cmd.Parameters.AddWithValue("@AccountName", message.From);
                cmd.Parameters.AddWithValue("@Subject", message.Subject);
                cmd.Parameters.AddWithValue("@Message", message.Message);
                cmd.Parameters.AddWithValue("@MessageType", 1);
                cmd.Parameters.AddWithValue("@MessageStatus", "new");
                cmd.Parameters.AddWithValue("@MessageTime", DateTime.Now);
                cmd.Parameters.AddWithValue("@InteractionChannelTypeID", 1);

                try
                {
                    Logger.Write($"Creating inbound queue=[Mail], QueueId=[{queue.QueueId}], address=[{message.From}], subject=[{message.Subject}]", EventSeverity.Information);
                   _cda.ExecuteNonQueryWithTransaction(cmd);
                }
                catch (SqlException ex)
                {
                    throw new Exception($"Failed to create inbound queue=[Mail].." , ex);
                }
                

                foreach (var file in message.MediaFiles)
                {
                    var media = new InboundQueueFile
                    {
                        QueueID = queue.QueueId,
                        IsAttachment = file.IsAttachment,
                        Filename = file.Filename,
                        FileData = file.FileData
                    };

                    InsertMediaFile(media);
                }
            }
        }

        public void CreateMailInboundQueue(MailMessage message)
        {
            var queue = new InboundQueue();
            var cmd = new SqlCommand("sp_OC_CreateInboundQueue") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QueueID", queue.QueueId);
            cmd.Parameters.AddWithValue("@AccountName", message.From);
            cmd.Parameters.AddWithValue("@Subject", message.Subject);
            cmd.Parameters.AddWithValue("@Message", message.Message);
            cmd.Parameters.AddWithValue("@MessageType", 1);
            cmd.Parameters.AddWithValue("@MessageStatus", "new");
            cmd.Parameters.AddWithValue("@MessageTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@InteractionChannelTypeID", 1);

            try
            {
                Logger.Write($"Creating inbound queue=[Mail], QueueId=[{queue.QueueId}], address=[{message.From}], subject=[{message.Subject}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create inbound queue=[Mail]..", ex);
            }


            foreach (var file in message.MediaFiles)
            {
                var media = new InboundQueueFile
                {
                    QueueID = queue.QueueId,
                    IsAttachment = file.IsAttachment,
                    Filename = file.Filename,
                    FileData = file.FileData
                };

                InsertMediaFile(media);
            }
            
        }

        public void CreateSmsInboundQueue(InboundQueue model)
        {
            var queue = new InboundQueue();
            var cmd = new SqlCommand("sp_CreateInboundQueue") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QueueID", queue.QueueId);
            cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
            cmd.Parameters.AddWithValue("@Subject", model.Subject);
            cmd.Parameters.AddWithValue("@Message", model.Message);
            cmd.Parameters.AddWithValue("@MessageType", model.MessageType);
            cmd.Parameters.AddWithValue("@MessageStatus", "new");
            cmd.Parameters.AddWithValue("@MessageTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@InteractionChannelTypeID", model.InteractionChannelTypeID);

            try
            {
                Logger.Write($"Creating inbound queue=[SMS], QueueId=[{queue.QueueId}], address=[{model.AccountName}], message=[{model.Message}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create inbound queue=[SMS]..", ex);
            }
        }

        public void CreateInboundQueue(InboundQueue model)
        {
            var queue = new InboundQueue();
            var cmd = new SqlCommand("sp_CreateInboundQueue") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QueueID", queue.QueueId);
            cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
            cmd.Parameters.AddWithValue("@Subject", model.Subject);
            cmd.Parameters.AddWithValue("@Message", model.Message);
            cmd.Parameters.AddWithValue("@MessageType", model.MessageType);
            cmd.Parameters.AddWithValue("@MessageStatus", "new");
            cmd.Parameters.AddWithValue("@MessageTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@InteractionChannelTypeID", model.InteractionChannelTypeID);

            try
            {
                Logger.Write($"Creating inbound queue=[General], QueueId=[{queue.QueueId}], address=[{model.AccountName}], subject=[{model.Subject}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create inbound queue..", ex);
            }
        }


        public void InsertMediaFile(InboundQueueFile file)
        {
            try
            {
                var cmd = new SqlCommand("sp_InsertInboundQueueFile") {CommandType = CommandType.StoredProcedure};
                cmd.Parameters.Add("@QueueID", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@Filename", SqlDbType.VarChar, 50);
                cmd.Parameters.Add("@FileData", SqlDbType.VarBinary, 8000);
                cmd.Parameters.Add("@IsAttachment", SqlDbType.Bit);

                //set values
                cmd.Parameters["@QueueID"].Value = file.QueueID;
                cmd.Parameters["@Filename"].Value = file.Filename;
                cmd.Parameters["@FileData"].Value = file.FileData;
                cmd.Parameters["@IsAttachment"].Value = file.IsAttachment;

                //execute command
                Logger.Write($"Create inbound queue file, QueueId=[{file.QueueID}], filename=[{file.Filename}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create inbound queue file..", ex);
            }
        }

        
    }
}
