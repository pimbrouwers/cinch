using System;
using System.Data.SqlClient;

namespace CinchORM {
	/// <summary>
	/// an exception class that is used to send error 5000 (user errors) from database calls to the user
	/// this lets us do validation in the database that can be shown to the user without the stack trace
	/// </summary>
	public class DBException : ApplicationException {

		/// <summary>
		/// Create an instance of the DBException class with the specified message.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="innerException"></param>
		public DBException(string msg, SqlException innerException) : base(msg, innerException) {		
		}
	}
}
