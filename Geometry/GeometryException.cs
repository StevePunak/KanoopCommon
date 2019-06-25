using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Geometry
{
	public class GeometryException : CommonException
	{
        public GeometryException(String format, params object[] parms)
            : base(parms == null || parms.Length == 0 ? format : String.Format(format, parms)) { }
	}
}
