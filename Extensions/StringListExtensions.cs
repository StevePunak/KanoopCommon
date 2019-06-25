using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class StringListExtensions
	{
		public static bool AreAllDigits(this List<String> list)
		{
			bool result = false;
			if(list.Count > 0)
			{
				result = true;
				foreach(String value in list)
				{
					Int64 x;
					if(Int64.TryParse(value, out x) == false)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
	}
}
