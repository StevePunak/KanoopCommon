using System;
using System.Collections.Generic;
using System.Threading;

using System.Diagnostics;

namespace KanoopCommon.Threading
{
	public class MutexLock : IDisposable
	{
		AutoResetEvent	_event;
		UInt32			_waiters;
		public UInt32 Waiters { get { return _waiters; } }

		public MutexLock()
		{
			_event = new AutoResetEvent(true);
			_waiters = 0;
		}

		~MutexLock()
		{
			Dispose();
		}

		public void Lock()
		{
			_waiters++;
			_event.WaitOne();
		}

		public void Unlock()
		{
			_event.Set();
			_waiters--;
		}

		public void Dispose()
		{
			if (_event != null)
			{
				_event.Dispose();
				_event = null;

				GC.SuppressFinalize(this);
			}
		}
	}


	public class MutexEvent
	{
		private AutoResetEvent _event = new AutoResetEvent(false);
		private AutoResetEvent AutoResetEvent { get { return _event; } }
		
		public MutexEvent()
		{
		}

		/// <summary>
		/// Wait for the event to be triggered
		/// </summary>
		/// <param name="time"></param>
		/// <returns>true if event occurred before timeout</returns>
		public bool Wait(TimeSpan time)
		{
			return Wait((int)time.TotalMilliseconds);
		}

		/// <summary>
		/// Wait for the event to be triggered
		/// </summary>
		/// <param name="time"></param>
		/// <returns>true if event occurred before timeout</returns>
		public bool Wait(int msecs)
		{
			bool result = false;

			if(msecs == 0)
			{
				msecs = int.MaxValue;
			}

			result = _event.WaitOne(msecs, false);

            return result;
		}

		/// <summary>
		/// Wait indefinitely for the event
		/// </summary>
		public void Wait()
		{
			_event.WaitOne();
		}

		public void Set()
		{
			_event.Set();
		}

		public void Clear()
		{
			_event.Reset();
		}
	}

}





