using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Logging;
using System.Diagnostics;
using System.Reflection;
using KanoopCommon.Performance;
using KanoopCommon.Extensions;

namespace KanoopCommon.Database
{
	public class DatabasePerformanceTimer : PerformanceTimer
	{
		String		m_strSQL;
		String		m_strMethod;
		DateTime	m_StartTime;

		static List<String> m_SkipMethods;

		static DatabasePerformanceTimer()
		{
			/**
			 * put methods to skip in the stack trace here
			 */
			m_SkipMethods = new List<string>()
			{
				"Update",
				"Insert"
			};
		}

		public DatabasePerformanceTimer(String strName)
			: base(strName) {}

		public void Start(String sql)
		{
			m_strSQL = sql;
			m_strMethod = "Unknown";
			StackTrace st = new StackTrace(2);
			foreach(StackFrame frame in st.GetFrames())
			{
				MethodBase method = frame.GetMethod();
				if(m_SkipMethods.Contains(method.Name) == false)
				{
					m_strMethod = method.Name;
					break;
				}
			}
			m_StartTime = DateTime.UtcNow;
			base.Start();
		}

		public override void Stop()
		{
			base.Stop();
			String sql = m_strSQL.Replace('\r', ' ').Replace('\n', ' ');
			SqlDataSource.PerformanceLog.LogText("\x22{0}\x22, \x22{1}\x22, \x22{2}\x22, \x22{3}\x22, \x22{4}\x22",
				m_StartTime.ToStandardFormat(), Name, ElapsedMilliseconds, m_strMethod, sql);
		}

		
	}
}
