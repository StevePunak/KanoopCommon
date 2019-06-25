using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;

namespace KanoopCommon.Logging
{
	public class LogOutputAggregator
	{
		#region Constants

		static readonly TimeSpan CACHE_CHECK_INTERVAL = TimeSpan.FromSeconds(30);

		#endregion

		#region Buffered Output Class

		class BufferedOutput
		{
			String					_text;
			public String Text { get { return _text; } }

			LogLevel			_level;
			public LogLevel Level { get { return _level; } }

			public int Count { get { return _outputList.Count; } }

			List<BufferedLine>		_outputList;
			MutexLock				_listLock;

			DateTime _createTime;
			public DateTime CreateTime { get { return _createTime; } }

			class BufferedLine
			{
				DateTime	m_timestamp;
				public DateTime Timestamp { get { return m_timestamp; } }

				public BufferedLine()
				{
					m_timestamp = DateTime.UtcNow;
				}
			}

			public BufferedOutput(LogLevel level, DateTime createTime, String text)
			{
				_level = level;
				_text = text;
				_createTime = createTime;
				_listLock = new MutexLock();
				_outputList = new List<BufferedLine>();
			}

			public void AddInstance()
			{
				try
				{
					_listLock.Lock();

					_outputList.Add(new BufferedLine());
				}
				finally
				{
					_listLock.Unlock();
				}
			}

			public bool TryGetOutput(TimeSpan outputTime, out int count)
			{
				count = 0;
				bool ret = false;

				try
				{
					_listLock.Lock();

					if(_outputList.Count > 0 && _outputList[0].Timestamp < DateTime.UtcNow - outputTime)
					{
						count = _outputList.Count;
						_outputList.Clear();
						ret = true;
					}
				}
				finally
				{
					_listLock.Unlock();
				}
				return ret;
			}
		}

		#endregion

		#region Private Members

		Dictionary<String, BufferedOutput>		_outputByText;
		MutexLock								_outputListLock;

		Log			_log;
		TimeSpan	_minumumOutputInterval;

		DateTime	_lastCacheCheckTime;

		#endregion

		#region Constructor

		public LogOutputAggregator(Log log, TimeSpan mininumLogInterval)
		{
			_log = log;
			_minumumOutputInterval = mininumLogInterval;
			_lastCacheCheckTime = DateTime.UtcNow;

			_outputByText = new Dictionary<String, BufferedOutput>();
			_outputListLock = new MutexLock();
		}

		#endregion

		#region Public Access Methods

		public void LogText(LogLevel logLevel, String strInText, params object[] parms)
		{
			String output = String.Format(strInText, parms);

			BufferedOutput cached;
			try
			{
				_outputListLock.Lock();
				if(_outputByText.TryGetValue(output, out cached) == false)
				{
					cached = new BufferedOutput(logLevel, DateTime.UtcNow, output);
					_outputByText.Add(output, cached);
				}
				cached.AddInstance();

				int count;
				if(cached.TryGetOutput(_minumumOutputInterval, out count))
				{
					_log.LogText(logLevel, String.Format("{0} - ({1} times)", output, count));
					_outputByText.Remove(output);
				}
			}
			finally
			{
				_outputListLock.Unlock();
			}

			CheckCache();
		}

		public void Flush()
		{
			try
			{
				_outputListLock.Lock();

				foreach(KeyValuePair<String, BufferedOutput> kvp in _outputByText)
				{
					_log.LogText(kvp.Value.Level, String.Format("{0} - ({1} times)", kvp.Key, kvp.Value.Count));
				}
			}
			finally
			{
				_outputByText.Clear();
				_outputListLock.Unlock();
			}
		}

		#endregion

		#region Private Methods

		void CheckCache()
		{
			DateTime now = DateTime.UtcNow;

			if(now > _lastCacheCheckTime + CACHE_CHECK_INTERVAL)
			{
				try
				{
					_outputListLock.Lock();

					List<String> toDelete = new List<String>();
					foreach(BufferedOutput output in _outputByText.Values)
					{
						if(now >= output.CreateTime)
						{
							toDelete.Add(output.Text);
						}
					}

					foreach(String text in toDelete)
					{
						BufferedOutput output;
						if(_outputByText.TryGetValue(text, out output))
						{
							_log.LogText(output.Level, String.Format("{0} - ({1} times) -c-", text, output.Count));
							_outputByText.Remove(text);
						}
					}
				}
				finally
				{
					_outputListLock.Unlock();
				}

				_lastCacheCheckTime = DateTime.UtcNow;
			}
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			return String.Format("Log reducer for {0}", Path.GetFileName(_log.FileName));
		}

		#endregion
	}
}
