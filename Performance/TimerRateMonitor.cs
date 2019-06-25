using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace KanoopCommon.Performance
{
	public class TimerRateMonitor
	{
		#region Constants

		#endregion

		#region Public Properties

		public int EventsPerSecond { get { return m_nLastEventsPerSecond; } }

		#endregion

		#region Private Member Variables

		Timer	m_OneSecondTimer;
		int			m_nEventCount;
		int			m_nLastEventsPerSecond;

		#endregion

		#region Constructors

		public TimerRateMonitor()
		{
			m_OneSecondTimer = new Timer();
			m_OneSecondTimer.AutoReset = true;
			m_OneSecondTimer.Interval = 1000;
			m_OneSecondTimer.Enabled = true;
			m_OneSecondTimer.Elapsed += new ElapsedEventHandler(OnOneSecondTimerElapsed);
			m_OneSecondTimer.Start();
		}

		#endregion

		#region Public Access Methods

		public void ReceivedEvent()
		{
			m_nEventCount++;
		}

		public void ReceivedEvents(int count)
		{
			m_nEventCount += count;
		}

		#endregion

		#region Event Handler

		void OnOneSecondTimerElapsed(object sender, ElapsedEventArgs e)
		{
			m_nLastEventsPerSecond = m_nEventCount;
			m_nEventCount = 0;
		}

		#endregion

	}
}
