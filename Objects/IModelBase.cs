using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    public interface IModelBase
    {
        string PrimaryKey { get; }
        string TableName { get; }
        string DisplayField { get; }

        bool Exists();
        bool Insert(List<string> cols = null);
        //void Update();
        T FindFirst<T>();
        T FindFirst<T>(int ID);
        
        //List<object> FindList();
        //List<object> FindAll(int skip, int take);
        string GetSchema();
        string GetColumns();
        
    }
}
