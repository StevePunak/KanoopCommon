using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Extensions
{
	public static class DirectoryExtensions
	{
		public static String GetNextNumberedFileName(String directory, String prefix, String extension, int numberOfZeros = 4)
		{
			String ret = String.Empty;
			int max = numberOfZeros * 10;
			for(int x = 1;x < 1000;x++)
			{
				String format = x.ToString().PadLeft(numberOfZeros, '0');
				String filename = String.Format("{0}{1}{2}", prefix, format, extension);
				if(File.Exists(Path.Combine(directory, filename)) == false)
				{
					ret = Path.Combine(directory, filename);
					break;
				}
			}
			return ret;
		}

		public static String GetParent(String originalDirectory, int levelsBack = 1)
		{
			String returnValue = originalDirectory;
			while(levelsBack > 0)
			{
				returnValue = Directory.GetParent(returnValue).FullName;
				--levelsBack;
			}
			return returnValue;
		}

		public static void CopyRecursive(string sourceDirectory, string targetDirectory)
		{
			DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
			DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

			CopyAll(diSource, diTarget);
		}

		private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory(target.FullName);

			// Copy each file into the new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
//				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}
