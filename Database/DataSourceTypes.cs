using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;

namespace KanoopCommon.Database
{
	public enum DataSourceType
	{
		Unknown,

		MySqlOdbc,
		MySqlNative,
		SQLiteOdbc,
		SQLiteNative,
		MSAccess,
		MSSqlServer,
		MSSqlExpress
	};

	public class OdbcParameterList : List<OdbcParameter>
	{
		public void Add(OdbcType type, Object objValue)
		{
			OdbcParameter parm = new OdbcParameter(type.ToString(), type);
			parm.Value = objValue;
			this.Add(parm);
		}
	}

	public enum InternalDatabaseType
	{
		Unknown = 0,

		ManagedNetwork = 1,
		DataStore = 2,
		DataStoreArchive = 3,
		QueueData = 4
	}

}
