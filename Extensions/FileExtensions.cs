using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KanoopCommon.Extensions
{
	public static class FileExtensions
	{
		/// <summary>
		/// Similar to File.ReadAllLines, but ensures that the handle is closed and marked for disposal
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static List<String> SafelyReadLines(String fileName)
		{
			List<String> lines = new List<String>();
			using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			using(TextReader ts = new StreamReader(fs))
			{
				String line;
				while((line = ts.ReadLine()) != null)
				{
					lines.Add(line);
				}
				ts.Close();
				fs.Close();
			}
			return lines;
		}

		public class FileInfoSizeSorter : IComparer<FileInfo>
		{
			public int Compare(FileInfo x, FileInfo y)
			{
				return x.Length.CompareTo(y.Length);
			}
		}


	}
}
