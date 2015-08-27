using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    public class CinchMapping
    {
        public List<string> Columns { get; set; }
        public List<string> ValuesQueryParams { get; set; }
        public List<SqlParameter> SqlParams { get; set; }
    }

    public static class Mapper
    {
        public static CinchMapping MapProperties<T>(T obj, List<string> cols = null)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            List<string> columns = new List<string>();
            List<string> valuesQueryParams = new List<string>();
            List<SqlParameter> sqlParams = new List<SqlParameter>();

            int i = 1;
            foreach (PropertyInfo prop in props)
            {
                if (prop.Attributes.GetAttributeFrom<CinchIgnoreAttribute>(prop) != null ||
                    prop.DeclaringType == typeof(ModelBase) ||
                    (cols != null && cols.Contains(prop.Name)))
                    continue;

                object value = prop.GetValue(obj);

                if (value == null)
                    continue;

                Type t = prop.PropertyType;
                string placeholder = String.Format("val{0}", i);

                columns.Add(prop.Name);
                valuesQueryParams.Add(placeholder);

                sqlParams.AddParameter(placeholder, Conversion.GetSqlDbType(t), value);
              
                i++;
            }

            return new CinchMapping() { 
                Columns = columns,
                ValuesQueryParams = valuesQueryParams,
                SqlParams = sqlParams
            };
        }
    }
}
