#define INCLUDE_PERFORMANCE_LOGGING

using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;
using System.Threading;
using System;

using KanoopCommon.Conversions;
using KanoopCommon.Logging;
using KanoopCommon.Threading;
using KanoopCommon.Types;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace KanoopCommon.Database
{
	public abstract class SqlDataSource : ISqlDataSource
	{
		#region Constants

		public const String MYSQL_5_1_DRIVER = "MySQL ODBC 5.1 Driver";
		public const String MYSQL_NATIVE_DRIVER = "MySQL Native Driver";
		public const String SQL_EXPRESS_DRIVER = "SQLExpress";
		public const String SQLITE3_ODBC_DRIVER = "SQLite3 ODBC Driver";
		public const String SQLITE3_NATIVE_DRIVER = "SQLite3 Native Driver";
		public const String MSSQL_DRIVER = "SQL Server";
		public const String MSACCESS_DRIVER = "Microsoft Access Driver";

		private readonly TimeSpan LogAggregatorInterval = TimeSpan.FromSeconds(30);

		#endregion

		#region Static Tables

		static Dictionary<String, DataSourceType> m_DriverRegex = new Dictionary<String, DataSourceType>()
		{
			{ "^mysql native*",			DataSourceType.MySqlNative },
			{ "^mysql odbc*",			DataSourceType.MySqlOdbc},
			{ "^sqlexpress*",			DataSourceType.MSSqlExpress},
			{ "^sqlite3 odbc*",			DataSourceType.SQLiteOdbc},
			{ "^sqlite3 native*",		DataSourceType.SQLiteNative},
			{ "^sql server*",			DataSourceType.MSSqlServer},
			{ "^access*",				DataSourceType.MSAccess},
		};

		public static List<String> AvailableDrivers
		{
			get
			{
				return new List<string>()
					   {
						   MYSQL_5_1_DRIVER,
						   MYSQL_NATIVE_DRIVER,
						   SQLITE3_ODBC_DRIVER,
						   SQLITE3_NATIVE_DRIVER,
						   MSSQL_DRIVER,
					   };
			}
		}

		#endregion

		#region Public Properties
		int m_DefaultTimeout = 30;
		public int DefaultTimeout
		{
			get
			{
				return m_DefaultTimeout;
			}
			set
			{
				m_DefaultTimeout = value;
			}

		}
		List<String> m_ConnectionDroppedRegex;
		public List<String> ConnectionDroppedRegexes
		{
			get { return m_ConnectionDroppedRegex; }
			set { m_ConnectionDroppedRegex = value; }
		}

		bool m_bReadOnly;
		public bool ReadOnly { get { return m_bReadOnly; } set { m_bReadOnly = value; } }

		bool m_bTrustedSql = false;
		public bool TrustedSql { get { return m_bTrustedSql; } set { m_bTrustedSql = value; } }

		static Log m_Log;
		public static Log Log
		{
			get
			{
				if (m_Log == null)
				{
					m_Log = Log.SystemLog;
				}
				return m_Log;
			}
		}

		static DatabasePerformanceLog m_PerformanceLog;
		public static DatabasePerformanceLog PerformanceLog { get { return m_PerformanceLog; } }

		static bool m_bPerformanceLoggingEnabled = false;
		public static bool PerformanceLoggingEnabled
		{
			get { return m_bPerformanceLoggingEnabled; }
			set { m_bPerformanceLoggingEnabled = value; }
		}

		protected ISqlConnectionPool _connectionPool = null;
		public ISqlConnectionPool ConnectionPool
		{
			get { return _connectionPool; }
			set { _connectionPool = value; }
		}

		protected bool m_bCreateIfNotExists = false;
		public bool CreateIfNotExists
		{
			get { return m_bCreateIfNotExists; }
			set { m_bCreateIfNotExists = value; }
		}

		public DBResult LastResult { get; protected set; }

		#endregion

		#region Constructor(s)

		static SqlDataSource()
		{
			m_bPerformanceLoggingEnabled = false;
		}

		public SqlDataSource()
			: this(null, false) { }

		public SqlDataSource(ISqlConnectionPool pool)
			: this(pool, false) { }

		public SqlDataSource(ISqlConnectionPool pool, bool createIfNotExists)
		{
			_connectionPool = pool;
			m_bCreateIfNotExists = createIfNotExists;
			m_ConnectionDroppedRegex = new List<String>();
			m_bReadOnly = false;
			m_LogReducer = new LogOutputAggregator(Log, LogAggregatorInterval);
		}

		private LogOutputAggregator m_LogReducer;

		#endregion

		#region Query Methods

		public virtual List<T> QueryList<T>(String sql, params object[] args) where T : new()
		{
			List<T> retVal = new List<T>();
			DatabaseDataReader reader;
			QueryString query = this.Format(sql, args);
			DBResult result = Query(query, out reader, DefaultTimeout);

			if (result.ResultCode == DBResult.Result.Success)
			{
				using (reader)
				{
					bool isLoadable = new T() is ILoadable;

					while (reader.Read())
					{
						T schemaClass = new T();

						if (isLoadable)
							((ILoadable)schemaClass).LoadFrom(reader);
						else
							DataReaderConverter.LoadClassFromDataReader(schemaClass, reader);

						retVal.Add(schemaClass);
					}
					//reader.Close();
				}
			}
			return retVal;
		}

		public virtual T QueryScalar<T>(String sql, params object[] args) where T : new()
		{
			T schemaClass = default(T);
			object o;
			QueryString query = this.Format(sql, args);
			DBResult result = QueryScalar(query, out o);

			if (result.ResultCode == DBResult.Result.Success)
			{
				schemaClass = DBUtil.GetValue<T>(o);
			}

			return schemaClass;

		}

		public virtual T QueryObject<T>(String sql, params object[] args) where T : new()
		{
			T schemaClass = default(T);
			DatabaseDataReader reader;
			QueryString query = this.Format(sql, args);
			DBResult result = Query(query, out reader);

			if (result.ResultCode == DBResult.Result.Success)
			{
				using (reader)
				{
					if (reader.Read())
					{
						schemaClass = new T();
						if (schemaClass is ILoadable)
							((ILoadable)schemaClass).LoadFrom(reader);
						else
							DataReaderConverter.LoadClassFromDataReader(schemaClass, reader);
					}
				}
			}
			return schemaClass;
		}
		public virtual DBResult QueryDataSets(QueryString sql, params ILoadable[] args)
		{
			DatabaseDataReader reader;
			DBResult retVal = Query(sql, out reader, DefaultTimeout);

			if (retVal.ResultCode == DBResult.Result.Success && args.Length > 0)
			{
				using (reader)
				{
					do
					{
						ILoadable storageClass = args[reader.ResultSet];

						while (reader.Read())
						{
							storageClass.LoadFrom(reader);
						}
					} while (reader.NextResult() && reader.ResultSet <= args.Length);
				}
			}
			return retVal;
		}
		public virtual DBResult Query(QueryString sql, out DatabaseDataReader reader, int timeout = 30)
		{
			return Query(sql, null, out reader, timeout);
		}

		public virtual DBResult Query(QueryString sql, OdbcParameterList parms, out DatabaseDataReader reader, int timeout = 30)
		{
			reader = null;

			DBResult ret;

			/** we must be able to get a connection */
			DatabaseConnection connection;
			if ((ret = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
			{
#if INCLUDE_PERFORMANCE_LOGGING
				if (m_bPerformanceLoggingEnabled) { connection.SelectTimer.Start(sql.ToString()); }
#endif

				/** execute the qurey */
				ret = Query(connection, sql, parms, out reader, timeout);

#if INCLUDE_PERFORMANCE_LOGGING
				if (m_bPerformanceLoggingEnabled) { connection.SelectTimer.Stop(); }
#endif
				if (ret.ResultCode == DBResult.Result.ConnectionDropped)
				{
					/** If the connection was lost during the query, we will perform retry logic */
					int retriesLeft = connection.ConnectionRetries;

					do
					{
						Log.LogText(LogLevel.WARNING, "The connection was found to be dropped - will retry {0} of {1}", retriesLeft, connection.ConnectionRetries);
						if (connection != null)
						{
							_connectionPool.DropConnection(connection);
						}

						if ((ret = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
						{
#if INCLUDE_PERFORMANCE_LOGGING
							if (m_bPerformanceLoggingEnabled) { connection.SelectTimer.Start(sql.ToString()); }
#endif
							ret = Query(connection, sql, out reader);

#if INCLUDE_PERFORMANCE_LOGGING
							if (m_bPerformanceLoggingEnabled) { connection.SelectTimer.Stop(); }
#endif
						}
						else
						{
							/** no connection could be obtained */
							connection = null;
						}

						if (connection != null && connection.ConnectionRetryInterval != TimeSpan.Zero)
						{
							Thread.Sleep(connection.ConnectionRetryInterval);
						}
					} while (ret.ResultCode == DBResult.Result.ConnectionDropped && --retriesLeft > 0);

					if (retriesLeft == 0)
					{
						connection = null;
					}
				}

				/** if we successfully got a connection, clean up */
				if (connection != null)
				{
					/** if we have something to return, create a result set */
					if (ret.ResultCode == DBResult.Result.Success)
					{
						reader.ConnectionPool = _connectionPool;
						reader.Connection = connection;
					}
					else
					{
						ReleaseConnection(connection);
						reader = new DatabaseDataReader();
					}
				}
			}

			LastResult = ret;
			return ret;
		}

		public DBResult QueryScalar(QueryString sql, out Object retObject)
		{
			DatabaseConnection connection;
			retObject = null;
			DBResult retVal;
			if ((retVal = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
			{
#if INCLUDE_PERFORMANCE_LOGGING
				if (m_bPerformanceLoggingEnabled)
					connection.SelectTimer.Start(sql.ToString());
#endif

				retVal = QueryScalar(connection, sql, out retObject);

#if INCLUDE_PERFORMANCE_LOGGING
				if (m_bPerformanceLoggingEnabled)
					connection.SelectTimer.Stop();
#endif
				if (retVal.ResultCode == DBResult.Result.ConnectionDropped)
				{
					int retriesLeft = connection.ConnectionRetries;

					do
					{
						Log.LogText(LogLevel.WARNING, "The connection was found to be dropped - will retry {0} of {1}", retriesLeft, connection.ConnectionRetries);
						if (connection != null)
						{
							_connectionPool.DropConnection(connection);
						}

						if ((retVal = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
						{
							retVal = QueryScalar(connection, sql, out retObject);
						}

						if (connection.ConnectionRetryInterval != TimeSpan.Zero)
						{
							Thread.Sleep(connection.ConnectionRetryInterval);
						}
					} while (retVal.ResultCode == DBResult.Result.ConnectionDropped && --retriesLeft > 0);
				}

				ReleaseConnection(connection);
			}
			LastResult = retVal;

			return retVal;
		}

		public abstract DBResult Query(DatabaseConnection conn, QueryString sql, out DatabaseDataReader reader, int timeout = 30);

		public abstract DBResult Query(DatabaseConnection conn, QueryString sql, OdbcParameterList parms, out DatabaseDataReader reader, int timeout = 30);

		public abstract DBResult QueryScalar(DatabaseConnection conn, QueryString sql, out Object result);

		#endregion

		#region Insert Methods

		public DBResult Insert(QueryString sql, OdbcParameterList parameterDictionary)
		{
			if (ReadOnly)
			{
				throw new DataSourceException("{0} is read-only", this);
			}

			DatabaseConnection connection;
			DBResult result = GetConnection(out connection);
			/** must have a connection */
			if (result.ResultCode == DBResult.Result.Success)
			{
				/** do the insert */
#if INCLUDE_PERFORMANCE_LOGGING
				if (m_bPerformanceLoggingEnabled) { connection.InsertTimer.Start(sql.ToString()); }
#endif
				result = Insert(connection, sql, parameterDictionary);

#if INCLUDE_PERFORMANCE_LOGGING
				if (m_bPerformanceLoggingEnabled) { connection.InsertTimer.Stop(); }
#endif

				if (result.ResultCode == DBResult.Result.ConnectionDropped)
				{
					/** If the connection was lost during the query, we will perform retry logic */
					int retriesLeft = connection.ConnectionRetries;

					do
					{
						Log.LogText(LogLevel.WARNING, "The connection was found to be dropped - will retry {0} of {1}", retriesLeft, connection.ConnectionRetries);

						/** lose the old connection and obtain a new one */
						if (connection != null)
						{
							_connectionPool.DropConnection(connection);
						}

						if ((result = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
						{
#if INCLUDE_PERFORMANCE_LOGGING
							if (m_bPerformanceLoggingEnabled) { connection.InsertTimer.Start(sql.ToString()); }
#endif
							result = Insert(connection, sql, parameterDictionary);

#if INCLUDE_PERFORMANCE_LOGGING
							if (m_bPerformanceLoggingEnabled) { connection.InsertTimer.Stop(); }
#endif
						}
						else
						{
							connection = null;
						}

						if (connection.ConnectionRetryInterval != TimeSpan.Zero)
						{
							Thread.Sleep(connection.ConnectionRetryInterval);
						}
					} while (result.ResultCode == DBResult.Result.ConnectionDropped && --retriesLeft > 0);
				}

				/** connection will be null in the case that we couldn't get one on a retry */
				if (connection != null)
				{
					ReleaseConnection(connection);
				}
			}
			LastResult = result;

			return result;
		}

		public DBResult Insert(QueryString sql)
		{
			return Insert(sql, null);
		}

		public virtual DBResult Insert(DatabaseConnection conn, QueryString sql, OdbcParameterList parameterDictionary)
		{
			if (ReadOnly)
			{
				throw new DataSourceException("{0} is read-only", this);
			}

			DBResult ret = Update(conn, sql, parameterDictionary);

			if (ret.ResultCode == DBResult.Result.Success)
			{
				ret.ItemID = (UInt32)GetLastInsertID(conn);
			}
			else if (ret.ResultCode == DBResult.Result.UpdateFailure)
			{
				ret.ResultCode = DBResult.Result.InsertionFailure;
			}
			return ret;
		}

		#endregion

		#region Update Methods

		public DBResult Update(QueryString sql)
		{
			return Update(sql, null);
		}

		public DBResult Update(QueryString sql, OdbcParameterList parameterDictionary)
		{
			if (ReadOnly)
			{
				throw new DataSourceException("{0} is read-only", this);
			}

			DatabaseConnection connection;
			DBResult result = GetConnection(out connection);

			result = Update(connection, sql, parameterDictionary);
			if(result.ResultCode == DBResult.Result.ConnectionDropped)
			{
				/** If the connection was lost during the query, we will perform retry logic */
				int retriesLeft = connection.ConnectionRetries;

				do
				{
					Log.LogText(LogLevel.WARNING, "The connection was found to be dropped - will retry {0} of {1}", retriesLeft, connection.ConnectionRetries);

					/** lose the old connection and obtain a new one */
					if (connection != null)
					{
						_connectionPool.DropConnection(connection);
					}

					if ((result = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
					{
#if INCLUDE_PERFORMANCE_LOGGING
						if (m_bPerformanceLoggingEnabled) { connection.UpdateTimer.Start(sql.ToString()); }
#endif

						result = Update(connection, sql, parameterDictionary);

#if INCLUDE_PERFORMANCE_LOGGING
						if (m_bPerformanceLoggingEnabled) { connection.UpdateTimer.Stop(); }
#endif
					}
					else
					{
						connection = null;
					}

					if (connection != null && connection.ConnectionRetryInterval != TimeSpan.Zero)
					{
						Thread.Sleep(connection.ConnectionRetryInterval);
					}
				} while (result.ResultCode == DBResult.Result.ConnectionDropped && --retriesLeft > 0);
			}

			/** connection will be null in the case that we couldn't get one on a retry */
			if (connection != null)
			{
				ReleaseConnection(connection);
			}
			LastResult = result;
			return result;
		}

		public abstract DBResult Update(DatabaseConnection conn, QueryString sql, OdbcParameterList parms);

		#endregion

		#region Execute Methods

		public virtual DBResult Execute(DatabaseConnection conn, QueryString sql, DatabaseParameterList parms)
		{
			throw new NotImplementedException("Execute not implemented in base class");
		}

		public virtual DBResult Execute(QueryString sql, DatabaseParameterList parms)
		{
			DatabaseConnection connection;
			DBResult result = GetConnection(out connection);
			if (result.ResultCode == DBResult.Result.Success)
			{
				/** do actual work in derived class */
				result = Execute(connection, sql, parms);
				if (result.ResultCode == DBResult.Result.ConnectionDropped)
				{
					/** If the connection was lost during the query, we will perform retry logic */
					int retriesLeft = connection.ConnectionRetries;

					do
					{
						Log.LogText(LogLevel.WARNING, "The connection was found to be dropped - will retry {0} of {1}", retriesLeft, connection.ConnectionRetries);

						/** lose the old connection and obtain a new one */
						if (connection != null)
						{
							_connectionPool.DropConnection(connection);
						}

						if ((result = GetConnection(out connection)).ResultCode == DBResult.Result.Success)
						{
							result = Execute(connection, sql, parms);
						}
						else
						{
							connection = null;
						}
						if (connection.ConnectionRetryInterval != TimeSpan.Zero)
						{
							Thread.Sleep(connection.ConnectionRetryInterval);
						}
					} while (result.ResultCode == DBResult.Result.ConnectionDropped && --retriesLeft > 0);
				}
			}

			/** connection will be null in the case that we couldn't get one on a retry */
			if (connection != null)
			{
				if (result.ResultCode == DBResult.Result.Exception)
				{
					Log.LogText(LogLevel.ERROR, "Dropping connection due to Execute exception");
					_connectionPool.DropConnection(connection);
				}
				else
					ReleaseConnection(connection);
			}
			LastResult = result;
			return result;
		}

		#endregion

		#region Delete Methods

		public DBResult Delete(QueryString sql)
		{
			if (ReadOnly)
			{
				throw new DataSourceException("{0} is read-only", this);
			}

			DBResult ret = Update(sql);
			if (ret.ResultCode == DBResult.Result.UpdateFailure)
			{
				ret.ResultCode = DBResult.Result.DeleteFailure;
			}
			return ret;
		}

		public DBResult Delete(QueryString sql, OdbcParameterList parameterDictionary)
		{
			if (ReadOnly)
			{
				throw new DataSourceException("{0} is read-only", this);
			}

			DBResult ret = Update(sql, parameterDictionary);
			if (ret.ResultCode == DBResult.Result.UpdateFailure)
			{
				ret.ResultCode = DBResult.Result.DeleteFailure;
			}
			return ret;
		}

		#endregion

		#region Transactional Methods

		public DBResult BeginTransaction(DatabaseConnection conn)
		{
			return this.Update(conn, FormatTrusted("start transaction;"), null);
		}

		public DBResult Commit(DatabaseConnection conn)
		{
			return this.Update(conn, FormatTrusted("commit;"), null);
		}

		public DBResult Rollback(DatabaseConnection conn)
		{
			return this.Update(conn, FormatTrusted("rollback;"), null);
		}

		#endregion

		#region Connection Handling

		public static DatabaseConnection CreateConnection(SqlDBCredentials credentials)
		{
			String strConnect = String.Format("Driver={{{0}}};UID={1};PWD={2};Server={3};Database={4};",
											  credentials.Driver,
											  credentials.UserName,
											  credentials.Password,
											  credentials.Host,
											  credentials.Schema);
			return new DatabaseConnection(new OdbcConnection(strConnect), credentials);
		}

		public virtual DBResult GetConnection(out DatabaseConnection connection)
		{
			connection = null;
			DBResult result = new DBResult();

			if (_connectionPool == null)
			{
				/** this is a programming error, and we will not handle this case */
				throw new Exception("SqlDataSource.GetConnection() - No Connection Pool associated with data source");
			}

			try
			{
				connection = _connectionPool.GetConnection();
				result.ResultCode = DBResult.Result.Success;
			}
			catch (Exception e)
			{
				result.ResultCode = DBResult.Result.Exception;
				result.Message = e.Message;
				//Log.LogText(LogLevel.ERROR, "EXCEPTION: GetConnection: {0}", e.Message);
				m_LogReducer.LogText(LogLevel.ERROR, "EXCEPTION: GetConnection: {0}", e.Message);
			}
			LastResult = result;

			return result;
		}

		public virtual DBResult ReleaseConnection(DatabaseConnection conn)
		{
			DBResult result = new DBResult(DBResult.Result.Exception);
			if (_connectionPool == null)
			{
				throw new Exception("SqlDataSource.ReleaseConnection() - No Connection Pool associated with data source");
			}

			/** drop it if it's dead */
			if (conn.DbConnection.State != System.Data.ConnectionState.Open)
			{
				_connectionPool.DropConnection(conn);
			}
			/** otherwise release it back into our pool */
			else if (_connectionPool.ReleaseConnection(conn))
			{
				result.ResultCode = DBResult.Result.Success;
			}
			LastResult = result;
			return result;
		}

		#endregion

		#region Utility

		public virtual DBResult TestConnection()
		{
			DBResult retVal = new DBResult(DBResult.Result.Success, "TestConnection Not Implemented in this data source");
			LastResult = retVal;

			return retVal;
		}

		public virtual UInt64 GetLastInsertID(DatabaseConnection conn)
		{
			UInt64 iRet = 0;
			QueryString sql = Format("SELECT LAST_INSERT_ID()");
			Object objID = null;
			DBResult result = QueryScalar(conn, sql, out objID);
			if (result.ResultCode == DBResult.Result.Success)
			{
				iRet = Convert.ToUInt64(objID);
			}
			return iRet;
		}

		bool IsConnectionDroppedException(Exception e)
		{
			int x = 0;

			for (; x < m_ConnectionDroppedRegex.Count && Regex.IsMatch(e.Message, m_ConnectionDroppedRegex[x], RegexOptions.IgnoreCase) == false; x++) ;
			return x < m_ConnectionDroppedRegex.Count;
		}

		protected virtual DBResult HandleException(QueryString sql, Exception e)
		{
			DBResult ret = new DBResult();
			ret.Message = e.Message;
			if (IsConnectionDroppedException(e))
			{
				ret.ResultCode = DBResult.Result.ConnectionDropped;
				if (sql.Value.Length < 10000)
					Log.SysLogText(LogLevel.ERROR, "{0}: {1}", e.Message, sql);
				else
					Log.SysLogText(LogLevel.ERROR, "{0}", e.Message);
			}
			else
			{
				ret.ResultCode = DBResult.Result.Exception;

				if (sql.Value.Length < 10000)
					Log.SysLogText(LogLevel.ERROR,
								"***** SQL EXCEPTION **** >>\n" +
								"Method: {0}\n" +
								"Signature: {1}\n" +
								"[Text Of The Exception Follows]:\n" +
								"{2}\n" +
								"Stack Trace Follows]\n" +
								"{3}\n" +
								"[SQL Follows]\n" +
								"{4}",
								DatabaseMethods.Method.Name,
								DatabaseMethods.Method,
								e.Message, e.StackTrace, sql);
				else
					Log.SysLogText(LogLevel.ERROR,
								"***** SQL EXCEPTION **** >>\n" +
								"Method: {0}\n" +
								"Signature: {1}\n" +
								"[Text Of The Exception Follows]:\n" +
								"{2}\n" +
								"Stack Trace Follows]\n" +
								"{3}\n" +
								"[SQL TOO LONG TO PRINT]",
								DatabaseMethods.Method.Name,
								DatabaseMethods.Method,
								e.Message, e.StackTrace);
			}
			return ret;
		}

		public static DataSourceType GetDriverType(SqlDBCredentials credentials)
		{
			DataSourceType type = DataSourceType.Unknown;

			foreach (KeyValuePair<String, DataSourceType> kvp in m_DriverRegex)
			{
				if (Regex.IsMatch(credentials.Driver, kvp.Key, RegexOptions.IgnoreCase))
				{
					type = kvp.Value;
					break;
				}
			}
			return type;
		}

		#endregion

		#region Query String Formatting

		public virtual QueryString Format(String format, params object[] args)
		{
			return new TrustedQueryString(format, args);
		}

		public virtual QueryString FormatUntrusted(String format, params object[] args)
		{
			return new TrustedQueryString(format, args);
		}

		public virtual QueryString FormatTrusted(String format, params object[] args)
		{
			return new TrustedQueryString(format, args);
		}

		public virtual QuotedString Quoted(Object value)
		{
			return new QuotedString(true, value.ToString());
		}

		#endregion

		#region Performance Logging

		public static void OpenPerformanceLog(String strDirectory, String strNamePrefix)
		{
			if (PerformanceLoggingEnabled == false)
			{
				try
				{
					String strFileName = String.Format(@"{0}{1}{2}.{3}", strDirectory, Path.DirectorySeparatorChar, strNamePrefix, "database.log");
					m_PerformanceLog = new DatabasePerformanceLog(strFileName);
					m_bPerformanceLoggingEnabled = true;
				}
				catch (Exception e)
				{
					Log.LogText(LogLevel.DEBUG, "Exception opening database log: {0}", e.Message);
				}
			}
		}

		public static void ClosePerformanceLog()
		{
			if (PerformanceLoggingEnabled)
			{
				m_Log.Close();
				m_bPerformanceLoggingEnabled = false;
			}
		}

		#endregion
	}
}
