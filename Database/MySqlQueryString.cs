using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace KanoopCommon.Database
{
	public class MySqlQueryString : QueryString
	{
		public MySqlQueryString(bool trusted)
			: base(trusted) {}

		public MySqlQueryString(bool trusted, String other)
			: base(trusted, other) {}

		public MySqlQueryString(bool trusted, String format, params object[] args)
			: base(trusted, format, args) {}

		protected override string EscapeString(string unescaped)
		{
			return MySqlHelper.EscapeString(unescaped);
		}

		protected override QueryString Create(bool trusted, string other)
		{
			return new MySqlQueryString(trusted, other);
		}
	}
}
