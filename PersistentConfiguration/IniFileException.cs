using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.PersistentConfiguration
{
	public class IniFileException : CommonException
	{
		public IniFileException(String format, params object[] parms)
            : base(parms == null || parms.Length == 0 ? format : String.Format(format, parms)) { }
    }
}
