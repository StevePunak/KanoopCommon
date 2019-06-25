using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace KanoopCommon.Performance
{
	public class MemoryUsage
	{
		static Process m_ThisProcess;
		static Dictionary<Type, long> m_UsageByType;

		static long m_BaseLineMemoryUsage;
		public static long BaseLineMemoryUsage { get { return m_BaseLineMemoryUsage; } }

		public static bool BaseLineSet { get { return m_BaseLineMemoryUsage != 0; } }

		static MemoryUsage()
		{
			m_ThisProcess = Process.GetCurrentProcess();
			m_UsageByType = new Dictionary<Type,long>();
			m_BaseLineMemoryUsage = 0;
		}

		public static long TotalMemoryUsage()
		{
			long totalMemory = 0;

			totalMemory += m_ThisProcess.NonpagedSystemMemorySize64;
			totalMemory += m_ThisProcess.PagedMemorySize64;
			totalMemory += m_ThisProcess.VirtualMemorySize64;

			return totalMemory;
		}

		public static void SetBaseLineMemoryUsage()
		{
			m_BaseLineMemoryUsage = GC.GetTotalMemory(true);
		}

		public static void AddTypeUsage(Type type, long usage)
		{
			if(m_UsageByType.ContainsKey(type) == false)
				m_UsageByType.Add(type, usage);
			else
				m_UsageByType[type] += usage;
		}

		public static void DumpTypeUsage()
		{
			while(m_UsageByType.Count > 0)
			{
				long leastCount = long.MaxValue;
				Type leastType = null;

				foreach(KeyValuePair<Type, long> kvp in m_UsageByType)
				{
					if(kvp.Value < leastCount)
					{
						leastType = kvp.Key;
						leastCount = kvp.Value;
					}
				}
				PerformanceLog.LogText("{0} - {1}", leastType, leastCount);
				m_UsageByType.Remove(leastType);
			}
		}

	}
}
