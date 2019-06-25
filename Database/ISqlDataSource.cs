using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public interface ISqlDataSource
	{

		// Non-Transactional Accessors which mamke use of internal/dynamically allocated connection

		int DefaultTimeout { get; set; }
		DBResult Query(QueryString sql, OdbcParameterList parms, out DatabaseDataReader reader, int timeout = 30);
		DBResult Query(QueryString sql, out DatabaseDataReader reader, int timeout = 30);
		DBResult QueryScalar(QueryString sql, out Object objRet);
		DBResult QueryDataSets(QueryString sql, params ILoadable[] args);

		T QueryScalar<T>(String sql, params object[] args) where T : new();
		T  QueryObject<T>(String sql, params object[] args) where T:  new();
		List<T>  QueryList<T>(String sql, params object[] args) where T:  new();
		

		DBResult Insert(QueryString sql, OdbcParameterList parameterDictionary);
		DBResult Insert(QueryString sql);

		DBResult Update(QueryString sql);
		DBResult Update(QueryString sql, OdbcParameterList parameterDictionary);
		DBResult Delete(QueryString sql);

		DBResult GetConnection(out DatabaseConnection connection);
		DBResult ReleaseConnection(DatabaseConnection connection);

		ISqlConnectionPool ConnectionPool
		{
			get;
			set;
		}

		// SQL utility methods
		DBResult Query(DatabaseConnection conn, QueryString sql, out DatabaseDataReader reader, int timeout = 30);
		DBResult Query(DatabaseConnection conn, QueryString sql, OdbcParameterList parms, out DatabaseDataReader reader, int timeout = 30);
		DBResult QueryScalar(DatabaseConnection conn, QueryString sql, out Object result);
		DBResult Insert(DatabaseConnection conn, QueryString sql, OdbcParameterList parameterDictionary);
		DBResult Update(DatabaseConnection conn, QueryString sql, OdbcParameterList parms);

		DBResult Execute(DatabaseConnection conn, QueryString sql, DatabaseParameterList parms);
		DBResult Execute(QueryString sql, DatabaseParameterList parms);

		//Accessors for transactional statements
		DBResult BeginTransaction(DatabaseConnection conn);
		DBResult Commit(DatabaseConnection conn);
		DBResult Rollback(DatabaseConnection conn);

		// Mysql utility methods
		UInt64 GetLastInsertID(DatabaseConnection conn);

		DBResult TestConnection();

		// String Formatting
		QueryString Format(String format, params object[] args);
		QueryString FormatUntrusted(String format, params object[] args);
		QueryString FormatTrusted(String format, params object[] args);
//		String Escape(String unescaped);
		QuotedString Quoted(Object value);

		bool TrustedSql { get; set; }
		DBResult LastResult { get; }
		bool ReadOnly { get; set; }
	}

}
