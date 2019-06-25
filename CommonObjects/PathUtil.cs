using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KanoopCommon.CommonObjects
{
	/// <summary>
	/// System.IO.Path is a static class and therefore cannot use extension methods.
	/// </summary>
	public class PathUtil
	{
		/// <summary>
		/// Is path a windows network path format
		/// </summary>
		/// <param name="strPath"></param>
		/// <returns></returns>
		public static bool IsUncPath(string strPath)
		{
			return strPath.StartsWith(@"\\");
		}

		/// <summary>
		/// Retrieve host portion of path name
		/// </summary>
		/// <param name="uncPath"></param>
		/// <returns></returns>
		public static string GetHost(string uncPath)
		{
			string []split = uncPath.Split(new char[]{'\\', '/'} ,  StringSplitOptions.RemoveEmptyEntries);

			return split[0];
		}

		/// <summary>
		/// Get users configuration directory
		/// </summary>
		/// <returns></returns>
		public static string GetPersonalConfigurationDirectory()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
		}

		static bool TryGetVersion(String fileName, out SoftwareVersion version)
		{
			version = null;
			int dashIndex = 0;
			String versionPart = Path.GetFileNameWithoutExtension(fileName);
			if((dashIndex = versionPart.IndexOf('-')) != -1)
			{
				versionPart = versionPart.Substring(dashIndex + 1);
				if(SoftwareVersion.IsValid(versionPart))
				{
					version = new SoftwareVersion(versionPart);
				}
			}
			return version != null;
		}

		/// <summary>
		/// Returns any config files 'like' the one given
		/// </summary>
		/// <param name="configFileName"></param>
		/// <returns></returns>
		static List<String> GetConfigFilesLike(string configFileName)
		{
			String path = Path.GetDirectoryName(configFileName);
			if(Directory.Exists(path) == false)
			{
				Directory.CreateDirectory(path);
			}
			List<String> files = new List<String>();
			String filePart = Path.GetFileNameWithoutExtension(configFileName);
			int dashIndex = filePart.IndexOf('-');
			String wildcard = String.Format("{0}*.config", filePart.Substring(0, dashIndex));
			files = new List<String>(Directory.GetFiles(GetPersonalConfigurationDirectory(), wildcard));
			return files;
		}

		/// <summary>
		/// Copy the config most 'like' the given to a new location (ostensibly for upgrade)
		/// </summary>
		/// <param name="configFileName"></param>
		public static void CopyBestConfigLike(String configFileName)
		{
			List<String> files = GetConfigFilesLike(configFileName);
			SortedDictionary<SoftwareVersion, String> index = new SortedDictionary<SoftwareVersion, String>();
			foreach(String file in files)
			{
				SoftwareVersion version;
				if(TryGetVersion(file, out version) == false)
				{
					index.Add(version, file);
				}
			}
			if(index.Count > 0)
			{
				String best = index[index.Max(kvp => kvp.Key)];
				if(best != null)
				{
					File.Copy(best, configFileName);
				}
			}
		}

		/// <summary>
		/// Return path where personal configuration should be stored
		/// </summary>
		/// <param name="configFileName"></param>
		/// <returns></returns>
		public static string GetPersonalConfigurationFilePath(string configFileName)
		{
			String path = Path.Combine(GetPersonalConfigurationDirectory(), configFileName);
			if(File.Exists(path) == false)
			{
				CopyBestConfigLike(path);
			}
			return path;
		}

	}
}
