using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class TypeExtensions
	{
		public static bool IsNumeric(this Type type)
		{
			bool result = false;

			switch(Type.GetTypeCode(type))
			{
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					result = true;
					break;
			}

			return result;
		}

	}
}
