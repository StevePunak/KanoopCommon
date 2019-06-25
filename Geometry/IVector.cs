using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Geometry
{
	public interface IVector
	{
		Double Range { get; }
		Double Bearing { get; }
	}
}
