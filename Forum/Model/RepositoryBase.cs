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
        /// Ū���@�����
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
        /// Ū���h�����
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
        /// ����åB��Ū����� 
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
        /// ����SQL�è��o�^�ǵ��G
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
        /// ����SP�è��o�^�ǵ��G
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
        /// SQL�Ѽ�
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
        private bool disposedValue = false; // �����h�l���I�s

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Forum: �B�m Managed ���A (Managed ����)�C
                    if (conn != null)
                    {
                        if (conn.State == ConnectionState.Open)
                            conn.Close();
                        conn.Dispose();
                    }
                }

                // Forum: ���� Unmanaged �귽 (Unmanaged ����) ���мg�U�誺�������C
                // Forum: �N�j�����]�� null�C

                disposedValue = true;
            }
        }

        // Forum: �ȷ�W�誺 Dispose(bool disposing) �㦳�|���� Unmanaged �귽���{���X�ɡA�~�мg�������C
        // ~RepositoryBase() {
        //   // �Ф��ܧ�o�ӵ{���X�C�бN�M���{���X��J�W�誺 Dispose(bool disposing) ���C
        //   Dispose(false);
        // }

        // �[�J�o�ӵ{���X���ت��b���T��@�i�B�m���Ҧ��C
        void IDisposable.Dispose()
        {
            // �Ф��ܧ�o�ӵ{���X�C�бN�M���{���X��J�W�誺 Dispose(bool disposing) ���C
            Dispose(true);
            // Forum: �p�G�W�誺�������w�Q�мg�A�Y�����U�檺���Ѫ��A�C
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}