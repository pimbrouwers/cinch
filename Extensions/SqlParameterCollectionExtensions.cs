using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    public static class SqlParameterCollectionExtensions
    {
        /// <summary>
        /// Sets up a parameter for the query
        /// </summary>
        /// <param name="id">The ID of the parameter</param>
        /// <param name="type">The Sql type of the parameter</param>
        /// <param name="Value">The value of the parameter</param>
        public static void AddParameter(this List<SqlParameter> collection, string parameterName, SqlDbType type, Object Value)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = parameterName;
            parameter.SqlDbType = type;           

            if (Value == null)
            {
                parameter.Value = Convert.DBNull;
            }
            else if (Value.ToString() == "" && type != SqlDbType.VarChar)
            {
                // must convert the empty string to a DBNull
                parameter.Value = Convert.DBNull;
            }
            else if (Value.ToString() == "" && (type == SqlDbType.Float || type == SqlDbType.Int || type == SqlDbType.Money))
            {
                parameter.Value = 0;
            }
            else
            {
                // set the value of the parameter
                parameter.Value = Value;
            }

            collection.Add(parameter);
        }
    }
}
