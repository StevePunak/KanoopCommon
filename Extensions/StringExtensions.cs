using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class StringExtensions
	{
		public static String SetMaxLength(this String value, int length)
		{
			return value.Length > length ? value.Substring(0, length) : value;
		}

		public static String PadToLength(this String value, int length)
		{
			return value.Length > length ? value.Substring(0, length) : value.PadRight(length);
		}

		public static String ToWindowsLineEndings(this String value)
		{
			String ret = value;
			int index = value.IndexOf('\n');
			try
			{
				do
				{
					if(index < ret.Length && ret[index + 1] != '\r')
						ret = ret.Insert(index + 1, "\r");
					index = ret.IndexOf('\n', index + 1);
				} while(index > 0);
			}
			catch(Exception)
			{

			}
			return ret;
		}

		public static bool IsAllDigits(this String value)
		{
			int index;
			for(index = 0;index < value.Length && Char.IsDigit(value[index]);index++) ;
			return index > 0 && index == value.Length;
		}

		public static String CamelCase(this String original)
		{
			StringBuilder sb = new StringBuilder();
			bool upperNext = true;

			foreach(Char c in original.ToCharArray())
			{
				if(upperNext)
				{
					sb.Append(Char.ToUpper(c));
					upperNext = false;
				}
				else if(c != '_')
				{
					sb.Append(Char.ToLower(c));
				}
				else
				{
					upperNext = true;
				}
			}

			return sb.ToString();
		}

		// https://en.wikipedia.org/wiki/S%C3%B8rensen%E2%80%93Dice_coefficient
		public static double DiceCoeffiecient(this string a, string b)
		{
			double result = 0.0;

			if(a.Length != 0 && b.Length != 0)
			{
				HashSet<char> setA = new HashSet<char>();
				HashSet<char> setB = new HashSet<char>();

				a.ToCharArray().ToList().ForEach(x => setA.Add(x));
				b.ToCharArray().ToList().ForEach(x => setB.Add(x));

				result = (2.0 * setA.Intersect(setB).Count()) / (setA.Count + setB.Count);
			}

			return result;
		}

		// https://en.wikipedia.org/wiki/Jaccard_index
		public static double JaccardIndex(this string a, string b)
		{
			double result = 0.0;

			if(a.Length != 0 && b.Length != 0)
			{
				HashSet<char> setA = new HashSet<char>();
				HashSet<char> setB = new HashSet<char>();

				a.ToCharArray().ToList().ForEach(x => setA.Add(x));
				b.ToCharArray().ToList().ForEach(x => setB.Add(x));

				double intersectionCount = setA.Intersect(setB).Count();

				result = (intersectionCount) / (setA.Count + setB.Count - intersectionCount);
			}

			return result;
		}

		public static string RemoveMultipleWhitespace(this String value, bool replaceTabs = true, bool excludeQuoted = true)
		{
			if(replaceTabs)
				value = value.TabsToSpaces();
			StringBuilder sb = new StringBuilder();
			bool lastWasWhite = false;
			bool inQuotedString = false;
			for(int x = 0;x < value.Length;x++)
			{
				if(value[x] == '\"' && excludeQuoted)
				{
					if(inQuotedString)
					{
						inQuotedString = false;
					}
					else
					{
						inQuotedString = true;
					}
					sb.Append(value[x]);
				}
				else if(Char.IsWhiteSpace(value[x]) && inQuotedString == false)
				{
					if(lastWasWhite == false)
					{
						sb.Append(value[x]);
						lastWasWhite = true;
					}
				}
				else
				{
					sb.Append(value[x]);
					lastWasWhite = false;
				}
			}
			return sb.ToString();
		}

		public static List<String> SplitWithQuotes(this String value, bool replaceTabs = true, bool excludeQuoted = true)
		{
			List<String> parts = new List<String>();

			if(replaceTabs)
				value = value.TabsToSpaces();
			StringBuilder sb = new StringBuilder();
			bool lastWasWhite = true;
			bool inQuotedString = false;
			for(int x = 0;x < value.Length;x++)
			{
				if(value[x] == '\"' && excludeQuoted)
				{
					if(inQuotedString)
					{
						inQuotedString = false;
						if(sb.Length > 0)
						{
							parts.Add(sb.ToString());
							sb.Clear();
						}
					}
					else
					{
						inQuotedString = true;
					}
				}
				else if(Char.IsWhiteSpace(value[x]) && inQuotedString == false)
				{
					if(lastWasWhite == false)
					{
						if(sb.Length > 0)
						{
							parts.Add(sb.ToString());
							sb.Clear();
						}
						lastWasWhite = true;
					}
				}
				else
				{
					sb.Append(value[x]);
					lastWasWhite = false;
				}
			}
			if(sb.Length > 0)
			{
				parts.Add(sb.ToString());
			}

			return parts;
		}

		public static bool TryGetFirstNonWhitespaceChar(this String value, out Char character, out int index)
		{
			character = default(Char);
			index = 0;
			while(index < value.Length && Char.IsWhiteSpace(value[index]))
				index++;
			if(index < value.Length)
			{
				character = value[index];
			}
			return index < value.Length;
		}

		public static string TabsToSpaces(this String value)
		{
			return value.Replace("\t", " ");
		}

		public static bool ContainsCaseInsensitive(this String haystack, String needle)
		{
			return haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		public static String Terminate(this String value)
		{
			int x = 0;
			for(x = 0;x < value.Length && value[x] >= 0x20 && value[x] <= 0x7f;x++) ;
			return value.Substring(0, x);
		}

	}

	public class CaseInsensitiveComparer : IComparer<String>
	{
		public int Compare(string x, string y)
		{
			return x.ToUpper().CompareTo(y.ToUpper());
		}
	}
}
