using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.CommonObjects
{
	/// <summary>
	/// This type is not legal for the attempted usage
	/// </summary>
	public class IllegalTypeComparison : CommonException
	{
        public IllegalTypeComparison(String format, params object[] parms)
            : base(format, parms) { }

	}
}
