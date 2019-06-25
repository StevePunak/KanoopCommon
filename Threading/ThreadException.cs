using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Threading
{
	public class ThreadException : CommonException
	{
		public ThreadException(String format, params object[] parms)
			: base(format, parms) {}
	}
}
