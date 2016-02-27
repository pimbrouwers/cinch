using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CinchORM
{
    internal static class CinchCache
    {
        internal static Dictionary<string, PropertyInfo[]> _objectPropertyCache = new Dictionary<string, PropertyInfo[]>();
        internal static Dictionary<string, ConstructorInfo> _objectConstructorCache = new Dictionary<string, ConstructorInfo>();
    }
}
