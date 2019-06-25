using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Logging
{
	public enum LogLevel
	{
		ALWAYS		= 0,
		[EnumStringAttribute("DBG")]
		DEBUG 		= 1,
		[EnumStringAttribute("INF")]
		INFO		= 2,
		[EnumStringAttribute("WRN")]
		WARNING		= 3,
		[EnumStringAttribute("ERR")]
		ERROR		= 4,
		[EnumStringAttribute("FAT")]
		FATAL		= 5,
		NOTHING		= 6
	}
}
