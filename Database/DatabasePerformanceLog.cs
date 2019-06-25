using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KanoopCommon.Threading;

namespace KanoopCommon.Database
{
	public class DatabasePerformanceLog : CSVFile
	{
		TextWriter		m_TextWriter;

		MutexLock		m_Lock;

		public DatabasePerformanceLog(String fileName)
			: base(fileName)
		{
			m_Lock = new MutexLock();

			m_TextWriter = File.CreateText(fileName);
			m_TextWriter.WriteLine("Timestamp,Type,Elapsed,Method,Sql");
			m_TextWriter.Flush();
		}

		public void LogText(String format, params object[] args)
		{
			try
			{
				m_Lock.Lock();

				String text = args == null || args.Length == 0 ? format : String.Format(format, args);
				m_TextWriter.WriteLine(text);
				m_TextWriter.Flush();
			}
			catch(Exception)
			{
			}
			finally
			{
				m_Lock.Unlock();
			}
		}
	}
}
