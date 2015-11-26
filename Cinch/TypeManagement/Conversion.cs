using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinchORM
{
    public static class Conversion
    {
        //public object ResolvePropertyValue(Type type, object value)
        //{
        //    TypeCode typeCode = Type.GetTypeCode(type);
        //    string strVal = String.Format("{0}", value);
        //    switch (typeCode)
        //    {
        //        case TypeCode.Int64:
        //            Int64 int64Test;
        //            if (!Int64.TryParse(strVal, out int64Test))
        //                throw new ApplicationException("could not resolve value type");
        //            else
        //                return int64Test;

        //        case TypeCode.Boolean:
        //            Int64 boolTest;
        //            if (!Int64.TryParse(strVal, out boolTest))
        //                throw new ApplicationException("could not resolve value type");
        //            else
        //                return boolTest;

        //        case TypeCode.String:
        //            return strVal;

        //        case TypeCode.DateTime:
        //            return SqlDbType.DateTime;

        //        case TypeCode.Double:
        //            return SqlDbType.Float;

        //        case TypeCode.Decimal:
        //            return SqlDbType.Decimal;

        //        case TypeCode.Int16:
        //        case TypeCode.Int32:
        //            return SqlDbType.Int;

        //        case TypeCode.Byte:
        //            return SqlDbType.TinyInt;

        //        default:
        //            throw new ArgumentOutOfRangeException("clr type");
        //    }
        //}

        public static SqlDbType GetSqlDbType(Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int64:
                    return SqlDbType.BigInt;

                case TypeCode.Boolean:
                    return SqlDbType.Bit;
                
                case TypeCode.Char:
                    return SqlDbType.NChar;

                case TypeCode.String:
                    return SqlDbType.NVarChar;

                case TypeCode.DateTime:
                    return SqlDbType.DateTime;

                case TypeCode.Double:
                    return SqlDbType.Float;

                case TypeCode.Decimal:
                    return SqlDbType.Decimal;

                case TypeCode.Int16:
                case TypeCode.Int32:
                    return SqlDbType.Int;
                                   
                case TypeCode.Byte:
                    return SqlDbType.TinyInt;

                default:
                    throw new ArgumentOutOfRangeException("clr type");
            }
        }

        public static Type GetClrType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return typeof(long?);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool?);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                    return typeof(char);
                    
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return typeof(DateTime?);

                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal?);

                case SqlDbType.Float:
                    return typeof(double?);

                case SqlDbType.Int:
                    return typeof(int?);

                case SqlDbType.Real:
                    return typeof(float?);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid?);

                case SqlDbType.SmallInt:
                    return typeof(short?);

                case SqlDbType.TinyInt:
                    return typeof(byte?);

                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);

                case SqlDbType.Structured:
                    return typeof(DataTable);

                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset?);

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }
    }
}
