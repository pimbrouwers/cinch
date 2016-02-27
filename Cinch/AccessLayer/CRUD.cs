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
    public static class CRUD
    {

        /// <summary>
        /// Checks if an object exists by primary key
        /// </summary>
        /// <param name="obj">The object to be searched.</param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns>The number of existent objects.</returns>
        public static int Exists<T>(int id) where T : ModelBase, new()
        {
            var obj = new T();
            int result = -1;

            if (obj != null)
            {
                string query = String.Format(Queries.FindFirst, obj.ColumnsFullyQualified, obj.TableNameFullyQualified, obj.TableName, obj.PrimaryKeyFullyQualified);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    cinch.AddParameter("id", SqlDbType.Int, id);
                    result = cinch.ExecuteScalarInt();
                }
            }

            return result;
        }

        /// <summary>
        /// Count by any criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int Count<T>(string where = null, object[] param = null) where T : ModelBase, new()
        {
            T obj = new T();
            int result = 0;

            if (obj != null)
            {
                CinchMapping mappings = Mapper.MapQuery<T>(obj, where, param);

                //make sure we have a WHERE 
                if (!string.IsNullOrWhiteSpace(mappings.QueryString) &&
                    mappings.QueryString.ToLowerInvariant().IndexOf("where") == -1)
                {
                    mappings.QueryString = String.Format("WHERE {0}", mappings.QueryString);
                }

                string query = String.Format(Queries.Count, obj.TableNameFullyQualified, obj.TableName, mappings.QueryString);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
                        cinch.AddParameters(mappings.SqlParams);

                    result = cinch.ExecuteScalarInt();
                }
            }

            return result;
        }

        /// <summary>
        /// Find first by primary key
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="id">The primary key to be searched.</param>
        /// <returns>The object if found; null, otherwise.</returns>
        public static T FindFirst<T>(int id) where T : ModelBase, new()
        {
            var obj = new T();

            if (obj != null)
            {
                string query = String.Format(Queries.FindFirst, obj.ColumnsFullyQualified, obj.TableNameFullyQualified, obj.TableName, obj.PrimaryKeyFullyQualified);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    cinch.AddParameter("id", SqlDbType.Int, id);
                    obj = cinch.FillObject<T>();
                }
            }

            return obj;
        }

        /// <summary>
        /// Finds all as List based on where / param
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<T> Find<T>(string where = null, object[] param = null) where T : ModelBase, new()
        {
            var obj = new T();
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

                string query = String.Format(Queries.Find, obj.ColumnsFullyQualified, obj.TableNameFullyQualified, obj.TableName, mappings.QueryString);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
                        cinch.AddParameters(mappings.SqlParams);

                    result = cinch.FillList<T>();
                }
            }

            return result;
        }
        
        /// <summary>
        /// Inserts an object to the database.
        /// </summary>
        /// <param name="obj">The object to be inserted.</param>
        /// <param name="cols">The whitelist of columns to be inserted.</param>
        /// <returns>Record ID if successfull, -1 otherwise</returns>
        public static int Insert<T>(T obj, List<string> cols = null) where T : ModelBase, new()
        {
            int result = -1;

            if(obj != null)
            {
                CinchMapping mappings = Mapper.MapProperties(obj, cols);
                string query = String.Format(Queries.Insert, obj.TableNameFullyQualified, mappings.ColumnsString, mappings.InsertValuesQueryParamsString);

                using (Cinch cinch = new Cinch(null, CommandType.Text))
                {
                    
                    cinch.AddParameters(mappings.SqlParams);

                    try
                    {
                        result = cinch.ExecuteScalarInt();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// UPDATE
        /// </summary>
        /// <param name="obj">The object to be updated.</param>
        /// <param name="cols">The whitelist of columns to be inserted.</param>
        /// <returns>Record ID if successfull, -1 otherwise</returns>
        public static int Update<T>(T obj, List<string> cols = null) where T : ModelBase
        {
            int result = -1;

            if (obj != null)
            {
                CinchMapping mappings = Mapper.MapProperties(obj, cols);
                string query = String.Format(Queries.Update, obj.TableNameFullyQualified, mappings.UpdateValuesQueryParamsString, obj.PrimaryKeyFullyQualified);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    cinch.AddParameters(mappings.SqlParams);
                    cinch.AddParameter("id", SqlDbType.Int, obj.ID);

                    try
                    {
                        result = cinch.ExecuteScalarInt();
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Delete by primary key
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="id">The primary key to be searched.</param>
        /// <returns>Record ID if successfull, -1 otherwise</returns>
        public static int Delete<T>(int id) where T : ModelBase, new()
        {
            var obj = new T();
            int result = -1;

            if (obj != null)
            {
                string query = String.Format(Queries.DeleteByID, obj.TableNameFullyQualified, obj.PrimaryKeyFullyQualified);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    cinch.AddParameter("id", SqlDbType.Int, id);

                    try
                    {
                        result = cinch.ExecuteScalarInt();
                    }
                    catch
                    {
                        throw;
                    }   
                }
            }

            return result;
        }

        /// <summary>
        /// Delete by any critera
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns>Record ID if successfull, -1 otherwise</returns>
        public static int Delete<T>(string where = null, object[] param = null) where T : ModelBase, new()
        {
            T obj = new T();
            int result = 0;

            if (obj != null)
            {
                CinchMapping mappings = Mapper.MapQuery<T>(obj, where, param);

                //make sure we have a WHERE 
                if (!string.IsNullOrWhiteSpace(mappings.QueryString) &&
                    mappings.QueryString.ToLowerInvariant().IndexOf("where") == -1)
                {
                    mappings.QueryString = String.Format("WHERE {0}", mappings.QueryString);
                }

                string query = String.Format(Queries.Delete, obj.TableNameFullyQualified, mappings.QueryString);

                using (Cinch cinch = new Cinch(query, CommandType.Text))
                {
                    if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
                        cinch.AddParameters(mappings.SqlParams);

                    try
                    {
                        result = cinch.ExecuteScalarInt();
                    }
                    catch
                    {
                        throw;
                    }                    
                }
            }

            return result;
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


        /// <summary>
        /// Finds all as List based on where / param
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="where"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        //public static List<T> PagedFind<T>(
        //    T obj,
        //    string where = null,
        //    object[] param = null,
        //    string orderBy = "",
        //    int page = 1,
        //    int pageSize = 100) where T : ModelBase
        //{
        //    List<T> result = new List<T>();

        //    if (obj != null)
        //    {
        //        CinchMapping mappings = Mapper.MapQuery<T>(obj, where, param);

        //        //make sure we have a WHERE 
        //        if (!string.IsNullOrWhiteSpace(mappings.QueryString) &&
        //            mappings.QueryString.ToLowerInvariant().IndexOf("where") == -1)
        //        {
        //            mappings.QueryString = String.Format("WHERE {0}", mappings.QueryString);
        //        }

        //        using (DataConnect cinch = new DataConnect(null, CommandType.Text))
        //        {
        //            var lowerIndex = ((page - 1) * pageSize) + 1;
        //            var upperIndex = lowerIndex + pageSize;

        //            string query = String.Format(
        //                Queries.PagedFind,
        //                obj.ColumnsFullyQualified,
        //                obj.TableNameFullyQualified,
        //                obj.TableName,
        //                mappings.QueryString,
        //                orderBy,
        //                lowerIndex,
        //                upperIndex);

        //            cinch.SetQuery(query);

        //            if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
        //                cinch.AddParameters(mappings.SqlParams);

        //            result = cinch.FillList<T>();
        //        }
        //    }

        //    return result;
        //}

        //public static T ExecCustom<T>(string query, object[] param = null) where T : ModelBase, new()
        //{
        //    T obj = new T();
        //    CinchMapping mappings = Mapper.MapQuery<T>(obj, query, param);

        //    if(!String.IsNullOrWhiteSpace(mappings.QueryString))
        //    {
        //        using (DataConnect cinch = new DataConnect(mappings.QueryString, CommandType.Text))
        //        {
        //            if (mappings.SqlParams != null && mappings.SqlParams.Count > 0)
        //                cinch.AddParameters(mappings.SqlParams);

        //            return cinch.FillObject<T>();
        //        }
        //    }

        //    return null;
        //}
        
    }
}
