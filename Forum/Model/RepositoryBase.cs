using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Configuration;

namespace Forum.Models
{
    public class RepositoryBase : IDisposable
    {
        private string _ConnectString;
        private SqlConnection conn;
        public RepositoryBase(string DBName)
        {
            //this._ConnectString = WebConfigurationManager.ConnectionStrings[DBName].ConnectionString;

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false);
            IConfiguration config = builder.Build();        
            this._ConnectString = config.GetConnectionString(DBName);
            conn = new SqlConnection(this._ConnectString);
        }

        /// <summary>
        /// 讀取一筆資料
        /// </summary>
        /// <typeparam name="To"></typeparam>
        /// <param name="InputPatameter"></param>
        /// <param name="SqlCommand"></param>
        /// <param name="SqlType"></param>
        /// <returns></returns>
        public To ReadFirstRow<To>(string SqlCommand, List<SqlParameter> InputPatameter, CommandType SqlType)
        {
            To Result = Activator.CreateInstance<To>();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = new SqlCommand(SqlCommand, conn);
            cmd.CommandTimeout = 300;
            cmd.CommandType = SqlType;
            foreach (SqlParameter s in InputPatameter)
                cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Value = s.ParameterValue;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo property = Result.GetType().GetProperty(reader.GetName(i));
                        if (property != null && !reader.GetValue(i).Equals(DBNull.Value))
                            property.SetValue(Result, (reader.IsDBNull(i)) ? "[NULL]" : reader.GetValue(i));
                    }
                }
            }
            return Result;
        }

        /// <summary>
        /// 讀取多筆資料
        /// </summary>
        /// <typeparam name="To"></typeparam>
        /// <param name="InputPatameter"></param>
        /// <param name="SqlCommand"></param>
        /// <param name="SqlType"></param>
        /// <returns></returns>
        public List<To> ReadMultiRow<To>(string SqlCommand, List<SqlParameter> InputPatameter, CommandType SqlType)
        {
            List<To> Result = Activator.CreateInstance<List<To>>();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = new SqlCommand(SqlCommand, conn);
            cmd.CommandTimeout = 300;
            cmd.CommandType = SqlType;
            foreach (SqlParameter s in InputPatameter)
                cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Value = s.ParameterValue;
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    To item = Activator.CreateInstance<To>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PropertyInfo property = item.GetType().GetProperty(reader.GetName(i));
                        if (property != null && !reader.GetValue(i).Equals(DBNull.Value))
                            property.SetValue(item, (reader.IsDBNull(i)) ? "[NULL]" : reader.GetValue(i));
                    }
                    Result.Add(item);
                }
            }
            return Result;
        }


        //public List<To> ReadMultiRow_Try<To>(string SqlCommand, List<SqlParameter> InputPatameter, CommandType SqlType)
        //{
        //    List<To> Result = Activator.CreateInstance<List<To>>();
        //    if (conn.State != ConnectionState.Open)
        //        conn.Open();
        //    SqlCommand cmd = new SqlCommand(SqlCommand, conn);
        //    cmd.CommandTimeout = 300;
        //    cmd.CommandType = SqlType;
        //    foreach (SqlParameter s in InputPatameter)
        //        cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Value = s.ParameterValue;
        //    using (SqlDataReader reader = cmd.ExecuteReader())
        //    {
        //        while (reader.Read())
        //        {
        //            To item = Activator.CreateInstance<To>();
        //            for (int i = 0; i < reader.FieldCount; i++)
        //            {
        //                FieldInfo myFieldInfo = item.GetType().GetField("PartNumber");
        //                myFieldInfo.SetValue(item, "New value");

        //            }
        //            Result.Add(item);
        //        }
        //    }
        //    return Result;
        //}

        /// <summary>
        /// 執行並且不讀取資料 
        /// </summary>
        /// <param name="InputPatameter"></param>
        /// <param name="SqlCommand"></param>
        /// <param name="SqlType"></param>
        public int ExecuteNonRead(string SqlCommand, List<SqlParameter> InputPatameter, CommandType SqlType)
        {
            int rowCount = 0;
            if (conn.State != ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = new SqlCommand(SqlCommand, conn);
            cmd.CommandTimeout = 300;
            cmd.CommandType = SqlType;
            foreach (SqlParameter s in InputPatameter)
                cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Value = s.ParameterValue;
            rowCount = cmd.ExecuteNonQuery();
            return rowCount;
        }

        /// <summary>
        /// 執行SQL並取得回傳結果
        /// </summary>
        /// <param name="InputPatameter"></param>
        /// <param name="SqlCommand"></param>
        /// <param name="SqlType"></param>
        public Dictionary<string, object> ExecuteWithOutput(string SqlCommand, List<SqlParameter> InputPatameter)
        {
            Dictionary<string, object> OutputParameter = new Dictionary<string, object>();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            SqlCommand cmd = new SqlCommand(SqlCommand, conn);
            cmd.CommandTimeout = 300;
            cmd.CommandType = CommandType.Text;
            foreach (SqlParameter s in InputPatameter)
            {
                if (s.ParameterType == ParameterDirection.Input)
                    cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Value = s.ParameterValue;
                else if (s.ParameterType == ParameterDirection.Output)
                    cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Direction = ParameterDirection.Output;
            }
            //cmd.Parameters.Add("@MemberAccount", SqlDbType.NVarChar, 200).Value = "";
            //cmd.Parameters.Add("@Result", SqlDbType.Bit, 0).Direction = ParameterDirection.Output;
            cmd.ExecuteNonQuery();
            foreach (SqlParameter s in InputPatameter)
            {
                if (s.ParameterType == ParameterDirection.Output)
                    OutputParameter.Add(s.ParameterName, cmd.Parameters[s.ParameterName].Value);
            }
            return OutputParameter;
        }

        /// <summary>
        /// 執行SP並取得回傳結果
        /// </summary>
        /// <param name="InputPatameter"></param>
        /// <param name="SqlCommand"></param>
        /// <param name="SqlType"></param>
        public Dictionary<string, object> ExecuteSPWithOutput(string SqlCommand, List<SqlParameter> InputPatameter)
        {
            Dictionary<string, object> OutputParameter = new Dictionary<string, object>();
            using (SqlConnection conn = new SqlConnection(_ConnectString))
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                SqlCommand cmd = new SqlCommand(SqlCommand, conn);
                cmd.CommandTimeout = 300;
                cmd.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter s in InputPatameter)
                {
                    if (s.ParameterType == ParameterDirection.Input)
                        cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Value = s.ParameterValue;
                    else if (s.ParameterType == ParameterDirection.Output)
                        cmd.Parameters.Add(s.ParameterName, s.DataType, s.ParameterLength).Direction = ParameterDirection.Output;
                }
                //cmd.Parameters.Add("@MemberAccount", SqlDbType.NVarChar, 200).Value = "";
                //cmd.Parameters.Add("@Result", SqlDbType.Bit, 0).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                foreach (SqlParameter s in InputPatameter)
                {
                    if (s.ParameterType == ParameterDirection.Output)
                        OutputParameter.Add(s.ParameterName, cmd.Parameters[s.ParameterName].Value);
                }
            }
            return OutputParameter;
        }

        /// <summary>
        /// SQL參數
        /// </summary>
        public class SqlParameter
        {
            public SqlDbType DataType;
            public string ParameterName;
            public object ParameterValue;
            public short ParameterLength;
            public ParameterDirection ParameterType;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Forum: 處置 Managed 狀態 (Managed 物件)。
                    if (conn != null)
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Dispose();
                    }
                }

                // Forum: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // Forum: 將大型欄位設為 null。

                disposedValue = true;
            }
        }

        // Forum: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        // ~RepositoryBase() {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        void IDisposable.Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // Forum: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}