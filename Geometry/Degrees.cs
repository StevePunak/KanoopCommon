using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Extensions;

namespace KanoopCommon.Geometry
{
	public class Degrees
	{
		public static Double BearingBetween(Double bearing1, Double bearing2)
		{
			Double diff = bearing1.AngularDifference(bearing2) / 2;
			Double bearing = 0;
			if(CounterClockwiseDifference(bearing1, bearing2) > ClockwiseDifference(bearing1, bearing2))
			{
				bearing = bearing1.AddDegrees(diff);
			}
			else
			{
				bearing = bearing1.SubtractDegrees(diff);
			}
			return bearing;
		}

		/// <summary>
		/// Return the angular difference in a clockwise direction
		/// </summary>
		/// <param name="bearing1"></param>
		/// <param name="bearing2"></param>
		/// <returns></returns>
		public static Double ClockwiseDifference(Double bearing1, Double bearing2)
		{
			if(bearing1 > bearing2)
			{
				bearing1 -= 360;
			}

			Double diff = bearing2 - bearing1;

			return diff;
		}

		/// <summary>
		/// Return the angular difference in a counterclockwise direction
		/// </summary>
		/// <param name="bearing1"></param>
		/// <param name="bearing2"></param>
		/// <returns></returns>
		public static Double CounterClockwiseDifference(Double bearing1, Double bearing2)
		{
			if(bearing2 > bearing1)
			{
				bearing2 -= 360;
			}

			Double diff = bearing1 - bearing2;

			return diff;
		}

		public static Double AngularDifference(Double bearing1, Double bearing2)
		{
			Double diff1 = bearing1 - bearing2;
			if(diff1 < 0)
				diff1 += 360;

			Double diff2 = bearing2 - bearing1;
			if(diff2 < 0)
				diff2 += 360;

			return Math.Min(diff1, diff2);
		}
	}
}
