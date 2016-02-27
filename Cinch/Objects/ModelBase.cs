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
    public abstract class ModelBase : IModelBase
    {
        protected virtual string primaryKey { get; set; }
        private string _primaryKey;        
        public string PrimaryKey
        {
            get
            {
                if(String.IsNullOrWhiteSpace(_primaryKey))
                {
                    if (String.IsNullOrWhiteSpace(primaryKey))
                    {
                        _primaryKey = String.Format("{0}_id", TableName.ToLowerInvariant());
                    }
                    else
                    {
                        _primaryKey = primaryKey;
                    }
                }

                return _primaryKey;
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
        private string _tableName;        
        public string TableName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(tableName))
                {
                    _tableName = this.ObjType.Name;
                }
                else
                {
                    _tableName = tableName;
                }
                
                return _tableName;
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
        private string _schema;        
        public string Schema
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_schema))
                {
                    if(String.IsNullOrWhiteSpace(schema))
                    {
                        try
                        {
                            using (DataConnect dc = new DataConnect(Queries.GetSchema, CommandType.Text))
                            {
                                dc.SetQuery(Queries.GetSchema);
                                dc.AddParameter("table", SqlDbType.NVarChar, this.TableName);

                                _schema = String.Format("{0}", dc.ExecuteScalar());

                                if (String.IsNullOrWhiteSpace(_schema))
                                    throw new ApplicationException(String.Format("Invalid or null schema for {0}. Does the table exist?", this.objName), new NullReferenceException());

                            }
                        }
                        catch (SqlException ex)
                        {
                            throw new ApplicationException(String.Format("The database table {0} probably doesn't exist.", this.TableName), ex);
                        }
                    }
                    else
                    {
                        _schema = schema;
                    }
                }

                
                return _schema;
            }
            set
            {
                _schema = value;
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
        private Type _objType;        
        public Type ObjType {
            get
            {
                if(_objType == null)
                {
                    Type type = this.GetType();
                    _objType = type;
                }

                return _objType;
            }
        }

        private Dictionary<string, PropertyInfo> _properties;
        public Dictionary<string, PropertyInfo> Properties
        {
            get
            {
                if(_properties == null)
                {
                    _properties = ObjType.GetProperties().ToDictionary(p => p.Name, p => p);
                }

                return _properties;
            }
        }

        private string _objName;        
        public string objName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_objName))
                {
                    _objName = ObjType.Name;
                }

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
                    objValue = String.Format("{0}", this.Properties[this.PrimaryKey].GetValue(this, null));
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
