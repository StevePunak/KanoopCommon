using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KanoopCommon.Performance
{
	public class PerformanceFiles
	{
		public static String BaseDirectory
		{
			get { return Environment.GetEnvironmentVariable("MEMTEST_ROOT"); }
		}
		public static String SotapDirectory
		{
			get { return String.Format("{0}{1}sample_data{1}sotaps", BaseDirectory, Path.DirectorySeparatorChar); }
		}
		public static String IpcDirectory
		{
			get { return String.Format("{0}{1}sample_data{1}ipcs", BaseDirectory, Path.DirectorySeparatorChar); }
		}
		public static String OutputFile
		{
			get { return String.Format("{0}{1}tmp{1}out.csv", BaseDirectory, Path.DirectorySeparatorChar); }
		}
	}
}
