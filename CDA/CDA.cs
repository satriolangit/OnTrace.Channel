using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace CommonDataAccess
{
    public class CDA
    {
        private string connString;

        public CDA(string connString)
        {
            this.connString = connString;
        }
        
        public SqlConnection GetDBConnection
        {
            get
            {
                SqlConnection dbconn = new SqlConnection(connString);
                return dbconn;
            }
        }

        /// <summary>
        /// Sends a Transact-SQL statement to the Connection and builds a SqlDataReader using one of the CommandBehavior values.
        /// CommandBehavior.CloseConnection is automaticaly added.
        /// </summary>
        /// <remarks>
        /// You must explicitly call the Close method of the SqlDataReader when you are through using the SqlDataReader.
        /// </remarks>
        public SqlDataReader ExecuteReader(SqlCommand cmd, CommandBehavior behavior)
        {
            SqlConnection myConnection = GetDBConnection;
            cmd.Connection = myConnection;

            myConnection.Open();
            
            SqlDataReader dr = null;
            try
            {
                if ((behavior & CommandBehavior.CloseConnection) != CommandBehavior.CloseConnection)
                    behavior |= CommandBehavior.CloseConnection;
                dr = cmd.ExecuteReader(behavior);
            }
            catch
            {
                try
                {
                    if (dr != null) dr.Close();
                }
                catch { }
                myConnection.Close();                
                throw;
            }

            return dr;
        }

        /// <summary>
        /// Sends a Transact-SQL statement to the Connection and builds a SqlDataReader using one of the CommandBehavior values.
        /// CommandBehavior.CloseConnection is automaticaly added.
        /// </summary>
        /// <remarks>
        /// You must explicitly call the Close method of the SqlDataReader when you are through using the SqlDataReader.
        /// </remarks>
        public SqlDataReader ExecuteReader(string sql, CommandBehavior behavior)
        {
            SqlCommand myCommand = new SqlCommand(sql);
            return ExecuteReader(myCommand, behavior);
        }

        /// <summary>
        /// Sends a Transact-SQL statement to the Connection and builds a SqlDataReader.
        /// </summary>
        /// <remarks>
        /// You must explicitly call the Close method of the SqlDataReader when you are through using the SqlDataReader
        /// </remarks>
        public SqlDataReader ExecuteReader(SqlCommand cmd)
        {
            return ExecuteReader(cmd, 0);
        }

        /// <summary>
        /// Sends a Transact-SQL statement to the Connection and builds a SqlDataReader.
        /// </summary>
        /// <remarks>
        /// You must explicitly call the Close method of the SqlDataReader when you are through using the SqlDataReader
        /// </remarks>
        public SqlDataReader ExecuteReader(string sql)
        {
            SqlCommand myCommand = new SqlCommand(sql);
            return ExecuteReader(myCommand);
        }

        /// <summary>
        /// Sends a Transact-SQL statement to the Connection and builds a SqlDataReader.
        /// </summary>
        /// <remarks>
        /// You must explicitly call the Close method of the SqlDataReader when you are through using the SqlDataReader
        /// </remarks>
        public SqlDataReader ExecuteReaderSingleRow(string sql)
        {
            SqlCommand myCommand = new SqlCommand(sql);
            return ExecuteReader(myCommand, CommandBehavior.SingleRow);
        }

        /// <summary>
        /// Sends a Transact-SQL statement to the Connection and builds a SqlDataReader.
        /// </summary>
        /// <remarks>
        /// You must explicitly call the Close method of the SqlDataReader when you are through using the SqlDataReader
        /// </remarks>
        public SqlDataReader ExecuteReaderSingleRow(SqlCommand cmd)
        {
            return ExecuteReader(cmd, CommandBehavior.SingleRow);
        }

        /// <summary>
        /// Adds or refreshes rows in the DataSet to match those in the data source using the DataSet name, and creates a DataTable named "Table".
        /// </summary>
        public DataSet GetDataSet(SqlCommand sqlcom)
        {
            SqlConnection myconn = GetDBConnection;
            sqlcom.Connection = myconn;

            SqlDataAdapter da = new SqlDataAdapter(sqlcom);
            DataSet ds = new DataSet();

            try
            {
                da.Fill(ds);
            }
            finally
            {
                myconn.Close();
            }
            return ds;
        }


        public DataSet GetDataset(string sql, string tablename)
        {
            SqlConnection myconn = GetDBConnection;

            SqlDataAdapter da = new SqlDataAdapter(sql, myconn);
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, tablename);
            }
            finally
            {
                myconn.Close();
            }
            return ds;
        }

        /// <summary>
        /// Adds or refreshes rows in the DataSet to match those in the data source using the DataSet name, and creates a DataTable named "Table".
        /// </summary>
        public DataSet GetDataSet(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return GetDataSet(cmd);
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQuery(SqlCommand cmd)
        {
            SqlConnection myconn = GetDBConnection;
            cmd.Connection = myconn; 

            int affectedRow = 0;
            myconn.Open();            
            try
            {
                affectedRow = cmd.ExecuteNonQuery();
            }
            finally
            {
                myconn.Close();                
            }
            return affectedRow;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQuery(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQueryWithTransaction(SqlCommand cmd)
        {
            SqlConnection myconn = GetDBConnection;
            cmd.Connection = myconn;
            int affectedRow = 0;
            myconn.Open();
            
            SqlTransaction myTrans = myconn.BeginTransaction();
            try
            {
                cmd.Transaction = myTrans;
                affectedRow = cmd.ExecuteNonQuery();
                myTrans.Commit();
            }
            catch
            {
                try { myTrans.Rollback(); }
                catch { }
                throw;
            }
            finally
            {
                myconn.Close();
            }
            return affectedRow;
        }

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        public int ExecuteNonQueryWithTransaction(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql);
            return ExecuteNonQueryWithTransaction(cmd);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar(SqlCommand cmd)
        {
            SqlConnection myconn = GetDBConnection;
            cmd.Connection = myconn;
            object scalar = null;
            
            myconn.Open();
            try
            {
                scalar = cmd.ExecuteScalar();
            }
            finally
            {
                myconn.Close();
            }
            return scalar;
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        public object ExecuteScalar(string sql)
        {
            SqlCommand myCommand = new SqlCommand(sql);
            return ExecuteScalar(myCommand);
        }

        /// <summary>
        /// Adds or refreshes rows in a DataTable to match those in the data source using the DataTable name.
        /// </summary>
        public DataTable GetDataTable(SqlCommand cmd)
        {
            SqlConnection myconn = GetDBConnection;
            cmd.Connection = myconn;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            try
            {
                da.Fill(dt);
            }
            finally
            {
                myconn.Close();                
            }
            return dt;
        }

        /// <summary>
        /// Adds or refreshes rows in a DataTable to match those in the data source using the DataTable name.
        /// </summary>
        public DataTable GetDataTable(string sql)
        {
            SqlCommand myCommand = new SqlCommand(sql);
            return GetDataTable(myCommand);
        }		
    }
}
