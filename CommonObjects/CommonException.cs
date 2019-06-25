using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.CommonObjects
{
	/// <summary>
	/// Base class for all exceptions generated within Kanoop libraries
	/// </summary>
    public class CommonException : Exception
    {
        public CommonException(String format, params object[] parms)
            : base(parms == null || parms.Length == 0 ? format : String.Format(format, parms)) { }
    }
}
