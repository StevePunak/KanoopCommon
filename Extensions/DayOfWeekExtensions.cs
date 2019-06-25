using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class DayOfWeekExtensions
	{
		public static int ToMySqlDayOfWeek(this DayOfWeek day)
		{
			return (int)day + 1;
		}
	}
}
