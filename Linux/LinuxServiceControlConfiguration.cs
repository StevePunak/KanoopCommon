using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Linux
{
	public class LinuxServiceControlConfiguration
	{
		private const string DEFAULT_PLINK_NAME = "plink.exe";
		private const string DEFAULT_PLINK_PATH = @"C:\Program Files\PuTTY\" + DEFAULT_PLINK_NAME;

		private string m_PlinkPath;
		public string PlinkPath
		{
			get
			{
				return m_PlinkPath;
			}
			set
			{
				m_PlinkPath = value;
			}
		}

		public LinuxServiceControlConfiguration()
		{
			m_PlinkPath = GetDefaultPlinkPath();
		}

		public LinuxServiceControlConfiguration(string path)
		{
			m_PlinkPath = path;
		}

		public static string GetDefaultPlinkPath()
		{
			string path = DEFAULT_PLINK_PATH;

			if (Directory.Exists(@"C:\Program Files (x86)"))
			{
				path = path.Replace(@"Files\", @"Files (x86)\");
			}

			return path;
		}
	}
}
