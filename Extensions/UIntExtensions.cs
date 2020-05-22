using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class UIntExtensions
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

		/// <summary>
		/// Ensure that the value is between the two given (inclusive)
		/// </summary>
		/// <param name="value"></param>
		/// <param name="minimum"></param>
		/// <param name="maximum"></param>
		/// <returns></returns>
		public static UInt32 EnsureBetween(this UInt32 value, UInt32 minimum, UInt32 maximum)
		{
			if(value < minimum)
				return minimum;
			else if(value > maximum)
				return maximum;
			else
				return value;
		}
	}
}
