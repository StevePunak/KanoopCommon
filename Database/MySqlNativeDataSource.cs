using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data.Common;
using MySql.Data.MySqlClient;
using KanoopCommon.Database;
using KanoopCommon.Logging;
using System.Threading;
using KanoopCommon.Threading;
using KanoopCommon.Linux;
using System.Net;

namespace KanoopCommon.Database
{
	public class MySqlNativeDataSource : SqlDataSource
	{
		#region Constructor

		public MySqlNativeDataSource(ISqlConnectionPool pool)
			: base(pool) 
		{
			ConnectionDroppedRegexes = new List<String>()
			{
				"Fatal error encountered during command execution",
				"Fatal error encountered attempting to read the resultset",
				"current state is closed",
				" disabled",
				"Server closed the connection",
				"connection is closed",
				"requires an open connection",
				"timeout period elapsed prior to completion",
				"Deadlock found when trying to get lock",
			};
		}

		#endregion

		#region Connection Creation

		public static new DatabaseConnection CreateConnection(SqlDBCredentials credentials)
		{
			int port = 3306;
			string host = credentials.Host;
			if(credentials.IsSSH)
			{
				SSHTunnels.EnsureTunnelExists(credentials);
				port = credentials.SSHCredentials.LocalTunnelPort;
				if(credentials.SSHCredentials.IsTunnel)
				{
					host = IPAddress.Loopback.ToString();
				}
				else
				{
					host = credentials.SSHCredentials.TunnelHost;
				}
				
			}

			String connectString;
			if(String.IsNullOrEmpty(credentials.Charset))
			{
				connectString = String.Format("Database={0};Data Source={1};User Id={2};Password=\x22{3}\x22;allow user variables=true;allow Zero Datetime=true;Port={4};{5};default command timeout=60",
											credentials.Schema,
											host,
											credentials.UserName,
											credentials.Password,
											port,
											credentials.Options);
			}
			else
			{
				connectString = String.Format("Database={0};Data Source={1};User Id={2};Password=\x22{3}\x22;charset={4};allow user variables=true;allow Zero Datetime=true;Port={5};{6}",
											credentials.Schema,
											host,
											credentials.UserName,
											credentials.Password,
											credentials.Charset,
											port,
											credentials.Options);
			}

			DatabaseConnection connection = null;
			try
			{
				connection = new DatabaseConnection(new MySqlConnection(connectString), credentials);
			}
			catch(Exception e)
			{
				Console.WriteLine(ThreadBase.GetFormattedStackTrace(e));
			}
			return connection;
		}

		#endregion

		#region Query Methods

		public override DBResult Query(DatabaseConnection conn, QueryString sql, out DatabaseDataReader reader, int timeout = 30)
		{
			return Query(conn, sql, null, out reader, timeout);
		}

		public override DBResult Query(DatabaseConnection conn, QueryString sql, OdbcParameterList parms, out DatabaseDataReader reader, int timeout = 30)
		{
			DBResult ret = new DBResult(DBResult.Result.Success);
			reader = null;
			MySqlCommand cmd = null;
			try
			{
				if (conn == null)
					throw new Exception("No connection to Database. Credentials must be specified and valid.");

				// Retrofit SQL to take Odbc style parameter markers
				int i = 0;
				int loc = 0;
				if (parms != null)
				{
					String sqlString = sql.ToString();
					foreach (OdbcParameter parm in parms)
					{
						loc = sqlString.IndexOf('?',loc);
						if (loc >= 0)
						{
							loc++;
							sqlString = sqlString.Substring(0, loc ) + i + sqlString.Substring(loc );
							i++;
						}

					}
					sql = sql.Trusted ? FormatTrusted(sqlString) : FormatUntrusted(sqlString);
				}

				cmd = new MySqlCommand(sql.ToString(), (MySqlConnection)conn.DbConnection);
				cmd.CommandTimeout = timeout;
				if (parms != null)
				{
					i = 0;
					foreach (OdbcParameter parm in parms)
					{
						cmd.Parameters.Add(new MySqlParameter(""+i,parm.Value));
						i++;
					}
				}

				reader = new DatabaseDataReader(cmd.ExecuteReader());
				if(reader.HasRows == false)
				{
					ret.ResultCode = DBResult.Result.NoData;
					reader.Close();
				}

			}
			catch (Exception e)
			{
				ret = HandleException(sql, e);
			}
			finally
			{
				if (cmd != null)
				{
					// cmd.Dispose();
				}
			}
			LastResult = ret;
			return ret;
		}

		public override DBResult QueryScalar(DatabaseConnection conn, QueryString sql, out Object result)
		{
			DBResult ret = new DBResult(DBResult.Result.NoData);
			result = null;
			try
			{
				if (conn == null)
					throw new Exception("No connection to Database. Credentials must be specified and valid.");

				MySqlCommand cmd = new MySqlCommand(sql.ToString(), (MySqlConnection)conn.DbConnection);
				result = cmd.ExecuteScalar();
				if(result != null)
				{
					ret.ResultCode = DBResult.Result.Success;
				}
				cmd.Dispose();
			}
			catch (Exception e)
			{
				ret = HandleException(sql, e);
			}
			LastResult = ret;
			return ret;
		}

		#endregion

		#region Update Methods

		public override DBResult Update(DatabaseConnection conn, QueryString sql, OdbcParameterList parms)
		{
			if(ReadOnly)
			{
				throw new DataSourceException("{0} is read-only", this);
			}

			DBResult ret = new DBResult(DBResult.Result.Invalid);
			try
			{
				if (conn == null)
					throw new Exception("No connection to Database. Credentials must be specified and valid.");

				// Retrofit SQL to take Odbc style parameter markers
				int i = 0;
				int loc = 0;
				if (parms != null)
				{
					String sqlString = sql.ToString();
					foreach (OdbcParameter parm in parms)
					{
						loc = sqlString.IndexOf('?',loc);
						if (loc >= 0)
						{
							loc++;
							sqlString = sqlString.Substring(0, loc ) + i + sqlString.Substring(loc );
							i++;
						}

					}
					sql = sql.Trusted ? FormatTrusted(sqlString) : FormatUntrusted(sqlString);
				}

				MySqlCommand cmd = new MySqlCommand(sql.ToString(), (MySqlConnection)conn.DbConnection);
				if (parms != null)
				{
					i = 0;
					foreach (OdbcParameter parm in parms)
					{
						cmd.Parameters.Add(new MySqlParameter(""+i,parm.Value));
						i++;
					}
					
				}
				cmd.CommandTimeout = DefaultTimeout;
				ret.RowsAffected = cmd.ExecuteNonQuery();
				ret.ResultCode = DBResult.Result.Success;
				ret.Message = "SUCCESS";
				cmd.Dispose();
			}
			catch (Exception e)
			{
				ret = HandleException(sql, e);
			}
			LastResult = ret;
			return ret;
		}

		#endregion

		#region Utility

		public override DBResult TestConnection()
		{
			DBResult dbResult = new DBResult(DBResult.Result.Success);
			try
			{

				DatabaseConnection connection;
				dbResult = GetConnection(out connection);
				if (dbResult.ResultCode != DBResult.Result.Success)
				{
					throw new Exception(dbResult.ResultCode == DBResult.Result.Exception ? dbResult.Message : "UNKOWN CONNECTION ERROR");
				}

				QueryString sql = Format("SELECT NULL");
				DatabaseDataReader reader;
				if(Query(sql, out reader).ResultCode != DBResult.Result.Success)
				{
					dbResult.ResultCode = DBResult.Result.AuthenticationFailure;
				}
				ReleaseConnection(connection);

			}
			catch(Exception e)
			{
				dbResult.ResultCode = DBResult.Result.Exception;
				dbResult.Message = e.Message;
			}
			LastResult = dbResult;
			return dbResult;
		}

		#endregion

		#region Query String Formatting

		public override QueryString Format(String format, params object[] args)
		{
			return new MySqlQueryString(TrustedSql, format, args);
		}

		public override QueryString FormatUntrusted(String format, params object[] args)
		{
			return new MySqlQueryString(false, format, args);
		}

		public override QueryString FormatTrusted(String format, params object[] args)
		{
			return new MySqlQueryString(true, format, args);
		}

		public override QuotedString Quoted(object value)
		{
			return new MySqlQuotedString(TrustedSql, value);
		}

		#endregion
	}
}
