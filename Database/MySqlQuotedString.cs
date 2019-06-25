using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace KanoopCommon.Database
{
	public class MySqlQuotedString : QuotedString
	{
		public MySqlQuotedString(bool trusted, Object value)
			: base(trusted, BuildString(trusted, value)) {}

		static String BuildString(bool trusted, object value)
		{
			String what = value == null ? System.String.Empty : value.ToString();
			return System.String.Format("'{0}'", trusted ? what : MySqlHelper.EscapeString(what));
		}
	}
}
