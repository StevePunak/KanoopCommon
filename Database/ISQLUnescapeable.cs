using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public interface ISQLUnescapeable
	{
		string ToSQLString();
	}
	public interface ISQLUnescaped
	{
		Unescaped ToSQLString();
	}
}
