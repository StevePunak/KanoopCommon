using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Serialization;
using KanoopCommon.CommonObjects;
using System.Reflection;

namespace KanoopCommon.PersistentConfiguration
{
	[IsSerializable]
	public class ProgramConfiguration
	{
		ConfigFile	_configFile;
		[IgnoreForSerialization()]
		public ConfigFile ConfigFile 
		{ 
			get { return _configFile; } 
			set { _configFile = value; } 
		}

		public void Save()
		{
			if(_configFile != null)
			{
				_configFile.Save();
			}
		}

		public static String GetDefaultConfigFileName(Type type)
		{
			SoftwareVersion version = new SoftwareVersion(Assembly.GetEntryAssembly().GetName().Version);
			String fileName = String.Format("{0}_{1}_{2}.config", type.Name, version.Major, version.Minor);
			return PathUtil.GetPersonalConfigurationFilePath(fileName);
		}

		public virtual void ConfigLoadComplete(){}
	}

}
