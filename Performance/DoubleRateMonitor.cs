using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;

namespace KanoopCommon.Performance
{
	public class DoubleRateMonitor : RateMonitor<Double>
	{
		TimeSpan		m_WindowSize;
		public int WindowSeconds
		{
			set { m_WindowSize = TimeSpan.FromSeconds(value); }
		}

		public DoubleRateMonitor()
			: this(1) {}

		public DoubleRateMonitor(int nWindowSeconds)
			: base()
		{
			WindowSeconds = nWindowSeconds;
		}

		protected override RateMonitorEvent CreateEvent()
		{
			return new RateMonitorEvent();
		}

		public override double EventsPerPeriod
		{
			get
			{
				DateTime now = DateTime.UtcNow;
				Double ret = 0;

				RemoveOlderThan(m_WindowSize);

				if(Events.Count() > 0)
				{
					ret = (Double)Events.Count / (now - Events[0].EventTime).TotalSeconds;
				}
				return ret;
			}
		}
	}
}
