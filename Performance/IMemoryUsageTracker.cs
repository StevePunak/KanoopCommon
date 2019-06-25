using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Performance
{
	public interface IMemoryUsageTracker
	{
		int Count { get; }

		void AddDataPoint();
	}
}
