using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;

namespace KanoopCommon.Performance
{
	/// <summary>
	/// Very efficient eventrate monitor. Will not return non-integral values.
	/// For floating-point rate calculations use DoubleRateMonitor
	/// </summary>
	public class EventRateMonitor : RateMonitor<Int32>
	{
		#region Constants

		static readonly TimeSpan DEFAULT_PERIOD = TimeSpan.FromSeconds(1);
		static readonly int DEFAULT_MAX_EVENTS = 1000;

		#endregion

		public EventRateMonitor()
			: this(DEFAULT_MAX_EVENTS) {}

		public EventRateMonitor(TimeSpan period)
			: this(DEFAULT_MAX_EVENTS, period) {}

		public EventRateMonitor(int maxEvents)
			: this(maxEvents, DEFAULT_PERIOD) {}

		public EventRateMonitor(int maxEvents, TimeSpan period)
			: base(maxEvents, period) 
		{
		}

		public override Int32 EventsPerPeriod
		{
			get
			{
				RemoveOlderThan(Period);
				return Events.Count();
			}
		}

		protected override RateMonitorEvent CreateEvent()
		{
			return new RateMonitorEvent();
		}

	}
}
