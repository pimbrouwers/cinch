using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinchORM.AccessLayer
{
    internal static class SpecialCharacters
    {
        internal static string ColumnBegin
        {
            get
            {
                return "[";
            }
        }

        internal static string ColumnEnd
        {
            get
            {
                return "]";
            }
        }

        internal static string ParamPrefix
        {
            get
            {
                return "@";
            }
        }
    }
}
