using System;
using System.Collections.Generic;
using System.Text;

using System.Diagnostics;
using KanoopCommon.Logging;

namespace KanoopCommon.Performance
{
	public class PerformanceTimer
	{
		Stopwatch		_stopwatch;

		long			_totalTicks;

		long _callCount;
		public long CallCount { get { return _callCount; } }

		String			_name;
		public String Name { get { return _name; } }

		Double			_longestCalculatedAverage;
		public Double LongestCalculatedAverage { get { return _longestCalculatedAverage; } }

		Double			_longest;
		public Double Longest 
		{ 
			get 
			{ 
				return (Double)((double)_longest / (double)Stopwatch.Frequency) * 1000;
			} 
		}

		Double			_shortest;
		public Double Shortest 
		{ 
			get 
			{ 
				return (Double)((double)_shortest / (double)Stopwatch.Frequency) * 1000; 
			} 
		}

		public Double Average
		{
			get 
			{ 
				Double average = (Double)((double)_totalTicks / (double)Stopwatch.Frequency) / _callCount * 1000; 
				if(average > _longestCalculatedAverage)
					_longestCalculatedAverage = average;
				return average;
			}
		}

		public void Reset()
		{
			_stopwatch.Reset();
			_callCount = 0;
			_totalTicks = 0;
			_longestCalculatedAverage = 0;
			_longest = 0;
			_shortest = Double.NaN;
		}

		public Double ElapsedMilliseconds
		{
			get { return (Double)((double)_stopwatch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000; }
		}

		public long ElapsedTicks
		{
			get { return _stopwatch.ElapsedTicks; }
		}

		public PerformanceTimer(Object type)
			: this(type.ToString()) {}

		public PerformanceTimer(String name)
		{
			_stopwatch = new Stopwatch();
			_name = name;
			Reset();
		}

		public virtual void Start()
		{
			_stopwatch.Reset();
			_stopwatch.Start();
		}

		public virtual void Stop()
		{
			Double longest = _longest;
			Double shortest = _shortest;


			_stopwatch.Stop();
			_totalTicks += _stopwatch.ElapsedTicks;
			_callCount++;
			if(_stopwatch.ElapsedTicks > longest)
			{
				longest = _stopwatch.ElapsedTicks;
			}
			if((Double.IsNaN(shortest) || _stopwatch.ElapsedTicks < shortest))
			{
				shortest = _stopwatch.ElapsedTicks;
			}
			_longest = longest;
			_shortest = shortest;
		}

		public virtual void StopAndLog()
		{
			Stop(1, Log.SystemLog, LogLevel.DEBUG);
		}

		public virtual void Stop(long logFrequency, Log log, LogLevel level)
		{

			Stop();

			if ((_callCount % logFrequency) == 0)
			{
				DumpToLog(log, level);
				Reset();
			}

		}

		public void DumpToLog()
		{
			DumpToLog(Log.SystemLog, LogLevel.DEBUG);
		}

		public void DumpToLog(Log log, LogLevel level)
		{
			log.LogText(level, "{0}", this.ToString());
		}

		public override string ToString()
		{
			return String.Format("Timer {0} - Average: {1:0.0000}ms  Longest: {2:0.0000}ms  Shortest: {3:0.0000}ms  Call Count: {4}",
				_name, Average, Longest, Shortest, CallCount);
		}
	}
}
