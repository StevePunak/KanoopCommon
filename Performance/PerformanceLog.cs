using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Logging;

namespace KanoopCommon.Performance
{
	public class PerformanceLog
	{
		public static void OpenSystemLog()
		{
			Log log = new Log();
			log.Open(LogLevel.DEBUG, OpenFlags.OUTPUT_TO_CONSOLE | OpenFlags.OUTPUT_TO_DEBUG | OpenFlags.COMBO_VERBOSE);
			Log.SystemLog = log;
		}

		public static void CloseSystemLog()
		{
			Log.SystemLog.Close();
			Log.SystemLog = null;
		}

		public static void LogText(String format, params object[] parms)
		{
			if(Log.SystemLog != null)
			{
				Log.SysLogText(LogLevel.DEBUG, format, parms);
			}
			Console.WriteLine(format, parms);
		}
	}
}
