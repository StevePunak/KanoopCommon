using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class ListExtensions
	{
		public static void Swap<T>(this IList<T> list, int a, int b)
		{
			T tmp = list[a];
			list[a] = list[b];
			list[b] = tmp;
		}

		public static List<T> Parse<T>(String commaSeparatedList)
		{
			List<T> list = new List<T>();

			String[] parts = commaSeparatedList.Split(',');
			if(parts.Length > 0 && parts[0].Length > 0)
			{
				foreach(String part in parts)
				{
					String numString = part.Trim();
					T value = (T)Convert.ChangeType(numString, typeof(T));
					list.Add(value);
				}
			}
			return list;
		}

		public static bool TryParse<T>(String commaSeparatedList, out List<T> list)
		{
			list = new List<T>();
			bool success = true;
			try
			{
				foreach(String part in commaSeparatedList.Split(','))
				{
					String numString = part.Trim();
					if(numString.Length > 0)
					{
						T value = (T)Convert.ChangeType(numString, typeof(T));
						list.Add(value);
					}
				}
			}
			catch(Exception)
			{
				success = false;
			}

			return success;
		}

		public static IList<T> Clone<T>(this IList<T> listToClone) where T: ICloneable
		{
			return listToClone.Select(item => (T)item.Clone()).ToList();
		}

		public static String CommaDelimitedList<T>(this List<T> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach (T item in list)
			{
				sb.AppendFormat("{0},", item.ToString());
			}
			return sb.ToString().TrimEnd(',');
		}
	
	}
}
