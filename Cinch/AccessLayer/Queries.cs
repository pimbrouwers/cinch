using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinchORM
{
    internal static class Queries
    {
        internal static string Count
        {
            get
            {
                return @"
                    SELECT COUNT(*) AS CountResult
                    FROM {0} as {1}
                    {2}
                ";
            }
        }

        internal static string GetSchema
        {
            get
            {
                return @"
                    SELECT DISTINCT TABLE_SCHEMA FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table;
                ";
            }
        }

        internal static string GetColumns
        {
            get
            {
                return @"
                    DECLARE @cols VARCHAR(8000);
                    SELECT @cols = COALESCE(@cols + ', ', '') + CONCAT(@table, '.', COLUMN_NAME) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @table AND TABLE_SCHEMA = @schema;

                    SELECT @cols
                ";
            }
        }

        internal static string Exists
        {
            get
            {
                return  @"
                    SELECT {3} 
                    FROM {0} as [{1}]
                    WHERE {2} = @value;
                ";

            }
        }

        internal static string FindFirst
        {
            get
            {
                return @"
                    SELECT TOP 1 {0}
                    FROM {1} as {2}
                    WHERE {3} = @id
                ";
            }
        }

        internal static string PagedFind
        {
            get
            {
                return @"
                    SELECT {0}
                    FROM ( 
                        SELECT ROW_NUMBER() OVER (ORDER BY {4}) AS RowNum, *
                        FROM {1}
                        {3}
                    ) AS {2}
                    WHERE   RowNum >= {5}
                    AND RowNum < {6}
                    ORDER BY RowNum";
            }
        }

        internal static string Find
        {
            get
            {
                return @"
                    SELECT {0}
                    FROM {1} as {2}
                    {3}
                ";
            }
        }

        internal static string LastInserted {
            get
            {
                return "SCOPE_IDENTITY()";
            }
        }   

        internal static string Insert
        {
            get
            {
                string insert = @"
                    INSERT INTO {0} ({1})
                    VALUES ({2});

                    SELECT {{scope_identity}};
                ";

                return insert.Replace("{{scope_identity}}", Queries.LastInserted);
            }
        }

        internal static string Update
        {
            get
            {
                string update = @"
                    UPDATE {0}
                    SET {1}
                    WHERE {2} = @id;

                    SELECT {{scope_identity}};
                ";

                return update.Replace("{{scope_identity}}", Queries.LastInserted);
            }
        }

    }
}
