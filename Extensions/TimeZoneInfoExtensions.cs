using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class TimeZoneInfoExtensions
	{
		public static bool TryGetByName(String name, out TimeZoneInfo timeZone)
		{
			List<TimeZoneInfo> allZones = new List<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones());
			timeZone = allZones.Find(z => z.StandardName.Equals(name) || z.DaylightName.Equals(name) || z.DisplayName.Equals(name));
			return timeZone != null;
		}
	}
}
