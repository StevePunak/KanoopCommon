using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace KanoopCommon.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsNumericType(this object o)
		{
			return IsNumericType(o.GetType());
		}

		public static bool IsNumericType(Type type)
		{
			switch(Type.GetTypeCode(type))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;

				default:
					return false;
			}
		}
	}
}
