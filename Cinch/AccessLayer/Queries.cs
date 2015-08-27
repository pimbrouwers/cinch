using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinch
{
    public static class Queries
    {

        public static string Insert
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
