using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KanoopCommon.Performance
{
	public class FileMemoryUsageTracker : IMemoryUsageTracker, IDisposable
	{
		TextWriter m_Writer;
		bool m_bOnlySaveAfterBaseLineSet;
		int m_nDataPointCount;

		static FileMemoryUsageTracker m_LastInstance;

		public String FileName { get; private set; }

		public int Count { get { return m_nDataPointCount; } }

		public static int DataPointCount { get { return m_LastInstance == null ? 0 : m_LastInstance.Count; } }

		long m_nLastUsage;

		public FileMemoryUsageTracker(String fileName, bool onlySaveAfterBaseLine)
		{
			m_Writer = new StreamWriter(fileName);
			m_bOnlySaveAfterBaseLineSet = onlySaveAfterBaseLine;
			m_nDataPointCount = 0;
			m_LastInstance = this;
			m_nLastUsage = 0;
		}

		public void AddDataPoint()
		{
			long bytes = 0;
			if(m_bOnlySaveAfterBaseLineSet == false)
			{
				bytes = GC.GetTotalMemory(true);
				m_Writer.WriteLine("{0}\t{1}\t{2}", bytes, MemoryUsage.TotalMemoryUsage(), DateTime.Now.ToString("HH:mm:ss.fff"));
			}
			else if(MemoryUsage.BaseLineSet)
			{
				bytes = GC.GetTotalMemory(true) - MemoryUsage.BaseLineMemoryUsage;
				m_Writer.WriteLine("{0}\t{1}\t{2}", bytes, MemoryUsage.TotalMemoryUsage(), DateTime.Now.ToString("HH:mm:ss.fff"));
			}
			if(bytes == m_nLastUsage + 16384)
			{
			}
			m_nLastUsage = bytes;
			m_nDataPointCount++;
		}

		public static String MemoryUsageAsString()
		{
			
			long bytes = GC.GetTotalMemory(true);
			return String.Format ("{0}\t{1}\t{2}", bytes, MemoryUsage.TotalMemoryUsage(), DateTime.Now.ToString("HH:mm:ss.fff"));
		}

		public void Dispose()
		{
			m_Writer.Close();
			m_Writer.Dispose();
		}
	}
}
