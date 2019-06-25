using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Extensions
{
	public static class BooleanExtensions
	{
		public static String YesOrNo(this Boolean value)
		{
			return value ? "Yes" : "No";
		}

		public static bool TryParseYN(String value, out bool result)
		{
			bool parsed = false;
			result = false;
			if ((value.Length == 1) && 
				(value.ToLower() == "y" || value.ToLower() == "n") )
			{
				result = value.ToLower() == "y";
				parsed = true;
			}
			return parsed;
		}

		public static int ToZeroOrOne(this bool value)
		{
			return value ? 1 : 0;
		}

		public static bool TryParse01(String value, out bool result)
		{
			bool parsed = false;
			result = false;
			if ((value.Length == 1) && 
				(value == "0" || value.ToLower() == "1") )
			{
				result = value == "1";
				parsed = true;
			}
			return parsed;
		}

		public static bool TryParse(String value, out bool result)
		{
			bool parsed = true;
			result = true;

			if(	bool.TryParse(value, out result) == false &&
				TryParseYN(value, out result) == false &&
				TryParse01(value, out result) == false)
			{
				parsed = false;
			}

			return parsed;
		}

		public static bool Parse(String value)
		{
			bool result;
			if(TryParse(value, out result) == false)
			{
				throw new CommonException("Unparsable boolean '{0}'", value);
			}
			return result;
		}
	}
}
