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
    public class AdoOutboundQueueRepository
    {
        private readonly CDA _cda;
        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AdoOutboundQueueRepository(string connectionString)
        {
            _cda = new CDA(connectionString);
        }
        
        public ICollection<OutboundQueueFile> GetQueueFiles(string queueId)
        {
            var result = new List<OutboundQueueFile>();
            try
            {
                var cmd = new SqlCommand("sp_GetOutboundQueueFile");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@QueueID", queueId);

                Logger.Write($"Retrieving outbound queue files, QueueId=[{queueId}]", EventSeverity.Information);

                var dt = _cda.GetDataTable(cmd);
                result.AddRange(from DataRow row in dt.Rows
                    select new OutboundQueueFile
                    {
                        FileID = Convert.ToInt32(row["FileID"]), QueueID = row["QueueID"].ToString(), Filename = row["Filename"].ToString(), IsAttachment = row["IsAttachment"] != DBNull.Value && Convert.ToBoolean(row["IsAttachment"]), Url = row["Url"]?.ToString() ?? "", FileType = row["FileType"]?.ToString() ?? "", FileData = (byte[]) row["FileData"]
                    });
            }
            catch (Exception ex)
            {

                throw new Exception($"Failed to retrieve outbound queue file, QueueId=[{queueId}]", ex);
            }

            return result;
        }

        public void CreateMailQueue(MailMessage message)
        {
            var queue = new InboundQueue();
            var cmd = new SqlCommand("sp_CreateOutboundQueue") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QueueID", queue.QueueId);
            cmd.Parameters.AddWithValue("@AccountName", message.To);
            cmd.Parameters.AddWithValue("@Subject", message.Subject);
            cmd.Parameters.AddWithValue("@Message", message.Message);
            cmd.Parameters.AddWithValue("@MessageType", 1);
            cmd.Parameters.AddWithValue("@MessageStatus", "new");
            cmd.Parameters.AddWithValue("@MessageTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@InteractionChannelTypeID", 1);

            try
            {
                Logger.Write($"Creating outbound queue=[Mail], QueueId=[{queue.QueueId}], address=[{message.From}], subject=[{message.Subject}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create outbound queue=[Mail]..", ex);
            }


            foreach (var file in message.MediaFiles)
            {
                var media = new OutboundQueueFile
                {
                    QueueID = queue.QueueId,
                    IsAttachment = file.IsAttachment,
                    Filename = file.Filename,
                    FileData = file.FileData
                };

                InsertMediaFile(media);
            }
        }

        public void InsertMediaFile(OutboundQueueFile file)
        {
            try
            {
                var cmd = new SqlCommand("sp_InsertOutboundQueueFile") { CommandType = CommandType.StoredProcedure };
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
                Logger.Write($"Create outbound queue file, QueueId=[{file.QueueID}], filename=[{file.Filename}]", EventSeverity.Information);

                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create outbound queue file..", ex);
            }
        }

        public IEnumerable<OutboundQueue> GetOutboundQueues(int limit)
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_GetOutboundQueue");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Limit", limit);

                Logger.Write($"Retrieving outbound queues, limit=[{limit}]", EventSeverity.Information);
                var dt = _cda.GetDataTable(cmd);

                return (from DataRow row in dt.Rows
                        select new OutboundQueue
                        {
                            QueueId = row["QueueID"].ToString(),
                            AccountName = row["AccountName"].ToString(),
                            Subject =  row["Subject"]?.ToString() ?? "-",
                            Message = row["Message"].ToString(),
                            MessageType = Convert.ToInt32(row["MessageType"]),
                            MessageStatus = row["MessageStatus"].ToString(),
                            InteractionChannelTypeID = Convert.ToInt32(row["InteractionChannelTypeID"]),
                            AgentId = row["AgentID"] == DBNull.Value ? 0 : Convert.ToInt32(row["AgentID"]),
                            LastDistributedTime = row["LastDistributedTime"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["LastDistributedTime"]),
                            LastAgentID = row["LastAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(row["LastAgentID"]),
                            MediaFiles = GetQueueFiles(row["QueueID"].ToString())
                        }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve outbound queues..", ex);
            }
        }


        public IEnumerable<OutboundQueue> GetOutboundQueues(int channelId, int limit)
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_GetOutboundQueue");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@InteractionChannelID", channelId);
                cmd.Parameters.AddWithValue("@Limit", limit);

                Logger.Write($"Retrieving outbound queues, limit=[{limit}], channel=[{channelId}]", EventSeverity.Information);

                var dt = _cda.GetDataTable(cmd);

                return (from DataRow row in dt.Rows
                    select new OutboundQueue
                    {
                        QueueId = row["QueueID"].ToString(), AccountName = row["AccountName"].ToString(),
                        Subject = row["Subject"].ToString(),
                        Message = row["Message"].ToString(), MessageType = Convert.ToInt32(row["MessageType"]),
                        MessageStatus = row["MessageStatus"].ToString(),
                        InteractionChannelTypeID = channelId,
                        AgentId = row["AgentID"] ==   DBNull.Value ? 0 : Convert.ToInt32(row["AgentID"]),
                        LastDistributedTime = row["LastDistributedTime"] == DBNull.Value ? (DateTime?) null : Convert.ToDateTime(row["LastDistributedTime"]),
                        LastAgentID = row["LastAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(row["LastAgentID"]),
                        MediaFiles = GetQueueFiles(row["QueueID"].ToString())
                    }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve outbound queues, limit=[{limit}], channelId=[{channelId}]", ex);
            }
        }
        

        public void CreateQueue(OutboundQueue model)
        {
            var queue = new InboundQueue();
            var cmd = new SqlCommand("sp_CreateOutboundQueue") { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@QueueID", model.QueueId);
            cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
            cmd.Parameters.AddWithValue("@Message", model.Message);
            cmd.Parameters.AddWithValue("@MessageType", model.MessageType);
            cmd.Parameters.AddWithValue("@MessageStatus", "new");
            cmd.Parameters.AddWithValue("@MessageTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@InteractionChannelTypeID", model.InteractionChannelTypeID);
            cmd.Parameters.AddWithValue("@Subject", model.Subject);

            try
            {
                Logger.Write($"Creating outbound queue, ID=[{model.QueueId}], Account=[{model.AccountName}], Channel=[{model.InteractionChannelTypeID}], Subject=[{model.Subject}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (SqlException ex)
            {
                throw new Exception($"Failed to create outbound queue, ID=[{model.QueueId}], Account=[{model.AccountName}], Channel=[{model.InteractionChannelTypeID}], Subject=[{model.Subject}]", ex);
            }
        }

        public void Remove(string queueId)
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_RemoveOutboundQueue");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@QueueID", queueId);

                Logger.Write($"Removing outbound queue, ID=[{queueId}]", EventSeverity.Information);

                _cda.ExecuteNonQueryWithTransaction(cmd);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to remove outbound queue, ID : {queueId}", ex);
            }   
        }

    }
}
