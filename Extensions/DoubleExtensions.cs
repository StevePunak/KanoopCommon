using System;
using KanoopCommon.Geometry;
using KanoopCommon.Database;
using System.Globalization;

namespace KanoopCommon.Extensions
{
	public static class DoubleExtensions
	{
		public static string ToAbbreviatedTimeFormat(this Double milliseconds)
		{
			return TimeSpan.FromMilliseconds(milliseconds).ToAbbreviatedFormat();
		}

		public static bool EqualsAtPrecision(this Double value, Double other, int precision = EarthGeo.GeoPrecision)
		{
			bool result;
			if(precision == 0)
				result = value == other;
			else
				result = Math.Round(value, precision) == Math.Round(other, precision);
			return result;
		}

		public static string ToTimeString(this double seconds, bool showSeconds = true)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(seconds);
			TimeSpan duration = end - start;
			string time = String.Empty;

			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}
			
			if (duration.Hours > 0)
			{
				time = duration.Hours.ToString();
				time += "h ";
			}

			if (duration.Minutes > 0)
			{
				time += duration.Minutes;
				time += "m";
			}

			if (showSeconds)
			{
				time += " ";
				time += duration.Seconds;
				time += "s";
			}

			return time;
		}

		public static string ToTimeString(this decimal seconds, bool showSeconds = true)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds((double)seconds);
			TimeSpan duration = end - start;
			string time = String.Empty;

			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}
			
			if (duration.Hours > 0)
			{
				time = duration.Hours.ToString();
				time += "h ";
			}

			if (duration.Minutes > 0)
			{
				time += duration.Minutes;
				time += "m";
			}

			if (showSeconds)
			{
				time += " ";
				time += duration.Seconds;
				time += "s";
			}
			else
			{
				if (String.IsNullOrEmpty(time) && seconds > 0)
				{
					time = "< 1 min";
				}
			}

			return time;
		}

		public static string ToTimeString(this uint seconds, bool showSeconds = true)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(seconds);
			TimeSpan duration = end - start;
			string time = String.Empty;

			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}

			if (duration.Hours > 0)
			{
				time = duration.Hours.ToString();
				time += "h ";
			}

			if (duration.Minutes > 0)
			{
				time += duration.Minutes;
				time += "m";
			}

			if (showSeconds)
			{
				time += " ";
				time += duration.Seconds;
				time += "s";
			}
			else
			{
				if (String.IsNullOrEmpty(time) && seconds > 0)
				{
					time = "< 1 min";
				}
			}

			return time;
		}

		public static string ToCompactTimeString(this int seconds, bool showSeconds = true)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(Math.Abs(seconds));
			TimeSpan duration = end - start;
			string time = String.Empty;
			time = duration.Hours + ":";
			
			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}

			if (duration.Minutes <= 9)
			{
				time += "0";
			}
			
			time += duration.Minutes;

			if (showSeconds)
			{
				time += ":";

				if (duration.Seconds <= 9)
				{
					time += "0";
				}

				time += duration.Seconds;
			}

			return time;
		}

		public static string ToCompactTimeString(this double seconds, bool showSeconds = true)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(Math.Abs(seconds));
			TimeSpan duration = end - start;
			string time = String.Empty;
			time = duration.Hours + ":";
			
			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}

			if (duration.Minutes <= 9)
			{
				time += "0";
			}
			
			time += duration.Minutes;

			if (showSeconds)
			{
				time += ":";

				if (duration.Seconds <= 9)
				{
					time += "0";
				}

				time += duration.Seconds;
			}

			return time;
		}

		public static string ToCompactTimeString(this decimal seconds, bool showSeconds = true, DateTimeStyles dateTimeStyle = DateTimeStyles.AssumeUniversal)
		{
			string val = seconds.ToString();
			DateTime start = DateTime.Now;
			DateTime end = start.AddSeconds(Math.Abs((double)seconds));

			if (dateTimeStyle == DateTimeStyles.AssumeUniversal || dateTimeStyle == DateTimeStyles.AdjustToUniversal)
			{
				start = DateTime.UtcNow;
				end = start.AddSeconds(Math.Abs((double)seconds));
			}

			TimeSpan duration = end - start;
			string time = String.Empty;
			time = duration.Hours + ":";
			
			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}

			if (duration.Minutes <= 9)
			{
				time += "0";
			}
			
			time += duration.Minutes;

			if (showSeconds)
			{
				time += ":";

				if (duration.Seconds <= 9)
				{
					time += "0";
				}

				time += duration.Seconds;
			}

			return time;
		}

		public static string ToCompactTimeString(this long seconds, bool showSeconds = true)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(Math.Abs(seconds));
			TimeSpan duration = end - start;
			string time = String.Empty;
			time = duration.Hours + ":";

			if (!showSeconds && duration.Seconds >= 30)
			{
				duration.Add(new TimeSpan(0, 1, 0));
			}

			if (duration.Minutes <= 9)
			{
				time += "0";
			}

			time += duration.Minutes;

			if (showSeconds)
			{
				time += ":";

				if (duration.Seconds <= 9)
				{
					time += "0";
				}

				time += duration.Seconds;
			}

			return time;
		}

		public static string ToCompactMinutesString(this decimal seconds)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(Math.Abs((double)seconds));
			TimeSpan duration = end - start;
			string time = String.Empty;

			int minutes = duration.Hours * 60 + duration.Minutes;

			if (minutes <= 9)
			{
				time += "0";
			}

			time += minutes;
			time += ":";

			if (duration.Seconds <= 9)
			{
				time += "0";
			}

			time += duration.Seconds;
			return time;
		}

		public static string ToCompactMinutesString(this long seconds)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(Math.Abs((double)seconds));
			TimeSpan duration = end - start;
			string time = String.Empty;

			int minutes = duration.Hours * 60 + duration.Minutes;

			if (minutes <= 9)
			{
				time += "0";
			}

			time += minutes;
			time += ":";

			if (duration.Seconds <= 9)
			{
				time += "0";
			}

			time += duration.Seconds;
			return time;
		}

		public static string ToCompactMinutesString(this double seconds)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(Math.Abs(seconds));
			TimeSpan duration = end - start;
			string time = String.Empty;

			int minutes = duration.Hours * 60 + duration.Minutes;

			if (minutes <= 9)
			{
				time += "0";
			}

			time += minutes;
			time += ":";

			if (duration.Seconds <= 9)
			{
				time += "0";
			}

			time += duration.Seconds;
			return time;
		}

		public static string ToMinutes(this Double seconds)
		{
			DateTime start = DateTime.UtcNow;
			DateTime end = start.AddSeconds(seconds);
			TimeSpan duration = end - start;
			int minutes = (int)Math.Floor(duration.TotalMinutes);
			return minutes + "." + duration.Seconds + " Min";
		}
		/// <summary>
		/// Convert BigDecimal 17,3 to DateTime
		/// </summary>
		/// <param name="mysqlDateTime"></param>
		/// <returns></returns>
		public static DateTime ToDateTime(this Double mysqlDateTime, DateTimeKind kind = DateTimeKind.Unspecified)
		{
			Double date = mysqlDateTime;
			int year = (int)( date/10000000000);
			date -= year*10000000000;
		
			int month = (int)(date / 100000000);
			date -= month * 100000000;

			int day = (int)(date / 1000000);
			date -= day * 1000000;

			int hour = (int)(date / 10000);
			date -= hour * 10000;

			int minute = (int)(date / 100);
			date -= minute * 100;

			int seconds = (int) date;

			int milliseconds = (int)((mysqlDateTime - Math.Floor(mysqlDateTime)) * 1000);
			return new DateTime(year, month, day, hour, minute, seconds, milliseconds, kind);

		}

		private static readonly long UnixEpochTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
		public static DateTime ToDateTimeAsMillisecondsSinceEpoch(this Double millisecondsSinceEpoch, DateTimeKind kind)
		{
			return new DateTime((long)(millisecondsSinceEpoch) * 10000 + UnixEpochTicks, kind);
		}

		
		public static DateTime ToDateTimeAsMillisecondsSinceEpoch(this ulong millisecondsSinceEpoch, DateTimeKind kind)
		{
			return new DateTime((long)(millisecondsSinceEpoch) * 10000 + UnixEpochTicks, kind);
		}

		public static Boolean ToBoolean(this Double value)
		{
			return value != 0;
		}

		public static Int32 ToInt32(this Double value)
		{
			return (Int32)value;
		}

		public static UInt32 ToUInt32(this Double value)
		{
			return (UInt32)value;
		}

		/// <summary>
		/// REturn reciprocal bearing for given value
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Double Reciprocal(this Double value)
		{
			return value.AddDegrees(180);
		}

		/// <summary>
		/// Add the given number of degrees to the given angle
		/// </summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static Double AddDegrees(this Double value, Double count)
		{
			Double result = value + count;
			if(result >= 360)
				result -= 360;
			if(result < 0)
				result += 360;
			return result;
		}

		/// <summary>
		/// Subtract the given number of degrees to the given angle
		/// </summary>
		/// <param name="value"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static Double SubtractDegrees(this Double value, Double count)
		{
			Double result = value - count;
			if(result < 0)
				result += 360;
			if(result >= 360)
				result -= 360;
			return result;
		}

		/// <summary>
		/// Return the angular difference between this and another angle
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static Double AngularDifference(this Double value, Double other, SpinDirection direction = SpinDirection.None)
		{
			Double diff = 0;
			if(direction == SpinDirection.None)
				diff = Degrees.AngularDifference(value, other);
			else if(direction == SpinDirection.Clockwise)
			{
				diff = other - value;
				if(diff < 0)
					diff += 360;
			}
			else if(direction == SpinDirection.CounterClockwise)
			{
				diff = value - other;
				if(diff < 0)
					diff += 360;
			}
			return diff;
		}

		/// <summary>
		/// Return the direction (clockwise / counterclockwise) with shortest rotation to given bearing
		/// </summary>
		/// <param name="value"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static SpinDirection ShortestSpinDirectionTo(this Double value, Double other)
		{
			Double cw = Degrees.ClockwiseDifference(value, other);
			Double ccw = Degrees.CounterClockwiseDifference(value, other);
			return cw < ccw ? SpinDirection.Clockwise : SpinDirection.CounterClockwise;
		}

		public static bool Between(this Double value, Double d1, Double d2)
		{
			return value >= d1 && value <= d2;
		}

		/// <summary>
		/// Format double as an angle
		/// </summary>
		/// <param name="value"></param>
		/// <param name="precision"></param>
		/// <returns></returns>
		public static String ToAngleString(this Double value, int precision = 3)
		{
			String format = precision > 0
				? String.Format("{{0:0.{0}}}°", String.Empty.PadRight(precision, '0'))
				: "{0:0}°";
			return String.Format(format, value);
		}

		/// <summary>
		/// Format double to meters
		/// </summary>
		/// <param name="value"></param>
		/// <param name="precision"></param>
		/// <returns></returns>
		public static String ToMetersString(this Double value, int precision = 3)
		{
			String format = precision > 0
				? String.Format("{{0:0.{0}}}m", String.Empty.PadRight(precision, '0'))
				: "{0:0}m";
			return String.Format(format, value);
		}

		/// <summary>
		/// Is this angle within 'degrees' of the 'from' value?
		/// </summary>
		/// <param name="value">this Double</param>
		/// <param name="from">Value to see if we are within range of</param>
		/// <param name="degrees">How many degrees slack?</param>
		/// <returns></returns>
		public static bool IsWithinDegressOf(this Double value, Double from, Double degrees)
		{
			Double diff = Math.Min(Degrees.ClockwiseDifference(value, from), Degrees.CounterClockwiseDifference(value, from)); ;
			return diff <= degrees;
		}

	}
}
