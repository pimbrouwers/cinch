using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;

namespace Cinch
{
    public abstract class ModelBase : IModelBase
    {
        protected virtual string primaryKey { get; set; }
        public string PrimaryKey
        {
            get
            {
                if (String.IsNullOrWhiteSpace(primaryKey))
                {
                    string singular = objName.Singularize();
                    string titleCase = (String.IsNullOrWhiteSpace(singular)) ? objName.Titleize() : singular.Titleize();
                    primaryKey = String.Format("{0}ID", (String.IsNullOrWhiteSpace(titleCase) ? objName : titleCase));
                }

                return primaryKey;
            }
        }
        public string PrimaryKeyFullyQualified
        {
            get
            {
                return String.Format("[{0}].[{1}]", this.TableName, this.PrimaryKey);
            }
        }

        /// <summary>
        /// TABLE NAME
        /// </summary>
        protected virtual string tableName { get; set; }
        public string TableName
        {
            get
            {


                if (String.IsNullOrWhiteSpace(tableName))
                {
                    string plural = objName.Pluralize();
                    string titleCase = (String.IsNullOrWhiteSpace(plural)) ? objName.Titleize() : plural.Titleize();
                    return (String.IsNullOrWhiteSpace(titleCase) ? objName : titleCase);
                }

                return tableName;
            }
        }
        public string TableNameFullyQualified
        {
            get
            {
                return String.Format("[{0}].[{1}]", this.Schema, this.TableName);
            }
        }

        /// <summary>
        /// DISPLAY FIELD (for listing query)
        /// </summary>
        protected virtual string displayField { get; set; }
        public string DisplayField
        {
            get
            {
                if (String.IsNullOrWhiteSpace(displayField))
                {
                    throw new ApplicationException(String.Format("You must specify a 'DisplayField' for {0}", objName), new NullReferenceException());
                }

                return displayField;
            }
        }

        /// <summary>
        /// COLUMNS
        /// </summary>
        protected virtual string columns { get; set; }
        private List<string> columnList 
        { 
            get 
            {
                if (String.IsNullOrWhiteSpace(columns))
                    columns = GetColumns();

                return columns.StringToStringList(',').Select(c => c.Substring(c.IndexOf('.') + 1)).ToList(); 
            } 
        }
        public string ColumnsFullyQualified
        {
            get
            {
                if (String.IsNullOrWhiteSpace(columns))
                    columns = GetColumns();

                return columns;
            }
        }
        public string GetColumns()
        {
            string query = @"
                DECLARE @cols VARCHAR(8000);
                SELECT @cols = COALESCE(@cols + ', ', '') + CONCAT(@table, '.', COLUMN_NAME) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @table AND TABLE_SCHEMA = @schema;

                SELECT @cols
            ";

            using (DataConnect dc = new DataConnect(query, CommandType.Text))
            {
                dc.SetQuery(query);
                dc.AddParameter("table", SqlDbType.NVarChar, this.TableName);
                dc.AddParameter("schema", SqlDbType.NVarChar, this.Schema);

                string cols = String.Format("{0}", dc.ExecuteScalar());

                if (String.IsNullOrWhiteSpace(cols))
                    throw new ApplicationException(String.Format("Invalid or null column values for {0}", this.objName), new NullReferenceException());

                return cols;
            }
        }

        /// <summary>
        /// SCHEMA
        /// </summary>
        private string schema;
        public string Schema
        {
            get
            {
                if (String.IsNullOrWhiteSpace(schema))
                    schema = GetSchema();

                return schema;
            }
        }
        public string GetSchema()
        {
            string query = @"
                SELECT DISTINCT TABLE_SCHEMA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table;
            ";

            using (DataConnect dc = new DataConnect(query, CommandType.Text))
            {
                dc.SetQuery(query);
                dc.AddParameter("table", SqlDbType.NVarChar, this.TableName);

                string sch = String.Format("{0}", dc.ExecuteScalar());

                if (String.IsNullOrWhiteSpace(sch))
                    throw new ApplicationException(String.Format("Invalid or null schema for {0}", this.objName), new NullReferenceException());

                return sch;
            }
        }

        /// <summary>
        /// EXISTS - primary key
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            string query = String.Format(Queries.Exists, this.TableNameFullyQualified, this.TableName, this.PrimaryKeyFullyQualified);

            if (this.ID > 0)
            {
                using (DataConnect dc = new DataConnect(query, CommandType.Text))
                {
                    dc.SetQuery(query);
                    dc.AddParameter("id", SqlDbType.Int, this.ID);

                    int count = dc.ExecuteScalarInt();

                    if (count > 0)
                        return true;
                    else
                        return false;
                }
            }
            else
                throw new ApplicationException(String.Format("Could not check Existance for {0} because the objects primary key value is not set", objName), new NullReferenceException());
        }

        /// <summary>
        /// EXISTS - any column value
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Exists(PropertyInfo column, string value)
        {
            string query = String.Format(Queries.Exists, this.TableNameFullyQualified, this.TableName, String.Format("[{0}].[{1}]", this.TableName, column.Name));
            if (!String.IsNullOrWhiteSpace(value))
            {
                using (DataConnect dc = new DataConnect(query, CommandType.Text))
                {
                    dc.SetQuery(query);
                    dc.AddParameter("value", SqlDbType.NVarChar, value);

                    int count = dc.ExecuteScalarInt();

                    if (count > 0)
                        return true;
                    else
                        return false;
                }
            }
            else
                throw new ApplicationException(String.Format("Could not check Existance for {0} because the column value is not set", objName), new NullReferenceException());
        }

        /// <summary>
        /// INSERT
        /// </summary>
        /// <returns></returns>
        public bool Insert(List<string> cols = null)
        {
            
            using (DataConnect dc = new DataConnect(null, CommandType.Text))
            {
                CinchMapping mappings = Mapper.MapProperties(this, cols);
                string query = String.Format(Queries.Insert, this.TableNameFullyQualified, mappings.Columns, mappings.ValuesQueryParams);
                dc.SetQuery(query);
                dc.AddParameters(mappings.SqlParams);

                try
                {
                    return dc.ExecuteScalar() == null;
                }
                catch(Exception ex)
                {
                    //do something useful here
                    throw ex;
                }
            }

        }

        /// <summary>
        /// Find first by primary key of existing object
        /// </summary>
        /// <returns></returns>
        public T FindFirst<T>()
        {
            string query = @"
                SELECT {0}
                FROM {1} as {2}
                WHERE {3} = @id
            ";

            query = String.Format(query, this.ColumnsFullyQualified, this.TableNameFullyQualified, this.TableName, this.PrimaryKeyFullyQualified);

            if (this.ID > 0)
            {
                using (DataConnect dc = new DataConnect(query, CommandType.Text))
                {
                    dc.SetQuery(query);
                    dc.AddParameter("id", SqlDbType.Int, this.ID);

                    return dc.FillObject<T>();
                }
            }
            else
                throw new ApplicationException(String.Format("Could not FindFirst for {0} because the objects primary key value is not set", objName), new NullReferenceException());

        }

        /// <summary>
        /// Find first based on value
        /// </summary>
        /// <returns></returns>
        public T FindFirst<T>(int ID)
        {
            string query = @"
                SELECT {0}
                FROM {1} as {2}
                WHERE {3} = @id
            ";

            query = String.Format(query, this.ColumnsFullyQualified, this.TableNameFullyQualified, this.TableName, this.PrimaryKeyFullyQualified);

            using (DataConnect dc = new DataConnect(query, CommandType.Text))
            {
                dc.SetQuery(query);
                dc.AddParameter("id", SqlDbType.Int, ID);

                return dc.FillObject<T>();
            }
        }

        /// <summary>
        /// Get Class Name
        /// </summary>
        private string _objName;
        public string objName
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_objName))
                    return _objName;

                _objName = this.GetType().Name;

                return _objName;
            }
        }

        /// <summary>
        /// ID
        /// </summary>
        private int _ID;
        public int ID
        {
            get
            {
                if (_ID == -1 || _ID > 0)
                    return _ID;

                _ID = -1;
                string objValue = String.Format("{0}", this.GetType().GetProperty(this.PrimaryKey).GetValue(this, null));

                if (!int.TryParse(objValue, out _ID))
                    throw new ApplicationException(String.Format("Could not parse primary key for {0}", objName), new NullReferenceException());

                return _ID;
            }
        }

    }
}
