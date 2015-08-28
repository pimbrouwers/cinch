using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinchORM.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Simple function to return a detailed string from an exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string DetailedExceptionString(this Exception ex)
        {
            return String.Format("Exception: {0}{1}Stack Trace:{2}{3}", ex.Message.ToString(), Environment.NewLine, Environment.NewLine, ex.StackTrace);
        }
    }
}
