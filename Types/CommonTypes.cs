using System;
using System.Collections.Generic;
using System.Text;
using KanoopCommon.CommonObjects;
using System.ComponentModel;
using KanoopCommon.Conversions;
using KanoopCommon.Addresses;

namespace KanoopCommon.Types
{
	public enum OperatingSystemType
	{
		UNKNOWN = 0,

		LINUX = 1,
		WINDOWS_XP = 2,
		WINDOWS_NT = 3,
		WINDOWS_VISTA = 4,
		SOLARIS = 5,
		WINDOWS_MOBILE = 6,
		SYMBIAN = 7,
		BREW = 8,
		BLACKBERRY = 9,
		IOS = 10,
		ANDROID = 11,
		MAEMO = 12,
		MEEGO = 13,
		WINDOWS_XP_64 = 14,
		WINDOWS_VISTA_64,
		WINDOWS_SERVER_2003,
		WINDOWS_SERVER_2003_64,
		WINDOWS_7,
		WINDOWS_7_32,
		WINDOWS_SERVER_2008,
		WINDOWS_SERVER_2008_32,
		WINDOWS_SERVER_2008_R2,
		WINDOWS_SERVER_2008_R2_32
	}

	public static class OSExtensions
	{
		public static bool IsWindows(this OperatingSystemType os)
		{
			return os.ToString().Contains("WINDOWS");
		}
	}

	public enum Comparator
	{
		EQUAL_TO = 0,
		LESS_OR_EQUAL = 1,
		LESS_THAN = 2,
		GREATER_OR_EQUAL = 3,
		GREATER = 4,
	}

	public enum CardinalDirection
	{
		Invalid = 0,
		North = 1,
		East = 2,
		South = 3,
		West = 4
	}

	public enum ComparisonResult
	{
		EqualTo = 0,
		GreaterThan = 1,
		LessThan = 2
	}


}
