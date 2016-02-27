using CinchORM.AccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CinchORM
{

    public class CinchMapping
    {
        public List<string> Columns { get; set; }
        public string ColumnsString
        {
            get
            {
                if(this.Columns == null || Columns.Count == 0)
                    throw new ApplicationException(String.Format("Could not build ColumnsString because the objects ValuesQueryParams is null or empty"), new NullReferenceException());
                
                return string.Join(",", this.Columns);
            }
        }
        public string QueryString { get; set; }

        public List<string> ValuesQueryParams { get; set; }
        public string InsertValuesQueryParamsString
        {
            get
            {
                if(this.ValuesQueryParams == null || this.ValuesQueryParams.Count == 0)
                    throw new ApplicationException(String.Format("Could not build ValuesQueryParamsString because the objects ValuesQueryParams is null or empty"), new NullReferenceException());

                return string.Join(",", this.ValuesQueryParams);
            }
        }
        public string UpdateValuesQueryParamsString
        {
            get
            {
                if (this.ValuesQueryParams == null || this.ValuesQueryParams.Count == 0 || this.Columns == null || Columns.Count == 0)
                    throw new ApplicationException(String.Format("Could not build ValuesQueryParamsString because the objects ValuesQueryParams is null or empty"), new NullReferenceException());
                
                string updateValuesQueryParamsString = null;

                for (int i = 0; i < this.Columns.Count; i++)
			    {
			        string col = this.Columns[i];
                    string val = this.ValuesQueryParams[i];
                    updateValuesQueryParamsString = String.Format("{0}{1} = {2},", updateValuesQueryParamsString, col, val);
			    }

                if (updateValuesQueryParamsString.Substring(updateValuesQueryParamsString.Length - 1, 1) == ",")
                    return updateValuesQueryParamsString.Substring(0, updateValuesQueryParamsString.Length - 1);

                return updateValuesQueryParamsString;

            }
        }
        
        public List<SqlParameter> SqlParams { get; set; }
    }

    public static class Mapper
    {
        internal static Dictionary<string, CinchMapping> _cinchMappingCache = new Dictionary<string, CinchMapping>();
        
        public static CinchMapping MapProperties<T>(T obj, List<string> cols = null) where T : ModelBase
        {
            string cacheKey = String.Format("{0}_{1}", obj.ObjName, String.Join("-", cols.ToArray()));
            CinchMapping cinchMapping = null;

            if(!_cinchMappingCache.ContainsKey(cacheKey))
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

#if NET40
                    object value = prop.GetValue(obj, null);
#else
                object value = prop.GetValue(obj);
#endif

                    if (value == null)
                        continue;

                    Type t = prop.PropertyType;
                    string placeholder = String.Format("val{0}", i);

                    columns.Add(String.Format("{0}{1}{2}", SpecialCharacters.ColumnBegin, prop.Name, SpecialCharacters.ColumnEnd));
                    valuesQueryParams.Add(String.Format("{0}{1}", SpecialCharacters.ParamPrefix, placeholder));

                    sqlParams.AddParameter(placeholder, Conversion.GetSqlDbType(t), value);

                    i++;
                }

                cinchMapping = new CinchMapping()
                {
                    Columns = columns,
                    ValuesQueryParams = valuesQueryParams,
                    SqlParams = sqlParams
                };

                _cinchMappingCache.Add(cacheKey, cinchMapping);
            }
            else
            {
                cinchMapping = _cinchMappingCache[cacheKey];
            }

            return cinchMapping;
        }

        public static CinchMapping MapQuery<T>(T obj, string query, object[] param) where T : ModelBase
        {
            string cacheKey = String.Format("{0}_{1}_{2}", obj.ObjName, query, String.Join("-", param));
            CinchMapping cinchMapping = null;

            if(!_cinchMappingCache.ContainsKey(cacheKey))
            {
                cinchMapping = new CinchMapping() { QueryString = query };
                
                if (param != null && param.Count() > 0)
                {
                    //where clause has params, but no param values were passed in.
                    if (!String.IsNullOrWhiteSpace(query) && query.IndexOf('@') > -1 && param.Count() <= 0)
                        throw new ApplicationException(String.Format("Could not execute Find for {0} because the parameters array is empty", obj.ObjName), new NullReferenceException());
                    //param counts don't match
                    else if (query.Count(c => c == '@') != param.Count())
                        throw new ApplicationException(String.Format("Could not execute Find for {0} because the number of parameters in the where clause and parameters array do not match", obj.ObjName), new NullReferenceException());

                    List<SqlParameter> sqlParams = BuildParamsFromString(query, param);

                    cinchMapping.SqlParams = sqlParams;
                }

                _cinchMappingCache.Add(cacheKey, cinchMapping);
            }
            else
            {
                cinchMapping = _cinchMappingCache[cacheKey];
            }
            
            return cinchMapping;
        }

        public static CinchMapping MapQuery(string query, object[] param)
        {
            string cacheKey = String.Format("{0}_{1}", query, String.Join("-", param));
            CinchMapping cinchMapping = null;

            if (!_cinchMappingCache.ContainsKey(cacheKey))
            {
                cinchMapping = new CinchMapping() { QueryString = query };

                if (param != null && param.Count() > 0)
                {
                    //where clause has params, but no param values were passed in.
                    if (!String.IsNullOrWhiteSpace(query) && query.IndexOf('@') > -1 && param.Count() <= 0)
                        throw new ApplicationException(String.Format("Could not execute Find for \"{0}\" because the parameters array is empty", query), new NullReferenceException());
                    //param counts don't match
                    else if (query.Count(c => c == '@') != param.Count())
                        throw new ApplicationException(String.Format("Could not execute Find for \"{0}\" because the number of parameters in the where clause and parameters array do not match", query), new NullReferenceException());

                    List<SqlParameter> sqlParams = BuildParamsFromString(query, param);

                    cinchMapping.SqlParams = sqlParams;
                }

                _cinchMappingCache.Add(cacheKey, cinchMapping);
            }
            else
            {
                cinchMapping = _cinchMappingCache[cacheKey];
            }

            return cinchMapping;
        }

        private static List<SqlParameter> BuildParamsFromString(string query, object[] param)
        {
            Regex regex = new Regex("@[A-Za-z0-9]+");
            MatchCollection matches = regex.Matches(query);

            //param matches don't match param array
            if (matches.Count != param.Count())
                throw new ApplicationException(String.Format("Could not build Params because the number of parameters in the where clause and parameters array do not match"), new NullReferenceException());

            List<SqlParameter> sqlParams = new List<SqlParameter>();

            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                object value = param[i];

                //add sql param
                sqlParams.AddParameter(match.Value, Conversion.GetSqlDbType(value.GetType()), value);
            }

            return sqlParams;
        }
    }
}
