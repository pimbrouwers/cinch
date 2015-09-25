using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CinchORM;

namespace CinchORM
{
    public static class Cinch
    {
        /// <summary>
        /// Checks if an object exists using the primary key.
        /// </summary>
        /// <param name="obj">The object to be searched.</param>
        /// <returns>The number of existent objects.</returns>
        public static int Exists<T>(T obj) where T : IModelBase, IModelIdentity, IModelName
        {
            string query = String.Format(Queries.Exists, obj.TableNameFullyQualified, obj.TableName, obj.PrimaryKeyFullyQualified, obj.PrimaryKeyFullyQualified);

            if (obj.ID > 0)
            {
                using (DataConnect dc = new DataConnect(query, CommandType.Text))
                {
                    dc.SetQuery(query);
                    dc.AddParameter("value", SqlDbType.Int, obj.ID);

                    try
                    {
                        return dc.ExecuteScalarInt();
                    }
                    catch (Exception ex)
                    {
                        //do something useful here
                        throw ex;
                    }
                }
            }
            else
                throw new ApplicationException(String.Format("Could not check Existance for {0} because the objects primary key value is not set", obj.objName), new NullReferenceException());
        }

        /// <summary>
        /// Checks if an object exists using any of its properties.
        /// </summary>
        /// <param name="obj">The object to be searched.</param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns>The number of existent objects.</returns>
        public static int Exists<T>(T obj, PropertyInfo column, object value) where T : IModelBase, IModelName
        {
            string query = String.Format(Queries.Exists, obj.TableNameFullyQualified, obj.TableName, String.Format("[{0}].[{1}]", obj.TableName, column.Name), obj.PrimaryKeyFullyQualified);
            if (value != null)
            {
                using (DataConnect dc = new DataConnect(query, CommandType.Text))
                {
                    dc.SetQuery(query);
                    dc.AddParameter("value", Conversion.GetSqlDbType(value.GetType()), value);

                    try
                    {
                        return dc.ExecuteScalarInt();
                    }
                    catch (Exception ex)
                    {
                        //do something useful here
                        throw ex;
                    }
                }
            }
            else
                throw new ApplicationException(String.Format("Could not check Existance for {0} because the column value is not set", obj.objName), new NullReferenceException());
        }

        /// <summary>
        /// Find first by primary key of existing object
        /// </summary>
        /// <returns></returns>
        public static T FindFirst<T>(T obj) where T : IModelBase, IModelIdentity, IModelName
        {

            if (obj.ID > 0)
            {
                using (DataConnect dc = new DataConnect(null, CommandType.Text))
                {
                    string query = String.Format(Queries.FindFirst, obj.ColumnsFullyQualified, obj.TableNameFullyQualified, obj.TableName, obj.PrimaryKeyFullyQualified);
                    dc.SetQuery(query);
                    dc.AddParameter("id", SqlDbType.Int, obj.ID);

                    return dc.FillObject<T>();
                }
            }
            else
                throw new ApplicationException(String.Format("Could not FindFirst for {0} because the objects primary key value is not set", obj.objName), new NullReferenceException());

        }

        /// <summary>
        /// Find first based on explicit primary key value
        /// </summary>
        /// <returns></returns>
        public static T FindFirst<T>(T obj, int ID) where T : IModelBase
        {
            T result = default(T);

            if (obj != null)
            {
                string query = String.Format(Queries.FindFirst, obj.ColumnsFullyQualified, obj.TableNameFullyQualified, obj.TableName, obj.PrimaryKeyFullyQualified);

                using (DataConnect dc = new DataConnect(query, CommandType.Text))
                {
                    dc.SetQuery(query);
                    dc.AddParameter("id", SqlDbType.Int, ID);

                    result = dc.FillObject<T>();
                }
            }

            return result;
        }

        /// <summary>
        /// Find first based on explicit primary key value
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="ID">The primary key to be searched.</param>
        /// <returns>The object if found; null, otherwise.</returns>
        public static T FindFirst<T>(int ID) where T : IModelBase, new()
        {
            var obj = new T();

            return Cinch.FindFirst<T>(obj, ID);
        }

        /// <summary>
        /// Finds all as List based on where / param
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> Find<T>(T obj, string where = null, object[] param = null) where T : IModelBase, IModelName
        {
            List<T> result = new List<T>();

            if (obj != null)
            {
                CinchMapping mappings = Mapper.MapQuery<T>(obj, where, param);

                //make sure we have a WHERE 
                if (!string.IsNullOrWhiteSpace(mappings.QueryString) &&
                    mappings.QueryString.ToLowerInvariant().IndexOf("where") == -1)
                {
                    mappings.QueryString = String.Format("WHERE {0}", mappings.QueryString);
                }

                using (DataConnect dc = new DataConnect(null, CommandType.Text))
                {
                    string query = String.Format(Queries.Find, obj.ColumnsFullyQualified, obj.TableNameFullyQualified, obj.TableName, mappings.QueryString);
                    dc.SetQuery(query);

                    if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
                        dc.AddParameters(mappings.SqlParams);

                    result = dc.FillList<T>();
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all as List based on where / param
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> PagedFind<T>(
            T obj, 
            string where = null,
            object[] param = null, 
            string orderBy = "",
            int page = 1,
            int pageSize = 100) where T : IModelBase, IModelName
        {
            List<T> result = new List<T>();

            if (obj != null)
            {
                CinchMapping mappings = Mapper.MapQuery<T>(obj, where, param);

                //make sure we have a WHERE 
                if (!string.IsNullOrWhiteSpace(mappings.QueryString) &&
                    mappings.QueryString.ToLowerInvariant().IndexOf("where") == -1)
                {
                    mappings.QueryString = String.Format("WHERE {0}", mappings.QueryString);
                }

                using (DataConnect dc = new DataConnect(null, CommandType.Text))
                {
                    var lowerIndex = ((page - 1) * pageSize) + 1;
                    var upperIndex = lowerIndex + pageSize;

                    string query = String.Format(
                        Queries.PagedFind, 
                        obj.ColumnsFullyQualified, 
                        obj.TableNameFullyQualified, 
                        obj.TableName, 
                        mappings.QueryString,
                        orderBy,
                        lowerIndex,
                        upperIndex);

                    dc.SetQuery(query);

                    if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
                        dc.AddParameters(mappings.SqlParams);

                    result = dc.FillList<T>();
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all as List based on where / param
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> Find<T>(
            string where = null,
            object[] param = null) where T : IModelBase, IModelName, new()
        {
            T obj = new T();

            return Cinch.Find(obj, where, param);
        }

        /// <summary>
        /// Finds all as List based on where / param
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> PagedFind<T>(
            string where = null,
            object[] param = null,
            string orderBy = "",
            int page = 1,
            int pageSize = 100) where T : IModelBase, IModelName, new()
        {
            T obj = new T();

            return Cinch.PagedFind(obj, where, param, orderBy, page, pageSize);
        }

        /// <summary>
        /// Inserts an object to the database.
        /// </summary>
        /// <param name="obj">The object to be inserted.</param>
        /// <param name="cols">The whitelist of columns to be inserted.</param>
        /// <returns>Int32</returns>
        /// <exception cref="System.Exception">
        /// Throws <seealso cref="System.Exception"/>.
        /// </exception>
        public static int Insert<T>(T obj, List<string> cols = null) where T : IModelBase
        {
            using (DataConnect dc = new DataConnect(null, CommandType.Text))
            {
                CinchMapping mappings = Mapper.MapProperties(obj, cols);
                string query = String.Format(Queries.Insert, obj.TableNameFullyQualified, mappings.ColumnsString, mappings.InsertValuesQueryParamsString);
                dc.SetQuery(query);
                dc.AddParameters(mappings.SqlParams);

                try
                {
                    return dc.ExecuteScalarInt();
                }
                catch (Exception ex)
                {
                    //do something useful here
                    throw ex;
                }
            }
        }

        /// <summary>
        /// UPDATE
        /// </summary>
        /// <returns>Int32</returns>
        public static int Update<T>(T obj, List<string> cols = null) where T : IModelBase, IModelIdentity
        {
            using (DataConnect dc = new DataConnect(null, CommandType.Text))
            {
                CinchMapping mappings = Mapper.MapProperties(obj, cols);
                string query = String.Format(Queries.Update, obj.TableNameFullyQualified, mappings.UpdateValuesQueryParamsString, obj.PrimaryKeyFullyQualified);
                dc.SetQuery(query);
                dc.AddParameters(mappings.SqlParams);
                dc.AddParameter("id", SqlDbType.Int, obj.ID);

                try
                {
                    return dc.ExecuteScalarInt();
                }
                catch (Exception ex)
                {
                    //do something useful here
                    throw ex;
                }
            }
        }

        public static T ExecCustom<T>(T obj, string query, object[] param = null) where T : IModelBase, IModelName
        {
            CinchMapping mappings = Mapper.MapQuery<T>(obj, query, param);

            using (DataConnect dc = new DataConnect(null, CommandType.Text))
            {
                dc.SetQuery(mappings.QueryString);

                if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
                    dc.AddParameters(mappings.SqlParams);

                return dc.FillObject<T>();
            }
        }

        private static string AppendWhereClause(string value)
        {
            var result = value;

            //make sure we have a WHERE 
            if (!string.IsNullOrWhiteSpace(value) &&
                value.ToLowerInvariant().IndexOf("where") == -1)
            {
                result = String.Format("WHERE {0}", value);
            }

            return result;
        }
    }
}
