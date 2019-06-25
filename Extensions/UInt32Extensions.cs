using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class UInt32Extensions
	{
		public static Boolean CompareList(this List<UInt32> leftList, List<UInt32> rightList)
		{
			if (leftList.Count != rightList.Count)
			{
				return false;
			}

			foreach (UInt32 entry in leftList)
			{
				if (!rightList.Contains(entry))
				{
					return false;
				}
			}
			return true;
		}

		public static void CopyList(this List<UInt32> leftList, List<UInt32> rightList)
		{
			leftList.Clear();
			foreach (UInt32 entry in rightList)
			{
				leftList.Add(entry);
			}
		}

		public static String CommaDelimitedList(this List<UInt32> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach (UInt32 item in list)
			{
				sb.AppendFormat("{0},", item.ToString());
			}
			return sb.ToString().TrimEnd(',');
		}

		public static String SemiColonDelimitedList(this List<UInt32> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach (UInt32 item in list)
			{
				sb.AppendFormat("{0};", item.ToString());
			}
			return sb.ToString().TrimEnd(';');
		}
	}
}
