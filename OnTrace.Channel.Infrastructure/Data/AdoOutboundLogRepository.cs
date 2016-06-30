using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Entities;
using CommonDataAccess;
using OnTrace.Channel.Core.Interfaces;
using OnTrace.Channel.Infrastructure.Logger;

namespace OnTrace.Channel.Infrastructure.Data
{
    public class AdoOutboundLogRepository
    {
        private readonly CDA _cda;
        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AdoOutboundLogRepository(string connectionString)
        {
            _cda = new CDA(connectionString);
        }
        public void CreateLog(OutboundLog model)
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_CreateOutboundLog");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LogID", model.LogId);
                cmd.Parameters.AddWithValue("@AgentID", model.AgentId);
                cmd.Parameters.AddWithValue("@AccountName", model.AccountName);
                cmd.Parameters.AddWithValue("@Subject", model.Subject);
                cmd.Parameters.AddWithValue("@Message", model.Message);
                cmd.Parameters.AddWithValue("@MessageType", model.MessageType);
                cmd.Parameters.AddWithValue("@MessageStatus", model.MessageStatus);
                cmd.Parameters.AddWithValue("@InteractionChannelTypeID", model.InteractionChannelTypeId);
                cmd.Parameters.AddWithValue("@LastDistributedTime", model.LastDistributedTime);
                cmd.Parameters.AddWithValue("@LastAgentID", model.LastAgentId);
                cmd.Parameters.AddWithValue("@MessageTime", model.MessageTime);

                Logger.Write($"Create outbound log, LogId=[{model.LogId}], AgentId=[{model.AgentId}], Account=[{model.AccountName}], channel=[{model.InteractionChannelTypeId}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

                foreach (var file in model.MediaFiles)
                {
                    CreateFileLog(file, model.LogId);
                }

            }
            catch (Exception ex)
            {
                
                throw new Exception("Failed to create outbound log.", ex);
            }
        }

        public void CreateFileLog(OutboundFileLog model)
        {
            try
            {
                if (model == null) return;

                var cmd = new SqlCommand("sp_OC_CreateOutboundFileLog");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LogID", model.LogId);
                cmd.Parameters.AddWithValue("@Filename", model.Filename);
                cmd.Parameters.AddWithValue("@FileType", model.FileType);
                cmd.Parameters.AddWithValue("@Url", model.Url);
                cmd.Parameters.AddWithValue("@FileData", model.FileData);
                cmd.Parameters.AddWithValue("@IsAttachment", model.IsAttachment);

                Logger.Write($"Create outbound log file, LogId=[{model.LogId}], Filename=[{model.Filename}]", EventSeverity.Information);

                _cda.ExecuteNonQueryWithTransaction(cmd);
                

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create outbound log file", ex);
            }
        }

        public void CreateFileLog(OutboundQueueFile model, string logId)
        {
            try
            {
                if (model == null) return;

                var cmd = new SqlCommand("sp_OC_CreateOutboundFileLog") {CommandType = CommandType.StoredProcedure};
                cmd.Parameters.AddWithValue("@LogID", logId);
                cmd.Parameters.AddWithValue("@Filename", model.Filename);
                cmd.Parameters.AddWithValue("@FileType", model.FileType);
                cmd.Parameters.AddWithValue("@Url", model.Url);
                cmd.Parameters.AddWithValue("@FileData", model.FileData);
                cmd.Parameters.AddWithValue("@IsAttachment", model.IsAttachment);

                Logger.Write($"Create outbound log file, LogId=[{logId}], Filename=[{model.Filename}]", EventSeverity.Information);
                _cda.ExecuteNonQueryWithTransaction(cmd);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create outbound log file.", ex);
            }
        }
    }
}
