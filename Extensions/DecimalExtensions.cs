using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace KanoopCommon.Extensions
{
	public static class DecimalExtensions
	{
		private static readonly long UnixEpochTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks; 
		public static DateTime ToDateTime(this decimal timestamp)
		{
			return DateTime.ParseExact(timestamp.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}
		public static DateTime ToUTCDateTime(this Decimal millisecondsSinceEpoch)
		{
			return new DateTime((long)(millisecondsSinceEpoch *(decimal)1000)* 10000 + UnixEpochTicks, DateTimeKind.Utc);
		}
		public static DateTime ToDateTime(this Decimal millisecondsSinceEpoch, DateTimeKind dtKind )
		{
			DateTime retVal = new DateTime((long)(millisecondsSinceEpoch * (decimal)1000) * 10000 + UnixEpochTicks, dtKind);
			return retVal;
		}
		public static DateTime FromUnixToDateTime(this Decimal millisecondsSinceEpoch, DateTimeKind kind= DateTimeKind.Unspecified)
		{
			return new DateTime((long)(millisecondsSinceEpoch * (decimal)1000) * 10000 + UnixEpochTicks, kind);
		}
	}
}
