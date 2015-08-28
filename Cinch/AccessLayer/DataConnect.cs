using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace CinchORM
{
    /// <summary>
    /// This class encapsulates the components necessary to perform a SQL query
    /// </summary>
    public class DataConnect : IDisposable
    {

        private DataSet ds;
        private DataTable dt;
        private SqlDataReader dr;
        private SqlDataAdapter da;
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlTransaction Trans;

        /// <summary>
        /// Parameter collection of the underlying sql command object
        /// </summary>
        public IDataParameterCollection Parameters
        {
            get
            {
                if (cmd != null)
                {
                    return cmd.Parameters;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the command timeout for the active connection.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return cmd.CommandTimeout;
            }
            set
            {
                cmd.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Access the parameters of the underlying command object
        /// </summary>
        public IDataParameter this[string id]
        {
            get
            {
                return (IDataParameter)Parameters[id]; ;
            }
            set
            {
                Parameters[id] = value;
            }
        }


        public SqlConnection Connection
        {
            get
            {
                return conn;
            }
        }

        private string _connectionString;
        public string ConnectionString
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_connectionString))
                    return _connectionString;

                string appConnStr = ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString;

                if (!String.IsNullOrWhiteSpace(appConnStr))
                    _connectionString = appConnStr;
                else
                    throw new ApplicationException("Connection string with key \"DataContext\" is not configured");

                return _connectionString;
            }
        }

        /// <summary>
        /// Used for Stored Procedures
        /// </summary>
        /// <param name="query">String to hold the query</param>
        /// <param name="connStr">Connection string value from within web.config.</param>
        /// <param name="ct">CommandType of the query (usually CommandType.StoredProcedure)</param>
        public DataConnect(string query, CommandType ct)
        {
            SetConnectionString(ConnectionString);
            cmd = new SqlCommand(query, conn);
            cmd.CommandType = ct;
        }

        /// <summary>
        /// Sets up the connection string to the one entered
        /// </summary>	
        /// <param name="connStr">The connection string</param>
        private void SetConnectionString(string connStr)
        {
            conn = new SqlConnection(connStr);
        }

        /// <summary>
        /// Destructor for this object.
        /// </summary>
        public void Dispose()
        {
            if (conn != null) conn.Dispose();
        }

        /// <summary>
        /// Returns a data set with the data returned from the query.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>DataSet containing results from query.</returns>
        public DataSet Fill(string tableName)
        {
            try
            {
                // set up the dataset
                ds = new DataSet();

                // set up the adapter 
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                //verbose output
                VerboseOutput();

                // run the fill query
                da.Fill(ds, tableName);
                return ds;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Same as above but doesn't take a datatable name
        /// Use this if there is more than one data table come back from query
        /// </summary>
        /// <returns>DataSet with results from query</returns>
        public DataSet Fill()
        {
            try
            {
                // set up the dataset
                ds = new DataSet();

                // set up the adapter 
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                //verbose output
                VerboseOutput();

                // run the fill query
                da.Fill(ds);
                return ds;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Fill a dataset with only the schema information from a query
        /// </summary>
        /// <returns>DataSet with Schema</returns>
        public DataSet FillSchemaOnly()
        {
            try
            {
                // set up the dataset
                ds = new DataSet();

                // set up the adapter 
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                //verbose output
                VerboseOutput();

                // run the fill query
                da.FillSchema(ds, SchemaType.Source);
                return ds;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Returns a data table with the data returned from the query.
        /// </summary>
        /// <returns>DataTable containing results from query.</returns>
        public DataTable FillTable()
        {

            return FillTable("");
        }

        /// <summary>
        /// Returns a list of objects of the given Type, with propeties set based on how they match up to the fields returned in the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> FillList<T>()
        {
            try
            {

                SqlDataReader rd = FillSqlReader();
                List<T> lst = new List<T>();
                while (rd.Read())
                {
                    lst.Add(ConvertReaderToObject<T>(rd));
                }
                rd.Close();

                return lst;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                Close();
            }

        }

        /// <summary>
        /// Populates a single object of the given Type, with propeties set based on how they match up to the fields returned in the first row of the recordset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FillObject<T>()
        {
            try
            {

                SqlDataReader rd = FillSqlReader();
                T t = default(T);
                if (rd.Read())
                {
                    t = ConvertReaderToObject<T>(rd); ;
                }
                rd.Close();

                return t;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Close();
            }

        }

        private T ConvertReaderToObject<T>(SqlDataReader rd)
        {

            Type type = typeof(T);

            if (type.IsPrimitive || type == (typeof(string)))
            {
                //if this is a primitive type, we can't set a property, so simply attemp to use the first value in the row
                return (T)rd[0];

            }


            PropertyInfo[] props = type.GetProperties();
            ConstructorInfo constr = type.GetConstructor(System.Type.EmptyTypes);

            T t = (T)constr.Invoke(new object[0]);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite) continue;

                for (int i = 0; i < rd.FieldCount; i++)
                {
                    string fieldName = rd.GetName(i);
                    if (string.Compare(fieldName, prop.Name, true) == 0)
                    {
                        if (!rd.IsDBNull(i))
                        {

                            prop.SetValue(t, rd.GetValue(i), null);
                        }
                    }

                }
            }
            return t;
        }

        /// <summary>
        /// Convert a DataTable to a generic list of objects Type T based on the Reflected properties of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ConvertDataTableToList<T>(DataTable dt)
        {

            List<T> lst = new List<T>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    lst.Add(ConvertDataRowToObject<T>(row));
                }
            }

            return lst;

        }

        /// <summary>
        /// Converts a DataRow to an object of Type T based on the Reflected properties of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T ConvertDataRowToObject<T>(DataRow row)
        {

            if (row.Table == null)
            {
                throw new ArgumentException("Row must belong to a parent table.", "row");
            }
            Type type = typeof(T);
            PropertyInfo[] props = type.GetProperties();
            ConstructorInfo constr = type.GetConstructor(System.Type.EmptyTypes);
            if (constr == null)
            {
                throw new ArgumentException("The provided type does not have a default constructor.");
            }

            T t = (T)constr.Invoke(new object[0]);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite) continue;

                if (row.Table.Columns.Contains(prop.Name) && !row.IsNull(prop.Name))
                {
                    prop.SetValue(t, row[prop.Name], null);
                }

            }
            return t;
        }

        /// <summary>
        /// Returns a named data table with the data returned from the query.
        /// </summary>		
        /// <param name="tableName"></param>
        /// <returns>DataTable containing results from query.</returns>
        public DataTable FillTable(string tableName)
        {
            try
            {
                // set up the dataset
                dt = new DataTable(tableName);

                // set up the adapter 
                da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                //verbose output
                VerboseOutput();

                // run the fill query
                da.Fill(dt);
                return dt;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Fills a dataReader.  Use this for max performance
        /// </summary>
        /// <returns>Returns a SqlDataReader</returns>
        public SqlDataReader FillSqlReader()
        {
            try
            {
                //verbose output
                VerboseOutput();

                this.Open();

                dr = cmd.ExecuteReader();
                return dr;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// Fills a dataReader.  Use this for max performance
        /// </summary>
        /// <returns>Returns an IDataReader</returns>
        public IDataReader FillIDataReader()
        {
            try
            {
                //verbose output
                VerboseOutput();

                this.Open();
                dr = cmd.ExecuteReader();
                return dr;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Close the the Reader Connection.
        /// </summary>
        /// <remarks>
        /// The reader must always be closed after use.
        /// </remarks>
        public void CloseReader()
        {
            try
            {
                //MODIFIED: Mike 04/04/2005: removed the Close() call that closed the whole connection
                //This made it impossible to use the reader in a transaction
                dr.Close();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Sets the text based query for the DataConnect object
        /// </summary>
        /// <param name="query">A string representing the query to be executed</param>
        public void SetQuery(string query)
        {
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
        }

        /// <summary>
        /// Sets up a stored procedure to be executed
        /// </summary>
        /// <param name="sproc">The stored procedure to be run</param>
        public void SetStoredProcedure(string sproc)
        {
            cmd.CommandText = sproc;
            cmd.CommandType = CommandType.StoredProcedure;
        }
        /// <summary>
        /// Sets up the CommandType after the class has already been created
        /// </summary>
        /// <param name="ct">CommandType being set to</param>
        public void SetCommandType(CommandType ct)
        {
            cmd.CommandType = ct;
        }
        
        public void PrepareQuery()
        {
            cmd.Prepare();
        }

        /// <summary>
        /// Runs the query.  Usually used for udpate commands
        /// </summary>
        public void ExecuteNonQuery()
        {

            try
            {

                //verbose output
                VerboseOutput();

                Open();
                // run the query
                cmd.ExecuteNonQuery();

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (cmd.Transaction == null)
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// Execute the command and return an object
        /// </summary>
        /// <returns>An Object</returns>
        public Object ExecuteScalar()
        {
            Object rv = new Object();
            try
            {
                //verbose output
                VerboseOutput();

                Open();

                // run the query and return the value
                rv = cmd.ExecuteScalar();

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch
            {

                throw;
            }
            finally
            {
                if (cmd.Transaction == null)
                {
                    Close();
                }
            }
            return rv;
        }

        /// <summary>
        /// Execute the command and return an object
        /// </summary>
        /// <returns>An Object</returns>
        public int ExecuteScalarInt()
        {
            Object rv = new Object();
            try
            {
                //verbose output
                VerboseOutput();

                Open();

                // run the query and return the value
                rv = cmd.ExecuteScalar();

            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    DBException dbx = new DBException(sqlEx.Message, sqlEx);
                    throw dbx;
                }
                else
                {
                    throw sqlEx;
                }
            }
            catch
            {

                throw;
            }
            finally
            {
                if (cmd.Transaction == null)
                {
                    Close();
                }
            }

            int result;
            if (int.TryParse(String.Format("{0}", rv), out result))
            {
                return result;
            }
            else
                return -1;

        }

        /// <summary>
        /// Adds a parameter to the command
        /// </summary>
        /// <param name="id">ID of the parameter to be created</param>
        /// <param name="type">SqlDbType of the parameter</param>
        public void AddParameter(string id, SqlDbType type)
        {
            cmd.Parameters.Add(id, type);
        }


        /// <summary>
        /// Adds an input/output parameter to the command
        /// This parameter can accept and return values
        /// </summary>
        /// <param name="id">ID of the parameter to be created</param>
        /// <param name="type">SqlDbType of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        public void AddOutputParameter(string id, SqlDbType type, object value)
        {
            SqlParameter param = cmd.Parameters.Add(id, type);
            param.Direction = ParameterDirection.InputOutput;
            param.Value = value;

        }

        /// <summary>
        /// Adds an output parameter to the command
        /// This parameter can only return values
        /// </summary>
        /// <param name="id">ID of the parameter to be created</param>
        /// <param name="type">SqlDbType of the parameter</param>		
        public void AddOutputParameter(string id, SqlDbType type)
        {
            SqlParameter param = cmd.Parameters.Add(id, type);
            param.Direction = ParameterDirection.InputOutput;

        }


        /// <summary>
        /// Returns the value of an output parameter.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetOutputParameterValue(string id)
        {
            if (cmd.Parameters.Contains(id))
            {
                return cmd.Parameters[id].Value;
            }
            return null;
        }

        /// <summary>
        /// Returns the value of an output parameter in the desired type.  If the output is not in the desired type, of it is null, the default value for that Type will be returned (ie: null for reference types, 0 for int, etc).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetOutputParameterValue<T>(string id)
        {
            object o = GetOutputParameterValue(id);
            if (o is T) return (T)o;
            return default(T);
        }

        /// <summary>
        /// Sets up a parameter for the query
        /// </summary>
        /// <param name="id">The ID of the parameter</param>
        /// <param name="type">The Sql type of the parameter</param>
        /// <param name="Value">The value of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        public void AddParameter(string id, SqlDbType type, int size, Object Value)
        {
            // add the parameter to the command
            cmd.Parameters.Add(id, type, size);

            // set the value of the parameter
            cmd.Parameters[id].Value = Value;
        }
        
        /// <summary>
        /// Sets up a parameter for the query
        /// </summary>
        /// <param name="id">The ID of the parameter</param>
        /// <param name="type">The Sql type of the parameter</param>
        /// <param name="Value">The value of the parameter</param>
        public void AddParameter(string id, SqlDbType type, Object Value)
        {
            // add the parameter to the command
            cmd.Parameters.Add(id, type);

            if (Value == null)
            {
                cmd.Parameters[id].Value = Convert.DBNull;
            }
            else if (Value.ToString() == "" && type != SqlDbType.VarChar)
            {
                // must convert the empty string to a DBNull
                cmd.Parameters[id].Value = Convert.DBNull;
            }
            else if (Value.ToString() == "" && (type == SqlDbType.Float || type == SqlDbType.Int || type == SqlDbType.Money))
            {
                cmd.Parameters[id].Value = 0;
            }
            else
            {
                // set the value of the parameter
                cmd.Parameters[id].Value = Value;
            }
        }

        /// <summary>
        /// Add collection of Sql Paramters
        /// </summary>
        /// <param name="sqlParams"></param>
        public void AddParameters(List<SqlParameter> sqlParams)
        {
            if(sqlParams != null && sqlParams.Count > 0)
                cmd.Parameters.AddRange(sqlParams.ToArray());
        }

        /// <summary>
        /// Set the value of a given parameter.
        /// </summary>
        /// <param name="id">The parameter name</param>
        /// <param name="val">The parameter value.</param>
        public void SetParameter(string id, Object val)
        {
            if ((val == null || val.ToString() == "") && (cmd.Parameters[id].SqlDbType == SqlDbType.Float || cmd.Parameters[id].SqlDbType == SqlDbType.Int || cmd.Parameters[id].SqlDbType == SqlDbType.Money))
                cmd.Parameters[id].Value = 0;
            else
                cmd.Parameters[id].Value = val;
        }
        
        /// <summary>
        /// Used to clear out all of the parameters
        /// </summary>
        public void ClearParameters()
        {
            cmd.Parameters.Clear();
        }

        /// <summary>
        /// Opens the database connection.
        /// </summary>
        public void Open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        /// <summary>
        /// Closes the database connection.
        /// </summary>
        public void Close()
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Closes the database connection with option commit parameter.
        /// </summary>
        public void Close(bool commitChanges)
        {
            if (conn != null)
            {
                if (commitChanges)
                {
                    CommitTransaction();
                }
                conn.Close();
            }
        }

        /// <summary>
        /// This starts a transaction, can manually rollback or commit and on close with automatically commit.
        /// </summary>
        public void BeginTransaction()
        {
            Open();
            Trans = conn.BeginTransaction();
            cmd.Transaction = Trans;
        }

        /// <summary>
        /// This commits the transaction if the object exists.
        /// </summary>
        public void CommitTransaction()
        {
            if (cmd.Transaction != null)
            {
                cmd.Transaction.Commit();
            }
        }

        /// <summary>
        /// This allows the user to rollback all the way to the beginning.
        /// </summary>
        public void RollbackTransaction()
        {
            RollbackTransaction("");
        }

        /// <summary>
        /// This allows the user to rollback to the last save point.
        /// </summary>
        public void RollbackTransaction(string rollBackName)
        {
            if (cmd.Transaction != null)
            {
                if (rollBackName != "")
                {
                    cmd.Transaction.Rollback(rollBackName);
                }
                else
                {
                    cmd.Transaction.Rollback();
                }
            }
        }

        /// <summary>
        /// This allows the user to save a specific rollback marker.
        /// </summary>
        public void SaveTransaction(string savePointName)
        {
            if (cmd.Transaction != null)
            {
                cmd.Transaction.Save(savePointName);
            }
        }

        /// <summary>
        /// Trace the values of this sql object
        /// </summary>		
        public void VerboseOutput()
        {

            //only proceed if we have a command object
            if (cmd == null) return;

            //only write this out in verbose mode
            //do some logging
        }

        private string GetTraceOutputs()
        {
            StringBuilder output = new StringBuilder();
            output.Append(cmd.CommandType).Append(": ");
            output.Append(cmd.CommandText).Append("\r\n");

            //loop all prameters and output the name - value (dbtype)
            foreach (SqlParameter param in cmd.Parameters)
            {
                output.Append(param.ParameterName).Append(" = ").Append(param.Value).Append(" (").Append(param.DbType).Append(")\r\n");
            }
            return output.ToString();
        }


    }
}
