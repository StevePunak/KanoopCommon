using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public class QuotedString
	{
		bool m_bTrusted;
		public bool Trusted { get { return m_bTrusted; } }

		String m_Value;

		public QuotedString(bool trusted, Object value)
		{
			m_bTrusted = trusted;
			m_Value = value.ToString();
		}

		public override string ToString()
		{
			return m_Value;
		}
	}
}
