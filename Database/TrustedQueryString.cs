using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public class TrustedQueryString : QueryString
	{
		public TrustedQueryString()
			: base(true) {}

		public TrustedQueryString(String other)
			: base(true, other) {}

		public TrustedQueryString(String format, params object[] args)
			: base(true, format, args) {}
	}
}
