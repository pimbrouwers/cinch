using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CinchORM
{
    public interface IModelBase
    {
        string PrimaryKey { get; }
        string PrimaryKeyFullyQualified { get; }

        string TableName { get; }
        string TableNameFullyQualified { get; }

        string Schema { get; }
        string GetSchema();

        string DisplayField { get; }

        string ColumnsFullyQualified { get; }
        string GetColumns();

        //bool Exists();
        //bool Exists(PropertyInfo column, object value);

        //T FindFirst<T>() where T : ModelBase;
        //T FindFirst<T>(int ID) where T : ModelBase;
        //List<T> Find<T>(string where, string[] param) where T : ModelBase;

        //int Insert(List<string> cols = null);
        //int Update(List<string> cols = null);
        

        //List<object> Find();
        //List<object> FindAll(int skip, int take);
        
        
    }
}
