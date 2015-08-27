using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    internal static class Queries
    {

        internal static string Exists
        {
            get
            {
                return @"
                    SELECT COUNT(*) 
                    FROM {0} as [{1}]
                    WHERE {2} = @value
                ";
            }
        }

        internal static string Insert
        {
            get
            {
                return @"
                    INSERT INTO {0} ({1})
                    VALUES ({2})

                    SELECT SCOPE_IDENTITY()
                ";
            }
        }
    }
}
