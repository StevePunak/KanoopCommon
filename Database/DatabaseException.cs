using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Database
{
	public class DatabaseException : CommonException
	{
		public DBResult Result { get; private set; }

		public DatabaseException(String format, params object[] parms)
			: this(new DBResult(), format, parms) {}

		public DatabaseException(DBResult result, String format, params object[] parms)
			: base(format, parms) 
		{
			Result = result;
		}

	}
}
