using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Reflection
{
	public class ReflectionParsingException : CommonException
	{
		public ReflectionParsingException(String format, params object[] parms)
			: base(format, parms) {}
	}
}
