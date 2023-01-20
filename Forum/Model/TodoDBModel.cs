using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Forum.Models;
using Forum.Models.DataModel;
using System.Configuration;

namespace Forum.Models
{
    public class ForumDBModel : RepositoryBase
    {
        public ForumDBModel() : base("Forum")
        {
        }

        public List<User> GetUser()
        {
            List<User> result = new List<User>();
            try
            {
                List<SqlParameter> parameter = new List<SqlParameter>();

                string sqlCommand = $@"SELECT [ID]
                                             ,[Account]
                                             ,[Name]                                   
                                             ,[CreateTime]
                                             FROM [User]";
                //parameter.Add(new SqlParameter() { DataType = SqlDbType.NVarChar, ParameterName = "@GroupName", ParameterValue = GroupName, ParameterLength = 50, ParameterType = ParameterDirection.Input });

                result = ReadMultiRow<User>(sqlCommand, parameter, CommandType.Text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}