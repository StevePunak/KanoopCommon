using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;

namespace KanoopCommon.Performance
{
	public class BitRateMonitor : RateMonitor<UInt32>
	{
		class DataEvent : RateMonitorEvent
		{
			UInt32 m_nBytes;
			public UInt32 Bytes { get { return m_nBytes; } }

			public DataEvent(UInt32 nBytes)
				: base()
			{
				m_nBytes = nBytes;
			}
		}

		public BitRateMonitor()
			: base() {}

		protected override RateMonitorEvent CreateEvent()
		{
			return new DataEvent(0);
		}

		public void AddBytes(UInt32 nCount)
		{
			ReceivedEvent(new DataEvent(nCount));
		}

		public override UInt32 EventsPerPeriod
		{
			get { return BytesPerSecond; }
		}

		public UInt32 BytesPerSecond
		{
			get
			{
				RemoveOlderThan(TimeSpan.FromSeconds(1));

				UInt32 nRet = 0;
				try
				{
					Lock.Lock();
					foreach(DataEvent e in Events)
					{
						nRet += e.Bytes;
					}
				}
				finally
				{
					Lock.Unlock();
				}

				return nRet;
			}
		}

		public override string ToString()
		{
			String strRet = "";
			Double ret = (Double)BytesPerSecond;
			if(ret > 1000000)
			{
				strRet = String.Format("{0:.00} MB/ps", ret / 1048576);
			}
			else if(ret > 1000)
			{
				strRet = String.Format("{0:.00} KB/ps", ret / 1024);
			}
			else
			{
				strRet = String.Format("{0} B/ps", ret);
			}
			return strRet;
		}

	}
}
