#undef LOG_SPIN
#define MONITOR_PERFORMANCE
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

using KanoopCommon.Logging;
using KanoopCommon.Types;
using KanoopCommon.Performance;

namespace KanoopCommon.Threading
{
	#region Delegates

	public delegate void ThreadStartedHandler(ThreadBase thread);
	public delegate void ThreadStateChangedHandler(ThreadBase thread, ThreadBase.ThreadState state);
	public delegate void ThreadCompleteHandler(ThreadBase thread);
	public delegate void ThreadProgressHandler(ThreadBase thread, Int32 progressValue, String progressMessage);

	#endregion

	public abstract class ThreadBase
	{
		#region Constants

		const int DEFAULT_STARTUP_TIMEOUT_MS = 30 * 1000;
		const int DEFAULT_SHUTDOWN_TIMEOUT_MS = 30 * 1000;

		#endregion

		#region Events

		public event ThreadStartedHandler ThreadStarted;
		public event ThreadStateChangedHandler StateChanged;
		public event ThreadCompleteHandler ThreadComplete;
		public event ThreadProgressHandler ThreadProgress;

		#endregion

		#region Enumerations

		public enum ThreadState
		{
			Started = 0,
			Stopping = 1,
			Stopped = 2,
			Pausing = 3,
			Paused = 4,
			Resuming = 5,
			Aborted = 6,
			Starting = 7,

			Unknown = 1000
		}

		public enum ThreadResult
		{
			Success = 0,
			Timedout = 1,
			Aborted = 2,
			Canceled = 3
		}

		#endregion

		#region Public Properties

		ThreadState _state;
		public ThreadState State
		{
			get { return _state; }
		}

		ThreadResult _threadResult;
		public ThreadResult Result
		{
			get { return _threadResult; }
			set { _threadResult = value; }
		}

		String _name;
		public String Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public String Message { get; protected set; }

		int _interval;
		public TimeSpan Interval
		{
			get { return TimeSpan.FromMilliseconds(_interval); }
			set { _interval = (int)value.TotalMilliseconds; }
		}

		int _startupTimeout;
		public int StartupTimeout
		{
			get { return _startupTimeout; }
			set { _startupTimeout = value; }
		}

		int _shutdownTimeout;
		public int ShutdownTimeout
		{
			get { return _shutdownTimeout; }
			set { _shutdownTimeout = value; }
		}

		UInt64 _runCount;
		public UInt64 RunCount { get { return _runCount; } }

		public ThreadPriority Priority
		{
			get { return _thread.Priority; }
			set { _thread.Priority = value; }
		}

		public int ThreadID { get; private set; }

		public DateTime StartTime { get { return _startTime; } }

		public DateTime StartRunTime { get; set; }

		public TimeSpan RunTime { get { return _startTime != DateTime.MinValue ? DateTime.UtcNow - _startTime : TimeSpan.Zero; } }

		PerformanceTimer _performanceTimer;

		public Exception LastThreadException { get; private set; }

		private Log _log;
		public Log Log
		{
			get
			{
				return ( _log == null ) ? Log.SystemLog : _log;
			}
			set { _log = value; }
		}

		static MutexLock _threadRunCounterLock;
		static Dictionary<int, int> _ThreadRunCounter;
		static bool _logThreadRun;
		public static bool LogThreadRun
		{
			get { return _logThreadRun; }
			set
			{
				try
				{
					_threadRunCounterLock.Lock();
					if(_logThreadRun == false && value == true)
					{
						_logThreadRun = true;
						_ThreadRunCounter = new Dictionary<int, int>();
					}
					else if(_logThreadRun == true && value == false)
					{
						_logThreadRun = false;
					}
				}
				finally
				{
					_threadRunCounterLock.Unlock();
				}
			}

		}

		public Object Tag { get; set; }

		#endregion

		#region Private Member Variables

		private MutexEvent _startupEvent;
		private MutexEvent _shutdownEvent;
		private MutexEvent _resumeEvent;
		private MutexEvent _pausedEvent;
		private MutexEvent _threadCompleteEvent;

		private Thread _thread;

		private DateTime _startTime;
		private bool _didNiceAbort;

		private static List<ThreadBase> _startedThreads;
		private static MutexLock _threadListLock;

		#endregion

		#region Constructor(s)

		static ThreadBase()
		{
			_startedThreads = new List<ThreadBase>();
			_threadListLock = new MutexLock();
			_logThreadRun = false;
			_threadRunCounterLock = new MutexLock();
		}

		public ThreadBase(String name)
		{
			_startupEvent = new MutexEvent();
			_shutdownEvent = new MutexEvent();
			_pausedEvent = new MutexEvent();
			_resumeEvent = new MutexEvent();
			_threadCompleteEvent = new MutexEvent();

			_name = name;
			_didNiceAbort = false;
			_state = ThreadState.Stopped;

			_interval = 0;

			_startupTimeout = DEFAULT_STARTUP_TIMEOUT_MS;
			_shutdownTimeout = DEFAULT_SHUTDOWN_TIMEOUT_MS;

			_runCount = 0;

			_startTime = DateTime.MinValue;

			_performanceTimer = new PerformanceTimer(name);

			Tag = null;
			Message = String.Empty;
		}

		#endregion

		#region Public Access Methods (from outside contexts)

		public virtual bool Start(TimeSpan startupTimeout)
		{
			_startupTimeout = (int)startupTimeout.TotalMilliseconds;
			return _Start();
		}

		public virtual bool Start(int runInterval, int startupTimeout = DEFAULT_STARTUP_TIMEOUT_MS)
		{
			_interval = runInterval;
			_startupTimeout = startupTimeout;
			return _Start();
		}

		public virtual bool Start()
		{
			return _Start();
		}

		bool _Start()
		{
			bool bRet = false;

			if(_state != ThreadState.Stopped)
			{
				throw new Exception(String.Format("THREAD EXCEPTION [{0}]: Start() called while thread not in Stopped state", Name));
			}

			_startTime = DateTime.UtcNow;

			_thread = new Thread(ThreadMain);
			_thread.Name = String.Format("{0} [{1}]", _name, _thread.GetHashCode());
			_thread.Start();
			if(( bRet = _startupEvent.Wait(_startupTimeout) ) == false)
			{
				_thread.Abort();
				ChangeState(ThreadState.Aborted);
			}
			else
			{
				if(ThreadStarted != null)
				{
					ThreadStarted(this);
				}

				try
				{
					_threadListLock.Lock();
					_startedThreads.Add(this);
				}
				finally
				{
					_threadListLock.Unlock();
				}
			}
			return bRet && _state != ThreadState.Stopped;
		}

		public virtual bool Stop()
		{
			return Stop(_shutdownTimeout);
		}

		public virtual bool Stop(int timeout)
		{
			bool result = false;

			if(_state != ThreadState.Started && _state != ThreadState.Paused && _state != ThreadState.Stopping && _state != ThreadState.Stopped)
			{
				throw new Exception(String.Format("THREAD EXCEPTION [{0}]: Stop() called while thread not in Started state", Name));
			}

			_shutdownTimeout = timeout;

			if(_state != ThreadState.Stopped)
			{
				ChangeState(ThreadState.Stopping);

				_shutdownEvent.Set();
				_resumeEvent.Set();

				if(( result = _thread.Join(_shutdownTimeout) ) == false)
				{
					_thread.Abort();
					ChangeState(ThreadState.Aborted);
				}
				else
				{
					ChangeState(ThreadState.Stopped);
				}

				_startTime = DateTime.MinValue;

				_threadCompleteEvent.Set();
			}
			return result;
		}

		public virtual void BeginStop()
		{
			if(_state != ThreadState.Started && _state != ThreadState.Paused && _state != ThreadState.Stopping && _state != ThreadState.Stopped)
			{
				throw new Exception(String.Format("THREAD EXCEPTION [{0}]: BeginStop() called while thread not in Started state", Name));
			}

			if(_state != ThreadState.Stopped)
			{
				ChangeState(ThreadState.Stopping);

				_shutdownEvent.Set();
				_resumeEvent.Set();

			}
		}

		public virtual bool Pause() { return _Pause(0); }
		public virtual bool Pause(int nTimeout) { return _Pause(nTimeout); }
		bool _Pause(int nTimeout)
		{
			bool bRet = true;

			if(_state != ThreadState.Started)
			{
				throw new Exception("THREAD EXCEPTION: Pause() called while thread not in Started state");
			}

			/**
			 * set the event to break run cycle, then wait for OnRun to 
			 * set the paused event
			 */
			ChangeState(ThreadState.Pausing);

			_shutdownEvent.Set();

			if(nTimeout > 0)
			{
				bRet = _pausedEvent.Wait(nTimeout);
			}
			else
			{
				_pausedEvent.Wait();
			}

			_pausedEvent.Clear();
			return bRet;

		}

		public virtual bool Resume() { return Resume(0); }
		public virtual bool Resume(int nTimeout) { return _Resume(nTimeout); }
		bool _Resume(int nTimeout)
		{
			bool bRet = true;

			if(_state == ThreadState.Paused)
			{
				ChangeState(ThreadState.Resuming);

				_resumeEvent.Set();

				if(nTimeout > 0)
				{
					bRet = _pausedEvent.Wait(nTimeout);
				}
				else
				{
					_pausedEvent.Wait();
				}

				_pausedEvent.Clear();
			}
			return bRet;
		}

		public void Abort(bool niceAbort = false)
		{
			_didNiceAbort = niceAbort;
			if(_state != ThreadState.Stopped)
			{
				_thread.Abort();

				_threadCompleteEvent.Set();
			}
		}

		/**
		 * causes the thread to unblock, by setting the shutdown event, but
		 * leaving it in a Running state
		 */
		public void Unblock()
		{
			_shutdownEvent.Set();
		}

		/**
		 * Enables another thread to wait for this thread to "Join"
		 */
		public bool WaitForCompletion(TimeSpan howLong)
		{
			return WaitForCompletion((int)howLong.TotalMilliseconds);
		}

		public bool WaitForCompletion(int msecs = 0)
		{
			return _threadCompleteEvent.Wait(msecs);
		}

		#endregion

		#region Virtual Methods for Override (called within thread context)

		protected abstract bool OnRun();

		protected virtual bool OnPause()
		{
			return true;
		}

		protected virtual bool OnResume()
		{
			return true;
		}

		virtual protected bool OnStart()
		{
			return true;
		}

		virtual protected bool OnStop()
		{
			return true;
		}

		virtual protected void OnExit()
		{
		}

		virtual protected bool OnException(Exception e)
		{
			return false;           /** false = stop running */
		}


		#endregion

		#region Protected Access Methods (from within thread context)

		protected void SpinFor(TimeSpan timeToWait)
		{
			SpinWait spinWait = new SpinWait();

			int spinCount = 0;
			int yieldCount = 0;
			long ticksToWait = (long)( timeToWait.TotalSeconds * Stopwatch.Frequency );

			Stopwatch stopwatch = Stopwatch.StartNew();
			stopwatch.Start();
			long now = stopwatch.ElapsedTicks;
#if LOG_SPIN
			StringBuilder sb = new StringBuilder();
#endif
			if(ticksToWait > 0)
			{
				do
				{
					if(spinWait.NextSpinWillYield)
					{
						yieldCount++;
					}
					spinWait.SpinOnce();
					spinCount++;
					now = stopwatch.ElapsedTicks;
#if LOG_SPIN
					sb.AppendFormat("({0}, {1}) ", now, ticksToWait - now);
#endif

				} while(now < ticksToWait);
			}

#if LOG_SPIN
			if(spinCount > 0)
			{
				Log.LogText(LogLevel.DEBUG, "Asked to spin for {0} ticks... and it's {1} later we spun for Spun for {2}  Yielded {3} [{4}]", 
					ticksToWait, stopwatch.ElapsedTicks, spinCount, yieldCount, sb.ToString().Trim());
			}
#endif
		}

		protected void Sleep(TimeSpan duration)
		{
			Sleep((int)duration.TotalMilliseconds);
		}

		protected void Sleep(int msecs)
		{
			if(msecs > 0)
			{
				_shutdownEvent.Wait(msecs);
			}
		}

		#endregion

		#region The Thread Itself

		void ThreadMain()
		{
			ChangeState(ThreadState.Starting);

			if(OnStart())
			{
				ChangeState(ThreadState.Started);
				_startupEvent.Set();
				while(_state == ThreadState.Started || _state == ThreadState.Pausing || _state == ThreadState.Resuming)
				{
					try
					{
						if(LogThreadRun)
						{
							LogThreadOnRun();
						}
						StartRunTime = DateTime.UtcNow;
#if MONITOR_PERFORMANCE
						_performanceTimer.Start();
#endif
						bool runResult = OnRun();
#if MONITOR_PERFORMANCE
						_performanceTimer.Stop();
#endif
						if(runResult)
						{
							_runCount++;

							if(_interval > 0 && _state == ThreadState.Started)
							{
								_shutdownEvent.Wait(_interval);
							}
							else if(_interval < 0)
							{
								_shutdownEvent.Wait();
							}
							if(_state == ThreadState.Pausing)
							{
								if(OnPause() == false)
								{
									ChangeState(ThreadState.Stopping);
								}
								else
								{
									ChangeState(ThreadState.Paused);
									_pausedEvent.Set();

									_resumeEvent.Clear();
									_resumeEvent.Wait();

									if(_state == ThreadState.Resuming)
									{
										if(OnResume() == true)
										{
											ChangeState(ThreadState.Started);
											_pausedEvent.Set();
										}
										else
										{
											ChangeState(ThreadState.Stopping);
										}
									}
								}
							}

							if(_state == ThreadState.Stopping)
							{
								OnStop();
							}

							if(ThreadProgress != null)
							{
								ThreadProgress(this, 0, String.Empty);
							}
						}
						else
						{
							ChangeState(ThreadState.Stopping);
							OnStop();
						}
					}
					catch(ThreadAbortException e)
					{
						LastThreadException = e;
						if(_didNiceAbort == false)
						{
							Log.SysLogText(LogLevel.ERROR,
								"**** Thread {0} Aborted\n" +
								"{1}\n",
								Name,
								GetFormattedStackTrace(e));
						}
						ChangeState(ThreadState.Aborted);
						try
						{
							_threadListLock.Lock();
							_startedThreads.Remove(this);
						}
						finally
						{
							_threadListLock.Unlock();
						}
					}
					catch(Exception e)
					{
						LastThreadException = e;
						Log.SysLogText(LogLevel.ERROR,
							"**** Thread::{0} EXCEPTION type {1} THROWN.\n" +
							"                        **** THE TEXT OF THE EXCEPTION FOLLOWS:\n" +
							"{2}\n{3}",
							Name,
							e.GetType(),
							e.Message,
							GetFormattedStackTrace(e));
						if(OnException(e) == false)
						{
							Log.SysLogText(LogLevel.FATAL, "{0} THREAD STOPPING", Name);
							ChangeState(ThreadState.Stopping);
						}
					}
				}
				OnExit();
			}
			else
			{
				ChangeState(ThreadState.Stopped);
				_startupEvent.Set();
			}

			ChangeState(ThreadState.Stopped);

			_threadCompleteEvent.Set();

			try
			{
				_threadListLock.Lock();
				if(_startedThreads.Contains(this))
				{
					_startedThreads.Remove(this);
				}
			}
			finally
			{
				_threadListLock.Unlock();
			}

		}

		#endregion

		#region Private Utility Methods

		void ChangeState(ThreadState state)
		{
			if(_state != state)
			{
				_state = state;
				if(StateChanged != null)
				{
					StateChanged(this, _state);
				}

				if(ThreadComplete != null && ( _state == ThreadState.Stopped || _state == ThreadState.Aborted ))
				{
					ThreadComplete(this);
				}
			}
		}

		void LogThreadOnRun()
		{
			int hashCode = GetHashCode();
			int runCount = 0;
			try
			{
				_threadRunCounterLock.Lock();
				if(_ThreadRunCounter.ContainsKey(hashCode) == false)
				{
					_ThreadRunCounter.Add(hashCode, 0);
				}
				runCount = ++_ThreadRunCounter[hashCode];
			}
			finally
			{
				_threadRunCounterLock.Unlock();
			}

			Log.LogText(LogLevel.DEBUG, "OnRun {0} {1}", GetType().Name, runCount);
		}

		#endregion

		#region Public Utility Methods

		public static List<ThreadBase> GetRunningThreads()
		{
			List<ThreadBase> threads = new List<ThreadBase>();
			try
			{
				_threadListLock.Lock();
				threads = new List<ThreadBase>(_startedThreads);
			}
			finally
			{
				_threadListLock.Unlock();
			}

			return threads;
		}

		public static void DumpPerformanceData()
		{
			try
			{
				_threadListLock.Lock();
				foreach(ThreadBase thread in _startedThreads)
				{
					try
					{
						Log.SysLogText(LogLevel.DEBUG, "Thread {0} {1}", thread.Name, thread.ThreadID);
						thread._performanceTimer.DumpToLog();
					}
					catch(Exception e)
					{
						Log.SysLogText(LogLevel.ERROR, "Error stopping thread {0}: {1}", thread, e.Message);
					}
				}
			}
			finally
			{
				_threadListLock.Unlock();
			}

		}

		public static void StopAllRunningThreads(int timeoutms = 30)
		{
			List<ThreadBase> threads = GetRunningThreads();
			foreach(ThreadBase thread in threads)
			{
				try
				{
					thread.Stop(timeoutms);
				}
				catch(Exception e)
				{
					Log.SysLogText(LogLevel.ERROR, "Error stopping thread {0}: {1}", thread, e.Message);
				}
			}
		}

		public static void AbortAllRunningThreads()
		{
			List<ThreadBase> threads = GetRunningThreads();
			foreach(ThreadBase thread in threads)
			{
				try
				{
					thread.Abort();
				}
				catch(Exception e)
				{
					Log.SysLogText(LogLevel.ERROR, "Error stopping thread {0}: {1}", thread, e.Message);
				}
			}
		}

		public static String GetFormattedStackTrace(Exception e)
		{
			StackTrace stack = new StackTrace(e, true);

			return _GetFormattedStackTrace(stack);
		}

		public static String GetFormattedStackTrace()
		{
			StackTrace stack = new StackTrace();

			return _GetFormattedStackTrace(stack);
		}

		static String _GetFormattedStackTrace(StackTrace stack)
		{
			StringBuilder ret = new StringBuilder();

			/** roll through the stack trace */
			foreach(StackFrame frame in stack.GetFrames())
			{
				String filename = frame.GetFileName();
				if(filename != null)
				{
					filename = Path.GetFileName(filename);
					int lineNumber = frame.GetFileLineNumber();

					ret.AppendFormat("{0}({1}) ", filename, lineNumber);
				}

				MethodBase method = frame.GetMethod();
				ret.AppendFormat("{0}\r\n", method);
			}

			return ret.ToString();
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion

	}
}





