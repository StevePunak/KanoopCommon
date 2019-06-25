using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Types
{
	public enum IntersectionResult
	{
		Invalid = 0,

		OneIntersection = 1,
		TwoIntersections = 2,

		ContainedBy = 3,
		TooFarApart = 4,

		Same = 5
	}
}
