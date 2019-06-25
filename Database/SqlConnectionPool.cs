#undef DEBUG_CONNECTION_POOL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Threading;
using KanoopCommon.Logging;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace KanoopCommon.Database
{
	public interface ISqlConnectionPool
	{
		ISqlDataSource GetDataSource();
		SqlDBCredentials GetCredentials();

		// Allocate(lock) a connection from the pool. Create a connection if none available in pool
		DatabaseConnection GetConnection();

		// Disconnet and drop all connections related to this thread
		bool DropConnection(DatabaseConnection oldConn);

		// Release the connection back to the pool
		bool ReleaseConnection(DatabaseConnection connection);
	}

	public delegate void SqlConnectionStatusUpdate(object sender, bool isConnected, DatabaseConnection conn);

	public class SqlConnectionPool : ISqlConnectionPool
	{
		#region Constants

		public const int UNUSED_CONNECTION_TIMEOUT_SECS =			120;

		const int LOG_CONNECTION_SECS =								1;

		static readonly TimeSpan CONNECTION_CLEANUP_INTERVAL =		TimeSpan.FromSeconds(60);

		#endregion

		#region Events

		public event SqlConnectionStatusUpdate SqlConnectionStatusUpdate;

		#endregion

		#region Public Properties

		static DateTime LastConnectionLogTime { get; set; }

		#endregion

		#region Private Member Variables

		/**********************************************
		 * Statics
		 * ********************************************/
		static SqlConnectionPoolList s_PoolList = new SqlConnectionPoolList();
		static SqlDBCredentials s_DefaultCredentials = null;

		static List<String> _SkipMethods;

		/**********************************************
		 * Instance vars
		 * ********************************************/
		protected SqlDBCredentials _credentials = null;

		protected DateTime _lastConnectionCleanupTime;

		/**
		 * IMPORTANT:
		 * 
		 * Connections are either in one of these collections or the other....
		 * NEVER in both simultaneously!
		 */
		// Available Connection pool
		ConnectionList _availableConnectionStack = new ConnectionList();

		// Allocated connections
		protected Dictionary<DatabaseConnection, DatabaseConnection> _allocatedConnectionList = new Dictionary<DatabaseConnection, DatabaseConnection>();

		#endregion

		#region Internal Class Definitions

		/**********************************************
		 * Inner Classes
		 * ********************************************/
		protected class SqlConnectionPoolList: List<SqlConnectionPool> 
		{ 
			public SqlConnectionPool Find(SqlDBCredentials credentials)
			{
				SqlConnectionPool retVal = null;
				foreach (SqlConnectionPool pool in this)
				{
					if (pool.GetCredentials().Equals(credentials))
					{
						retVal = pool;
						break;
					}
				}
				return retVal;
			}
		}
		protected class ConnectionList : List<DatabaseConnection> { }

		#endregion

		#region Static Constructor

		static SqlConnectionPool()
		{
			_SkipMethods = new List<String>()
			{
				"Update",
				"Insert"
			};
		}

		#endregion

		#region Constructor(s) (Instance and Factory

		public SqlConnectionPool(SqlDBCredentials credentials)
		{
			_credentials = credentials.Clone();
			_lastConnectionCleanupTime = DateTime.UtcNow;
		}

		public static SqlConnectionPool Instance(SqlDBCredentials credentials)
		{
			SqlConnectionPool retVal = s_PoolList.Find(credentials);
			if (retVal == null)
			{
				retVal = new SqlConnectionPool(credentials);
				s_PoolList.Add(retVal);
			}
			return retVal;
		}
		
		public static SqlConnectionPool Instance()
		{
			return (s_DefaultCredentials != null) 
			? Instance(s_DefaultCredentials)
			: null;
		}

		#endregion

		#region Public Access Methods

		/**
		 * Instance(SqlDBCredentials)
		 * Gets the Connection Pool correspondant to the credentials passed in 
		 */
		public static void SetDefaultCredentials(SqlDBCredentials credentials)
		{
			s_DefaultCredentials = credentials.Clone();
		}

		public SqlDBCredentials GetCredentials()
		{
			return _credentials;
		}

		public bool DropConnection(DatabaseConnection oldConn)
		{
			bool bRet = false;
			lock (this)
			{
				bRet = _allocatedConnectionList.Remove(oldConn);
				if (bRet)
				{
					// Close the connection
					oldConn.DbConnection.Close();
				}
				else
				{
					Log.SysLogText(LogLevel.DEBUG, "SqlConnectionPool: Dropped connection which was never allocated");
				}
			}
			return bRet;
		}

		public DatabaseConnection GetConnection()
		{
			DatabaseConnection retConn = null;
			Exception reThrow = null;

			lock (this)
			{
				while(_availableConnectionStack.Count > 0)
				{
					/** take the first one */
					DatabaseConnection connection = _availableConnectionStack[0];
					_availableConnectionStack.RemoveAt(0);
					
					if(	connection.LastUse <= DateTime.UtcNow - TimeSpan.FromSeconds(UNUSED_CONNECTION_TIMEOUT_SECS) ||
						connection.DbConnection.State != ConnectionState.Open)
					{
						try
						{
							connection.DbConnection.Close();
						}
						catch{ /** this can't fail */}
						continue;
					}
					else
					{
						connection.LastUse = DateTime.UtcNow;
						retConn = connection;
						_allocatedConnectionList.Add(retConn, retConn);
						if (LastConnectionLogTime < DateTime.UtcNow - TimeSpan.FromSeconds(LOG_CONNECTION_SECS))
						{
							//Log.SysLogText(LogLevel.DEBUG, "*******   DB CONNECTIONS: ALLOCED: {0}  AVAILABLE: {1}", m_AllocatedConnectionList.Count, m_AvailableConnectionStack.Count);
							LastConnectionLogTime = DateTime.UtcNow;
						}
						break;
					}
				}

				if(retConn == null)
				{
					try
					{
						DataSourceType type = SqlDataSource.GetDriverType(_credentials);
						switch(type)
						{
							case DataSourceType.MySqlNative:
								retConn = MySqlNativeDataSource.CreateConnection(_credentials);	// new OdbcConnection(strConnect);
								break;

							default:
								retConn = SqlDataSource.CreateConnection(_credentials);			// new OdbcConnection(strConnect);
								break;
						}
						retConn.DbConnection.Open();
						_allocatedConnectionList.Add(retConn, retConn);
						if (SqlConnectionStatusUpdate != null)
						{
							SqlConnectionStatusUpdate(this, true, retConn);
						}
					}
					catch (Exception e)
					{
						reThrow = new Exception(string.Format("DBUtil EXCEPTION: {0}  CONNECT WAS {1}", e.Message, retConn.DbConnection.ConnectionString), e);
						if (SqlConnectionStatusUpdate != null)
						{
							SqlConnectionStatusUpdate(this, false, retConn);
						}
					}
				}
			}
			if (reThrow != null)
			{
				throw reThrow;
			}
#if DEBUG_CONNECTION_POOL
if(m_Credentials.UserName == "spunak")
{
	Log.LogToConsoleAndDebug("CONNDBG: GetConn Allocced New {0:x} - available: {1}  alloced: {2}  caller: {3}", 
		retConn.GetHashCode(), m_AvailableConnectionStack.Count, m_AllocatedConnectionList.Count, GetCallingMethod());
}
#endif
			return retConn;
		}

		public bool ReleaseConnection(DatabaseConnection connection)
		{
			bool bRet = false;
			lock (this)
			{
				MySqlConnection mConn = (MySqlConnection)connection.DbConnection;
				bRet = _allocatedConnectionList.Remove(connection);
				//Log.SysLogText(LogLevel.DEBUG, "Releaseing connection: {0}", mConn.ServerThread);
#if DEBUG_CONNECTION_POOL
if(m_Credentials.UserName == "rtrsvc")
{
	Log.LogToConsoleAndDebug("CONNDBG: ReleaseConn1 - available to pool {0:x} available: {1}  alloced: {2}  returned: {3}", 
		connection.GetHashCode(), m_AvailableConnectionStack.Count, m_AllocatedConnectionList.Count, bRet);
}
#endif
				if (bRet)
				{
					/** add it to the end of the list */
					_availableConnectionStack.Add(connection);
#if DEBUG_CONNECTION_POOL
if(m_Credentials.UserName == "rtrsvc")
{
	Log.LogToConsoleAndDebug("CONNDBG: ReleaseConn2 {0:x} - available: {1}  alloced: {2}  caller: {3}\n{4}",
		connection.GetHashCode(), m_AvailableConnectionStack.Count, m_AllocatedConnectionList.Count, GetCallingMethod(), GetRemainingConnectionHashCodes());
}
#endif
				}
				else
				{
					Log.SysLogText(LogLevel.DEBUG, "SqlConnectionPool: Released connection which was never allocated");
				}

				/** clean up and release old connections */
				if(DateTime.UtcNow > _lastConnectionCleanupTime + CONNECTION_CLEANUP_INTERVAL)
				{
					if(_availableConnectionStack.Count > 0)
					{
						/** sort by last use time */
						SortedList<TimeSpan, DatabaseConnection> sorter = new SortedList<TimeSpan, DatabaseConnection>();
						foreach(DatabaseConnection conn in _availableConnectionStack)
						{
							TimeSpan lastUsed = DateTime.UtcNow - conn.LastUse;
						
							/** do not allow dups */
							while(sorter.ContainsKey(lastUsed))
								lastUsed = lastUsed + TimeSpan.FromMilliseconds(1);

							sorter.Add(lastUsed, conn);
						}

						List<DatabaseConnection> sortedConnections = new List<DatabaseConnection>(sorter.Values.Reverse()); /** we now have list orderd from stalest to freshest */
						while(	sortedConnections.Count > _credentials.OptimalPoolSize &&
								DateTime.UtcNow > sortedConnections[0].LastUse + _credentials.StaleConnectionCloseTime)
						{
							sortedConnections[0].DbConnection.Close();
							_availableConnectionStack.Remove(sortedConnections[0]);
							MySqlConnection mConn2 = (MySqlConnection)sortedConnections[0].DbConnection;
							//Log.SysLogText(LogLevel.DEBUG, "Clean up old connection: {0}", mConn2.ServerThread);
						}
					}					
					_lastConnectionCleanupTime = DateTime.UtcNow;
				}
			}
			return bRet;
		}
		
		public ISqlDataSource GetDataSource()
		{
			return GetDataSource(false);
		}
		
		public ISqlDataSource GetDataSource(bool createIfNotExists)
		{
			ISqlDataSource ret = null;
			if(_credentials.DSN.Length > 0)
			{
				ret = new MySqlDataSource(this);
			}
			else
			{
				DataSourceType type = SqlDataSource.GetDriverType(_credentials);
				switch(type)
				{

					case DataSourceType.MySqlOdbc:
						ret = new MySqlDataSource(this);
						break;

					case DataSourceType.MySqlNative:
					default:
						ret = new MySqlNativeDataSource(this);
						break;

				}
			} 
			
			return ret;
		}

		#endregion

		#region Debug Utility

		static String GetCallingMethod()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("");
			StackTrace st = new StackTrace(2);
			int x = 0;
			foreach(StackFrame frame in st.GetFrames())
			{
				MethodBase method = frame.GetMethod();
				sb.AppendFormat("{0}.{1}/", method.DeclaringType.Name, method.Name);
				if(++x > 5)
					break;
			}

			return sb.ToString().TrimEnd(new char[] { '/'});
		}

		String GetRemainingConnectionHashCodes()
		{
			StringBuilder sb = new StringBuilder();

			lock(this)
			{
				foreach(DatabaseConnection connection in _allocatedConnectionList.Values)
				{
					sb.AppendFormat("{0:x}\n", connection.GetHashCode());
				}
			}

			return sb.ToString();
		}

		#endregion
	}
}
