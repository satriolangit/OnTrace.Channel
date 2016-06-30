using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonDataAccess;
using OnTrace.Channel.Core.Domain;
using OnTrace.Channel.Core.Interfaces;
using OnTrace.Channel.Infrastructure.Logger;

namespace OnTrace.Channel.Infrastructure.Data
{
    public class AdoMasterDataRepository
    {
        private readonly CDA _cda;
        private static readonly ILogger Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AdoMasterDataRepository(string connectionString)
        {
            _cda = new CDA(connectionString);
        }

        /// <summary>
        /// Type : incoming / outgoing
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public MailAccount GetMailAccount(string type)
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_GetMailAccount");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", type);
                
                var dt = _cda.GetDataTable(cmd);
                var account = new MailAccount();

                foreach (DataRow row in dt.Rows)
                {
                    account.SettingType = row["SettingType"].ToString();
                    account.Username = row["Username"].ToString();
                    account.Password = row["Password"].ToString();
                    account.Server = row["Server"].ToString();
                    account.Port = Convert.ToInt32(row["Port"]);
                    account.SecurityType = row["SecurityType"].ToString();
                    account.EnableSsl = Convert.ToBoolean(row["EnableSSL"]);
                }

                return account;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrive mail account.", ex);
            }
        }

        
        public ModemSetting GetModemSetting()
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_GetModemSetting");
                cmd.CommandType = CommandType.StoredProcedure;

                var dt = _cda.GetDataTable(cmd);
                var setting = new ModemSetting();

                foreach (DataRow row in dt.Rows)
                {
                    setting.Port = Convert.ToInt32(row["COMPort"]);
                    setting.BaudRate = Convert.ToInt32(row["BaudRate"]);
                    setting.Timeout = Convert.ToInt32(row["Timeout"]);
                }

                return setting;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve modem setting", ex);
            }
        }

        public ChannelType GetChannelType(string code)
        {
            try
            {
                var cmd = new SqlCommand("sp_OC_GetChannelType");
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Code", code);

                var dt = _cda.GetDataTable(cmd);
                var result = new ChannelType();

                foreach (DataRow row in dt.Rows)
                {
                    result.Code = row["Code"].ToString();
                    result.Description = row["Description"].ToString();
                    result.InteractionChannelTypeId = Convert.ToInt32(row["InteractionChannelTypeID"]);

                }

                return result;

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve channel type, type=[{code}]", ex);
            }
        }
        
    }
}
