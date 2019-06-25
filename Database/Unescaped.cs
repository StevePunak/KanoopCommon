using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public class Unescaped
	{
		String		m_strValue;

		public Unescaped(String value)
		{
			m_strValue = value;
		}

		public static Unescaped String(String value)
		{
			return new Unescaped(value);
		}

		public static Unescaped String(object value)
		{
			Unescaped retVal = new Unescaped("NULL");
			if (value != null)
			{
				retVal = new Unescaped(value.ToString());
			}
			return retVal;
		}

		public override string ToString()
		{
			return m_strValue;
		}
	}
}
