using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;
using System.IO;

namespace KanoopCommon.Logging
{
	class AsynchLogThread : ThreadBase
	{
		List<Object>		_logLines;
		MutexLock			_logLineListLock;

		TextWriter			_fileTextWriter;
		BinaryWriter		_binaryWriter;

		public AsynchLogThread(TextWriter tw, BinaryWriter bw)
			: base("AsynchLogThread") 
		{
			_fileTextWriter = tw;
			_binaryWriter = bw;

			_logLines = new List<Object>();
			_logLineListLock = new MutexLock();
		}

		public void AddLogLine(String strLine)
		{
			_logLineListLock.Lock();
			_logLines.Add(strLine);
			_logLineListLock.Unlock();
		}

		public void AddBinaryData(String strLabel, byte[] data, int offset, int count)
		{
			byte[] buffer = new byte[count];
			Array.Copy(data, offset, buffer, 0, count);
			BinaryLogEntry entry = new BinaryLogEntry(strLabel, buffer);

			_logLineListLock.Lock();
			_logLines.Add(entry);
			_logLineListLock.Unlock();
		}

		protected override bool OnRun()
		{
			Interval = TimeSpan.FromSeconds(10);
			_logLineListLock.Lock();
			foreach(Object o in _logLines)
			{
				if(o is String)
				{
					_fileTextWriter.WriteLine(o as String);
					_fileTextWriter.Flush();
				}
				else if(o is BinaryLogEntry)
				{
					BinaryLogEntry entry = o as BinaryLogEntry;
					String strOutput = String.Format("[{0} b:{1}/{2}]\n", DateTime.Now.ToString("HH:mm:ss.fff"), entry.Data.Length, entry.Label);
					_binaryWriter.Write(ASCIIEncoding.UTF8.GetBytes(strOutput));
					_binaryWriter.Write(entry.Data);
					_binaryWriter.Write('\x0a');
					_binaryWriter.Flush();
				}
			}
			_logLines.Clear();
			_logLineListLock.Unlock();

			return true;
		}

	}
}
