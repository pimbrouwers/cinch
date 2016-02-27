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
        
        string ColumnsFullyQualified { get; }
        string GetColumns();
        
    }
}
