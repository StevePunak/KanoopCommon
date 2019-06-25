using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data.Common;

namespace KanoopCommon.Database
{
	public class MySqlDataSource : SqlDataSource
	{

		#region Constructor(s)

		public MySqlDataSource(ISqlConnectionPool pool)
			: base(pool) {}

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
			try
			{
				if (conn == null)
				{
					throw new Exception("No connection to Database. Credentials must be specified and valid.");
				}

				OdbcCommand cmd = new OdbcCommand(sql.ToString(), (OdbcConnection)conn.DbConnection);
				reader = new DatabaseDataReader(cmd.ExecuteReader());
				if(reader.HasRows == false)
				{
					ret.ResultCode = DBResult.Result.NoData;
				}
			}
			catch (Exception e)
			{
				ret = HandleException(sql, e);
			}
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

				OdbcCommand cmd = new OdbcCommand(sql.ToString(), (OdbcConnection)conn.DbConnection);
				result = cmd.ExecuteScalar();
				if (result != null)
				{
					ret.ResultCode = DBResult.Result.Success;
				}
			}
			catch (Exception e)
			{
				ret = HandleException(sql, e);
			}
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
			OdbcCommand cmd = null;
			try
			{
				if (conn == null)
					throw new Exception("No connection to Database. Credentials must be specified and valid.");

				cmd = new OdbcCommand(sql.ToString(), (OdbcConnection)conn.DbConnection);
				if (parms != null)
				{
					cmd.Parameters.AddRange(parms.ToArray());
				}
				ret.RowsAffected = cmd.ExecuteNonQuery();
				cmd.Parameters.Clear(); // Bug Fix #360
				ret.ResultCode = DBResult.Result.Success;
				ret.Message = "SUCCESS";
			}
			catch (Exception e)
			{
				ret = HandleException(sql, e);
				if(cmd != null && parms != null)
				{
					cmd.Parameters.Clear();
				}
			}
			return ret;
		}

		#endregion
	}
}
