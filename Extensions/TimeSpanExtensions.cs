using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class TimeSpanExtensions
	{
		public const Double DaysPerYear = 365.242;
		public const long MicrosecondsPerTick = TimeSpan.TicksPerMillisecond / 1000;

		static readonly Dictionary<String, SpanType> _stringToSpanIndex = new Dictionary<String, SpanType>()
		{
			{ "d",          SpanType.Days },
			{ "h",          SpanType.Hours },
			{ "m",          SpanType.Minutes },
			{ "s",          SpanType.Seconds },
			{ "ms",         SpanType.Milliseconds },
		};

		enum SpanType
		{
			Days,
			Hours,
			Minutes,
			Seconds,
			Milliseconds
		}

		public static String ToShortString(this TimeSpan ts)
		{
			StringBuilder sb = new StringBuilder();
			if(ts.Days >= 1)
				sb.AppendFormat("{0}d ", (int)ts.Days);
			if(ts.Hours >= 1)
				sb.AppendFormat("{0}h ", (int)ts.Hours);
			sb.AppendFormat("{0}m ", (int)ts.Minutes);
			sb.AppendFormat("{0}s", (int)ts.Seconds);
			return sb.ToString();
		}

		public static string ToAbbreviatedFormat(this TimeSpan span, bool milliseconds = false)
		{
			StringBuilder sb = new StringBuilder();
			if (span == TimeSpan.Zero)
				sb.Append("0m");
			else if(span == TimeSpan.MaxValue)
				sb.Append("never");
			else
			{
				if(span.Ticks < 0)
				{
					sb.Append("-");

				}

				Double days = span.Days;
				Double years = 0;

				if(days > DaysPerYear)
				{
					years = days / DaysPerYear;
					days = days % DaysPerYear;
				}

				if (years != 0)
					sb.AppendFormat("{0}y ", (int)Math.Abs(years));
				if (days != 0)
					sb.AppendFormat("{0}d ", (int)Math.Abs(days));
				if (span.Hours != 0)
					sb.AppendFormat("{0}h ", (int)Math.Abs(span.Hours));
				if (span.Minutes != 0)
					sb.AppendFormat("{0}m ", (int)Math.Abs(span.Minutes));
				if (span.Seconds != 0)
					sb.AppendFormat("{0}s ", (int)Math.Abs(span.Seconds));
				if(milliseconds)
				{
					if (span.Milliseconds != 0)
						sb.AppendFormat("{0}ms ", Math.Abs(span.Milliseconds % 999));
				}
			}
			return sb.ToString().Trim();

		}

		public static TimeSpan Parse(String stringValue)
		{
			TimeSpan ts = TimeSpan.MinValue;
			if(TryParse(stringValue, out ts) == false)
			{
				throw new InvalidCastException("Could not parse timespan");
			}
			return ts;
		}

		public static bool TryParse(String stringValue, out TimeSpan timespan)
		{
			bool result = false;
			if((result = TimeSpan.TryParse(stringValue, out timespan)) == false)
			{
				int partsParsed = 0;
				bool parseFailure = false;

				int days = 0, hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

				StringBuilder sb = new StringBuilder();
				for(int x = 0;x < stringValue.Length;x++)
				{
					if(Char.IsDigit(stringValue[x]))
					{
						sb.Append(stringValue[x]);
					}
					else if(Char.IsWhiteSpace(stringValue[x]))
					{
						continue;
					}
					else
					{
						if(sb.ToString().Length == 0)
						{
							break;
						}

						int value;
						if(Int32.TryParse(sb.ToString(), out value) == false)
						{
							break;
						}

						String typeValue = stringValue.Length >= x + 2 && Char.IsLower(stringValue[x + 1])
							? stringValue.Substring(x, 2)
							: stringValue.Substring(x, 1);

						SpanType spanType;
						if(_stringToSpanIndex.TryGetValue(typeValue, out spanType))
						{
							x += typeValue.Length - 1;
							switch(spanType)
							{
								case SpanType.Days:
									days = value;
									partsParsed++;
									sb = new StringBuilder();
									break;
								case SpanType.Hours:
									hours = value;
									partsParsed++;
									sb = new StringBuilder();
									break;
								case SpanType.Minutes:
									minutes = value;
									partsParsed++;
									sb = new StringBuilder();
									break;
								case SpanType.Seconds:
									seconds = value;
									partsParsed++;
									sb = new StringBuilder();
									break;
								case SpanType.Milliseconds:
									milliseconds = value;
									partsParsed++;
									sb = new StringBuilder();
									break;
								default:
									parseFailure = true;
									break;
							}
						}
						else
						{
							parseFailure = true;
							break;
						}
					}
				}

				if(partsParsed > 0 && parseFailure == false)
				{
					timespan = new TimeSpan(days, hours, minutes, seconds, milliseconds);
					result = true;
				}
			}
			return result;
		}

		public static string ToHMSString(this TimeSpan span)
		{
			return span.ToString(@"hh\:mm\:ss");
		}

		public static bool IsWithin(this TimeSpan span, TimeSpan howMuch, TimeSpan ofTarget)
		{
			bool result = span + howMuch >= ofTarget && span - howMuch <= ofTarget;
			return result;
		}

		public static bool IsBetween(this TimeSpan span, TimeSpan start, TimeSpan end)
		{
			bool result = false;
			if(start < end)
			{
				result = span >= start && span <= end;
			}
			else
			{
				result = span >= start || span <= end;
			}
			return result;
		}

		public static TimeSpan TimeOfDayDiff(this TimeSpan span, TimeSpan other)
		{
			TimeSpan lower = Min(span, other);
			TimeSpan upper = Max(span, other);

			TimeSpan ts1 = upper - lower;
			TimeSpan ts2 = (lower + TimeSpan.FromDays(1) - upper);

			return Min(ts1, ts2);
		}

		public static void CreateWindow(this TimeSpan center, TimeSpan windowSize, out TimeSpan start, out TimeSpan end)
		{
			start = center - TimeSpan.FromTicks(windowSize.Ticks / 2);
			end = center + TimeSpan.FromTicks(windowSize.Ticks / 2);
		}

		public static TimeSpan Min(TimeSpan s1, TimeSpan s2)
		{
			return s1 < s2 ? s1 : s2;
		}

		public static TimeSpan Max(TimeSpan s1, TimeSpan s2)
		{
			return s1 > s2 ? s1 : s2;
		}

		public static TimeSpan Abs(this TimeSpan span)
		{
			return TimeSpan.FromSeconds(Math.Abs(span.TotalSeconds));
		}

		public static int ByteArraySize { get { return sizeof(UInt64); } }

		public static TimeSpan Average(List<TimeSpan> timeSpans)
		{
			Double value = 0.0;
			foreach (TimeSpan timeSpan in timeSpans)
			{
				value += timeSpan.TotalMilliseconds;
			}
			return new TimeSpan (0, 0, Convert.ToInt32 (value/(1000.0 * Convert.ToDouble (timeSpans.Count))));
		}

		public static byte[] Serialize(this TimeSpan timespan)
		{
			byte[] serialized = new byte[ByteArraySize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(serialized)))
			{
				bw.Write(timespan.Ticks);
			}
			return serialized;
		}

		public static TimeSpan Deserialize(BinaryReader br)
		{
			UInt64 ticks = br.ReadUInt64();
			return TimeSpan.FromTicks((long)ticks);
		}

		public static long TotalMicroseconds(this TimeSpan value)
		{
			return value.Ticks / MicrosecondsPerTick;
		}

		public static long CurrentMicroseconds
		{
			get { return DateTime.UtcNow.Ticks / MicrosecondsPerTick; }
		}
	}
}
