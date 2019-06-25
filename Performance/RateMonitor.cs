using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;

namespace KanoopCommon.Performance
{
	public abstract class RateMonitor<T>
	{
		#region Constants

		public static int DefaultMaxEvents { get { return 10000; } }

		static readonly TimeSpan DEFAULT_PERIOD = TimeSpan.FromSeconds(1);

		#endregion

		#region Public Properties

		TimeSpan	_period;
		public TimeSpan Period { get { return _period; } set { _period = value; } }

		#endregion

		#region Protected Properties

		List<RateMonitorEvent>	_eventList;
		protected List<RateMonitorEvent>	Events { get { return _eventList; } }
		
		MutexLock				_lock;
		protected MutexLock Lock { get { return _lock; } }

		#endregion

		#region Private Member Variables

		int				_maxEvents;

		#endregion

		#region Abstract Properties

		public abstract T EventsPerPeriod { get; }

		#endregion

		protected RateMonitor()
			: this(DefaultMaxEvents) {}
		protected RateMonitor(int maxEventsPerPeriod)
			: this(maxEventsPerPeriod, DEFAULT_PERIOD) {}

		protected RateMonitor(int maxEventsPerPeriod, TimeSpan period)
		{
			_eventList = new List<RateMonitorEvent>();
			_lock = new MutexLock();
			_maxEvents = maxEventsPerPeriod;
			_period = period;
		}

		protected abstract RateMonitorEvent CreateEvent();

		public void ReceivedEvent()
		{
			ReceivedEvent(CreateEvent());
			RemoveSurplusEvents();
		}

		public void ReceivedEvents(int count)
		{
			try
			{
				_lock.Lock();

				while(count-- > 0)
					_eventList.Add(CreateEvent());
			}
			finally
			{
				_lock.Unlock();
			}

			RemoveSurplusEvents();
		}

		public void Clear()
		{
			try
			{
				_lock.Lock();

				_eventList.Clear();
			}
			finally
			{
				_lock.Unlock();
			}
		}

		protected void ReceivedEvent(RateMonitorEvent item)
		{
			try
			{
				_lock.Lock();

				_eventList.Add(item);
			}
			finally
			{
				_lock.Unlock();
			}

			RemoveSurplusEvents();
		}

		protected void RemoveSurplusEvents()
		{
			try
			{
				_lock.Lock();

				while(_eventList.Count > _maxEvents)
					_eventList.RemoveAt(0);
			}
			finally
			{
				_lock.Unlock();
			}
		}

		protected void RemoveOlderThan(TimeSpan ts)
		{
			try
			{
				_lock.Lock();
				DateTime oldest = DateTime.UtcNow - ts;
				while(_eventList.Count > 0 && _eventList[0].EventTime < oldest)
					_eventList.RemoveAt(0);
			}
			finally
			{
				_lock.Unlock();
			}
		}

	}
}
