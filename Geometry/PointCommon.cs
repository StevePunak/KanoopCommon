using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Crypto;

namespace KanoopCommon.Geometry
{
	internal static class PointCommon
	{
		public static int ComparePoints(PointD point1, PointD point2, int precision)
		{
			int result;

			if(	(result = Math.Round(point1.X, precision).CompareTo(Math.Round(point2.X, precision))) == 0 &&
				(result = Math.Round(point1.Y, precision).CompareTo(Math.Round(point2.Y, precision))) == 0 )
			{
				/** result is 0, compare equal */
			}
			else
			{
				/** we will use the first non-equal value */
			}

			return result;
		}

		public static int GetHashCode(PointD point)
		{
			return (int)MD5.HashStringAsUInt32(point.ToString());
		}

		public static bool Equals(PointD thisPoint, object other)
		{
			return ((PointD)other).X == thisPoint.X && ((PointD)other).Y == thisPoint.Y;
		}
	}
}
