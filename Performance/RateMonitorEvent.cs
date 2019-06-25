using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Performance
{
	public class RateMonitorEvent
	{
		DateTime m_EventTime;
		public DateTime EventTime { get { return m_EventTime; } }

		public RateMonitorEvent()
		{
			m_EventTime = DateTime.UtcNow;
		}
	}
}
