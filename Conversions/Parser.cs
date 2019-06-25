using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace KanoopCommon.Conversions
{
	public static class Parser
	{
		public static bool TryParse(String str, out Int32 value)
		{
			return Int32.TryParse(str, out value);
		}

		public static bool TryParse(String str, out Double value)
		{
			return Double.TryParse(str, out value);
		}

		public static bool TryParse(String str, out bool value)
		{
			bool parsed = value = false;
			str = str.ToLower();
			if(str == "true" || str == "1" || str == "on")
			{
				value = true;
				parsed = true;
			}
			else if(str == "false" || str == "0" || str == "off")
			{
				value = false;
				parsed = true;
			}
			return parsed;
		}

		/// <summary>
		/// Parse a string into a list of numbers
		/// </summary>
		/// <param name="inString"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static bool TryParse(String inString, out List<UInt32> values)
		{
			values = new List<UInt32>();

			StringBuilder sb = new StringBuilder();
			for(int index = 0;index < inString.Length;index++)
			{
				if(Char.IsWhiteSpace(inString[index]))
					continue;

				UInt32 value1;
				if(TryGetUInt32(inString, ref index, out value1) == false)
				{
					break;
				}

				values.Add(value1);

				if(index >= inString.Length)
					continue;

				if(inString[index] == ',')
				{
					continue;
				}

				if(inString[index] == '-')
				{
					UInt32 value2;
					index++;
					if(TryGetUInt32(inString, ref index, out value2) == false)
					{
						break;
					}
					for(value1++;value1 <= value2;value1++)
					{
						values.Add(value1);
					}
				}
			}
			return values.Count > 0;
		}

		static bool TryGetUInt32(String inString, ref int index, out UInt32 value)
		{
			bool result = false;
			value = 0;

			StringBuilder sb = new StringBuilder();
			for(;index < inString.Length;index++)
			{
				if(Char.IsDigit(inString[index]))
				{
					sb.Append(inString[index]);
				}
				else
				{
					if(UInt32.TryParse(sb.ToString(), out value) == true)
					{
						result = true;
					}
					break;
				}
			}

			if(result == false && UInt32.TryParse(sb.ToString(), out value) == true)
			{
				result = true;
			}
			return result;
		}

	}
}
