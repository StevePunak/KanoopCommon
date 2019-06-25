using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace KanoopCommon.Extensions
{
	public static class DateTimeExtensions
	{
		private static readonly long UnixEpochTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks; 

		public  static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		static String		_localOffsetString;
		private static int	_LastTimeZoneUpdateHour;

		static DateTimeExtensions()
		{
			UpdateTimeZone();
		}

		private static void UpdateTimeZone()
		{
			TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now); 
			int hoursOffset = utcOffset.Hours;
			int minutesOffset = utcOffset.Minutes;
			if(minutesOffset > 0)
			{
				_localOffsetString = String.Format("{0:00}{1:00}", hoursOffset, minutesOffset);
			}
			else if(hoursOffset < 0)
			{
				_localOffsetString = String.Format("{0:00}", hoursOffset);
			}
			else if(hoursOffset >= 0)
			{
				_localOffsetString = String.Format("+{0:00}", hoursOffset);
			}

			_LastTimeZoneUpdateHour = DateTime.Now.Hour;
		}

		public static DateTime FromSecondsSinceEpoch(UInt64 secondsSinceEpoch)
		{
			DateTime date = Epoch.AddSeconds(secondsSinceEpoch);
			return date;
		}

		public static DateTime FromTimeB(UInt64 secondsSinceEpoch, UInt16 milliseconds)
		{
			DateTime date = Epoch.AddSeconds(secondsSinceEpoch);
			date = date.AddMilliseconds(milliseconds);
			return date;
		}

		public static void CreateWindow(this DateTime center, TimeSpan windowSize, out DateTime start, out DateTime end)
		{
			start = center - TimeSpan.FromTicks(windowSize.Ticks / 2);
			end = center + TimeSpan.FromTicks(windowSize.Ticks / 2);
		}

		public static DateTime SubtractWeekDays(this DateTime from, int days)
		{
			DateTime time = from;
			for(int x = 0;x < days;x++)
			{
				while(time.DayOfWeek == DayOfWeek.Saturday || time.DayOfWeek == DayOfWeek.Sunday)
					time = time.AddDays(-1);
				time = time.AddDays(-1);
			}
			return time;
		}

		public static String ToDateString(this DateTime dt)
		{
			return String.Format("{0:yyyy'-'MM'-'dd HH':'mm':'ss'Z'}", dt);
		}

		public static String ToStandardFormat(this DateTime dt, bool milliseconds = false)
		{
			return ToMySqlString(dt, milliseconds);
		}

		public static String ToStandardFormatWithT(this DateTime dt, bool milliseconds = false)
		{
			return milliseconds
				? String.Format("{0:yyyy-MM-ddTHH:mm:ss.fff}", dt)
				: String.Format("{0:yyyy-MM-ddTHH:mm:ss}", dt);
		}

		public static String ToMySqlString(this DateTime dt, bool milliseconds = true)
		{
			return milliseconds  
				? String.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dt)
				: String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
		}

		public static String ToImageDateString(this DateTime dt)
		{
			return String.Format("{0:yyyy-MM-dd HHmmss}", dt);
		}

		public static String ToMySqlDate(this DateTime dt)
		{
			return String.Format("{0:yyyy-MM-dd}", dt);
		}

		public static String ToDateHashString(this DateTime dt)
		{
			return String.Format("{0:yyyyMMddHHmmssZ}", dt);
		}

		public static String ToExcelSquashedString(this DateTime dt, bool milliseconds = false)
		{
			if(milliseconds)
			{
				return String.Format("{0:yyyyMMddHHmmssfff}", dt);
			}
			else
			{
				return String.Format("{0:yyyyMMddHHmmss}", dt);
			}
		}

		public static Double ToMySQLDecimal173(this DateTime dt)
		{
			return Double.Parse(String.Format("{0:yyyyMMddHHmmss.fff}", dt));
		}

		public static String ToHMSString(this DateTime dt, bool milliseconds = false)
		{
			String format  = milliseconds ? "{0:HH:mm:ss.fff}" : "{0:HH:mm:ss}";
			return String.Format(format, dt);
		}

		public static String ToWeekdayString(this DateTime dt)
		{
			/** Wed 12/10 05:22:21 */
			return String.Format("{0:ddd MM/dd HH:mm:ss}", dt);
		}

		public static String ToIso8601String(this DateTime dt)
		{
			String ret = String.Empty;
			if(dt.Hour != _LastTimeZoneUpdateHour)
			{
				UpdateTimeZone();
			}

			if(dt.Kind == DateTimeKind.Utc)
				ret = String.Format("{0:yyyy-MM-ddTHH:mm:ss.fff}Z", dt);
			else
				ret = String.Format("{0:yyyy-MM-ddTHH:mm:ss.fff}{1:00}", dt, _localOffsetString);
			return ret;
		}

		public static String ToLogFormatString(this DateTime dt, bool milliseconds = true)
		{
			return milliseconds
				? dt.ToString("MMM dd HH:mm:ss.fff")
				: dt.ToString("MMM dd HH:mm:ss");
		}

		public static String ToHttpHeaderString(this DateTime dt)
		{
			return String.Format("{0} GMT", dt.ToUniversalTime().ToString("ddd, dd MMM yyyy HH:mm:ss"));
		}

		public static bool IsBetween(this DateTime dt, DateTime start, DateTime end)
		{
			return dt >= start && dt <= end;
		}

		public static bool IsMinimumYearTime(this DateTime dt)
		{
			return dt.DayOfYear == 1 && dt.Hour == 0 && dt.Minute == 0 && dt.Second == 0 && dt.Millisecond == 0;
		}

		public static DateTime GetDateOnly(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day);
		}

		public static DateTime GetTimeOnly(this DateTime dt)
		{
			return new DateTime(2000, 1, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
		}

		public static TimeSpan TimeOfDayDifference(DateTime d1, DateTime d2)
		{
			TimeSpan t1 = d1.TimeOfDay + TimeSpan.FromDays(1);
			TimeSpan t2 = d2.TimeOfDay + TimeSpan.FromDays(1);

			Double seconds = Math.Abs(t2.TotalSeconds - t1.TotalSeconds);

			return TimeSpan.FromSeconds(seconds);
		}

		public static TimeSpan GetDifference(DateTime d1, DateTime d2)
		{
			return TimeSpan.FromSeconds(Math.Abs((d1 - d2).TotalSeconds));
		}

		public static bool TryParseIso8601(String dateTime, out DateTime dt)
		{
			dt = DateTime.MinValue;
			bool result = false;
			
			if (dateTime.Length <= 0)
			{
				result = false;
			}
			else if(dateTime[dateTime.Length - 1] != 'Z')
			{
				// Assuming UTC because old packets are not encoded with Timezone
				if((result = DateTime.TryParseExact(dateTime, "yyyy-MM-dd'T'HH:mm:ss.ffffffzz", 
															CultureInfo.InvariantCulture,
															DateTimeStyles.AssumeUniversal, out dt)) == false)
				{
					if((result = DateTime.TryParseExact(dateTime, "yyyy-MM-dd'T'HH:mm:ss.ffffffzz", 
															CultureInfo.InvariantCulture,
															DateTimeStyles.AssumeUniversal, out dt)) == false)
					{
						result = DateTime.TryParseExact(dateTime, "yyyy-MM-dd'T'HH:mm:ss.ffffff", 
															CultureInfo.InvariantCulture,
															DateTimeStyles.AssumeUniversal, out dt);
					}
				}
			}
			else
			{
				result = DateTime.TryParseExact(dateTime, "yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'", 
															CultureInfo.InvariantCulture,
															DateTimeStyles.AssumeLocal, out dt);
			}

			if(result == false && String.IsNullOrEmpty(dateTime) == false)
			{
				result = DateTime.TryParse(dateTime, out dt);
			}
			return result;
		}

		public static bool TryParseMySqlTime(String dateTime, out DateTime dt)
		{
			dt = DateTime.MinValue;
			bool result = dateTime.Length >= 19;
			if(result)
				result = DateTime.TryParseExact(dateTime.Substring(0, 19), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
			return result;
		}

		public static bool TryParseLogFormat(String dateTime, out DateTime dt)
		{
			dt = DateTime.MinValue;
			bool result = dateTime.Length >= 19;
			if(result)
				result = DateTime.TryParseExact(dateTime.Substring(0, 19), "MMM dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
			return result;
		}

		public static bool TryParseVerbose1(String dateTime, out DateTime dt)
		{
			dt = DateTime.MinValue;
			bool result = dateTime.Length >= 25;
			if(result)      // Wed Dec 19 03:21:41 am 2018
				result = DateTime.TryParseExact(dateTime.Trim(), "ddd MMM dd HH:mm:ss tt yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
			else            // Wed Feb 14 01:14:26 2018
				result = DateTime.TryParseExact(dateTime.Trim(), "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
			return result;
		}

		public static bool TryParseUInt64(String input, out DateTime value)
		{
			return	DateTime.TryParseExact(input, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value) ||
					DateTime.TryParseExact(input, "yyyyMMddHHmmss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value);
		}

		public static bool TryParseUInt64(UInt64 input, out DateTime value)
		{
			return DateTime.TryParseExact(input.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value);
		}

		public static bool TryParseImageFileFormat(String input, out DateTime value)
		{
			bool result = true;
			if(	DateTime.TryParseExact(input, "yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value) == false &&
				DateTime.TryParseExact(input, "yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out value) == false)
			{
				result = false;
			}
			return result;
		}

		public static bool TryParse(String input, out DateTime value)
		{
			bool result = true;
			if(	DateTime.TryParse(input, out value) == false &&
				TryParseMySqlTime(input, out value) == false &&
				TryParseLogFormat(input, out value) == false &&
				TryParseIso8601(input, out value) == false &&
				TryParseUInt64(input, out value) == false &&
				TryParseVerbose1(input, out value) == false &&
				TryParseImageFileFormat(input, out value) == false)
			{
				result = false;
			}
			return result;
		}

		public static bool TryGetDateString(String fileName, out String dateString)
		{
			dateString = String.Empty;
			bool result = false;
			using(StreamReader sr = new StreamReader(fileName))
			{
				String line;
				while(result != true && (line = sr.ReadLine()) != null)
				{
					dateString = String.Empty;
					int spaceCount = 0;
					StringBuilder sb = new StringBuilder();
					for(int x = 0;x < line.Length;x++)
					{
						if(Char.IsDigit(line[x]) || line[x] == ':' || line[x] == ' ' || line[x] == '.' || line[x] == '-' || line[x] == 'Z' || line[x] == 'T')
						{
							sb.Append(line[x]);
							if(line[x] == ' ' && ++spaceCount > 1)
							{
								result = true;
								dateString = sb.ToString().TrimEnd();
								break;
							}
						}
						else
						{
							dateString = String.Empty;
							break;
						}
					}
				}
			}
			return result;
		}

		public static DateTime RemoveMilliseconds(this DateTime value)
		{
			value = new DateTime(value.Ticks - (value.Ticks % TimeSpan.TicksPerSecond));
			return value;
		}

		public static DateTime RemoveSeconds(this DateTime value)
		{
			value = new DateTime(value.Ticks - (value.Ticks % TimeSpan.TicksPerMinute));
			return value;
		}

		public static DateTime RemoveMinutes(this DateTime value)
		{
			value = new DateTime(value.Ticks - (value.Ticks % TimeSpan.TicksPerHour));
			return value;
		}

		public static DateTime ParseLogFormat(String dateTime)
		{
			DateTime dt;
			if(!DateTime.TryParseExact(dateTime, "MMM dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
			{
				dt = DateTime.MinValue;
			}
			return dt;
		}

		public static DateTime FloorSeconds(this DateTime dateTime)
		{
			return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
		}

		public static long ToMillisecondsSinceEpoch(this DateTime value)
		{
			return (value.ToUniversalTime().Ticks - UnixEpochTicks) / 10000;
		}

		public static Decimal ToUnixTime(this DateTime value)
		{
			return (Decimal)(value.Ticks - UnixEpochTicks) / (Decimal)10000000;
		}

		public static DateTime ToUTCFromMillisecondsSinceEpoch(this long millisecondsSinceEpoch)
		{
			return new DateTime(millisecondsSinceEpoch * 10000 + UnixEpochTicks, DateTimeKind.Utc);
		}

		public static int MonthsApart(this DateTime d, DateTime other, bool considerDays = false)
		{
			int monthsApart = (d.Year - other.Year) * 12 + d.Month - other.Month;

			if (considerDays)
			{
				monthsApart += d.Day >= other.Day ? 0 : -1;
			}

			return Math.Abs(monthsApart);
		}

		public static DateTime StartOfMonth(this DateTime d)
		{
			return new DateTime(d.Year, d.Month, 1);
		}

		public static int WeeksApart(this DateTime d, DateTime other, bool considerDays = false)
		{
			int weeksApart = Math.Abs(d.Year * d.WeeksInYear() + d.WeekOfYear() - (other.Year * other.WeeksInYear() + other.WeekOfYear()));

			if (considerDays)
			{
				weeksApart += d.DayOfWeek > other.DayOfWeek ? 0 : -1;
			}

			return weeksApart;
		}

		public static int WeekOfYear(this DateTime d)
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			return ci.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
		}

		public static int WeeksInYear(this DateTime d)
		{
			return new DateTime(d.Year, 12, 31).WeekOfYear();
		}

		public static DateTime StartOfWeek(this DateTime d, DayOfWeek startOfWeek)
		{
			int span = d.DayOfWeek - startOfWeek;

			if (span < 0)
			{
				span += 7;
			}

			return d.AddDays(-1 * span).Date;
		}

		public static DateTime EndOfWeek(this DateTime d, DayOfWeek endOfWeek)
		{
			int dayNumber = ((int)endOfWeek + 1) % 7;
			DateTime start = d.StartOfWeek((DayOfWeek)dayNumber);
			return start.AddDays(6).Date;
		}

		public static int DaysApart(this DateTime d, DateTime other)
		{
			return Math.Abs((int)(d - other).TotalDays);
		}

		public static string ToMonthAndDayString(this DateTime d)
		{
			return d.ToString("MMM-d");
		}

		public static string ToStandardHour(this int militaryHour)
		{
			string suffix = "am";


			if (militaryHour > 12)
			{
				if (militaryHour < 24)
				{
					suffix = "pm";
				}

				militaryHour -= 12;
			}
			else if(militaryHour == 12)
			{
				suffix = "pm";
			}
			else if (militaryHour == 0)
			{
				militaryHour = 12;
			}

			return militaryHour + suffix;
		}

		public static DateTime FromMillisecondsSinceEpoch(UInt64 milliseconds)
		{
			return Epoch + TimeSpan.FromMilliseconds(milliseconds);
		}

		public static DateTime FromMillisecondsSinceEpoch(long milliseconds)
		{
			return Epoch + TimeSpan.FromMilliseconds((ulong)milliseconds);
		}

		public static DateTime Min(DateTime dt1, DateTime dt2)
		{
			return dt1 < dt2 ? dt1 : dt2;
		}

		public static DateTime Max(DateTime dt1, DateTime dt2)
		{
			return dt1 > dt2 ? dt1 : dt2;
		}
	}
}
