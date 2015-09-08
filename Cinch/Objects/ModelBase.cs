using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;

namespace CinchORM
{
    public abstract class ModelBase : IModelBase, IModelIdentity, IModelName
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
        /// SCHEMA
        /// </summary>
        protected virtual string schema { get; set; }
        public string Schema
        {
            get
            {
                if (String.IsNullOrWhiteSpace(schema))
                    return GetSchema();
                else
                    return schema;
            }
        }
        public string GetSchema()
        {
            try
            {
                using (DataConnect dc = new DataConnect(Queries.GetSchema, CommandType.Text))
                {
                    dc.SetQuery(Queries.GetSchema);
                    dc.AddParameter("table", SqlDbType.NVarChar, this.TableName);

                    string sch = String.Format("{0}", dc.ExecuteScalar());

                    if (String.IsNullOrWhiteSpace(sch))
                        throw new ApplicationException(String.Format("Invalid or null schema for {0}. Does the table exist?", this.objName), new NullReferenceException());

                    return sch;
                }
            }
            catch (SqlException ex)
            {
                throw new ApplicationException(String.Format("The database table {0} probably doesn't exist.", this.TableName), ex);
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
            using (DataConnect dc = new DataConnect(Queries.GetColumns, CommandType.Text))
            {
                dc.SetQuery(Queries.GetColumns);
                dc.AddParameter("table", SqlDbType.NVarChar, this.TableName);
                dc.AddParameter("schema", SqlDbType.NVarChar, this.Schema);

                string cols = String.Format("{0}", dc.ExecuteScalar());

                if (String.IsNullOrWhiteSpace(cols))
                    throw new ApplicationException(String.Format("Invalid or null column values for {0}", this.objName), new NullReferenceException());

                return cols;
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

                string objValue = null;

                try {
                    objValue = String.Format("{0}", this.GetType().GetProperty(this.PrimaryKey).GetValue(this, null));
                }
                catch (Exception) {
                    throw new ApplicationException(String.Format("Could not parse primary key for {0}", objName), new NullReferenceException());
                }
                
                if (!int.TryParse(objValue, out _ID))
                    throw new ApplicationException(String.Format("Could not parse primary key for {0}", objName), new NullReferenceException());

                return _ID;
            }
            set
            {
                _ID = value;
            }
        }
    }
}
