using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class Int32Extensions
	{
		public static int IncrementRoundRobin(this int value, int roundRobinSize)
		{
			int ret = value + 1 == roundRobinSize ? 0 : value + 1;
			return ret;
		}

		public static int DecrementRoundRobin(this int value, int roundRobinSize)
		{
			int ret = value - 1 == -1 ? roundRobinSize - 1 : value - 1;
			return ret;
		}

		public static bool IsBetween(this int value, int min, int max)
		{
			return value >= min && value <= max;
		}

		public static int AbsoluteDifference(this int value, int other)
		{
			return Math.Abs(value - other);
		}

		public static int EnsureBetween(this int value, int minimum, int maximum)
		{
			if(value < minimum)
				return minimum;
			else if(value > maximum)
				return maximum;
			else
				return value;
		}

		public static UInt32 EnsureBetween(this UInt32 value, UInt32 minimum, UInt32 maximum)
		{
			if(value < minimum)
				return minimum;
			else if(value > maximum)
				return maximum;
			else
				return value;
		}
	}
}
