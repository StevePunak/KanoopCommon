using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KanoopCommon.Logging
{
	public class MemoryLog
	{
		const int DEFAULT_MAX_LINES = 10000;

		List<String>		m_Lines;
		int					m_nMaxLines;

		public MemoryLog()
			: this(DEFAULT_MAX_LINES) {}

		public MemoryLog(int maxLines)
		{
			m_Lines = new List<String>();
			m_nMaxLines = maxLines;
		}

		public void WriteLine(String line)
		{
			m_Lines.Add(line);
			while(m_Lines.Count > m_nMaxLines)
				m_Lines.RemoveAt(0);
		}

		public void WriteToFile(String fileName)
		{
			File.WriteAllLines(fileName, m_Lines.ToArray());
		}

		public void Stop()
		{
			m_Lines.Clear();
		}

	}
}
