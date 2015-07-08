using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    public static class ReflectionExtensions
    {
        public static T GetAttributeFrom<T>(this object instance, PropertyInfo property) where T : Attribute
        {
            var attrType = typeof(T);        
            return (T)property.GetCustomAttributes(attrType, false).FirstOrDefault();
        }
    }
}
