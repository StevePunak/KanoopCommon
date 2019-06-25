using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Serialization;
using System.IO;
using System.Reflection;
using KanoopCommon.CommonObjects;
using System.Diagnostics;
using System.Xml;
using KanoopCommon.Extensions;

namespace KanoopCommon.PersistentConfiguration
{
	public class ConfigFile
	{
		#region Constants

		const String DEFAULT_PRIMARY_SECTION = "KANOOP_CONFIG";

		#endregion

		#region Public Properties

		String _fileName;
		public String FileName
		{
			get
			{
				if (_fileName == null)
				{
					_fileName = String.Empty;
				}
				return _fileName;
			}
			set { _fileName = value; }
		}

		String _rootSectionName;
		public String RootSectionName { get { return _rootSectionName; } }

		Dictionary<String, ProgramConfiguration> _configurations;
		public Dictionary<String, ProgramConfiguration> Configurations
		{
			get
			{
				if (_configurations == null)
				{
					_configurations = new Dictionary<String, ProgramConfiguration>();
				}
				return _configurations;
			}
		}

		#endregion

		#region Constructor

		public ConfigFile(String strFileName)
			: this(strFileName, DEFAULT_PRIMARY_SECTION) { }

		ConfigFile(String strFileName, String rootSectionName)
		{
			_fileName = strFileName;
			_rootSectionName = rootSectionName;

			if (!File.Exists(_fileName))
			{
				if (Path.GetDirectoryName(strFileName).Length > 0 && !Directory.Exists(Path.GetDirectoryName(strFileName)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(strFileName));
				}

				XmlDocument doc = new XmlDocument();
				XmlNode rootNode = doc.CreateElement(rootSectionName);
				doc.AppendChild(rootNode);
				doc.Save(_fileName);
			}

			LoadProgramConfigurations();
		}

		#endregion

		#region Private Methods

		void LoadProgramConfigurations()
		{
			Configurations.Clear();

			/** load the XML file */
			XmlDocument doc = new XmlDocument();
			doc.Load(_fileName);

			/** load each program confguration within it */
			foreach (XmlNode node in doc.DocumentElement.ChildNodes)
			{
				String configTypeName;
				Type configurationType;
				if (node.Attributes != null && node.TryGetAttribute(SerializationXML.ATTR_TYPE, out configTypeName) &&
					(configurationType = KanoopSerializableObject.GetTypeFromAssembly(configTypeName)) != null &&
					configurationType.IsSubclassOf(typeof(ProgramConfiguration)))
				{
					Object config = KanoopSerializableObject.Deserialize(node);
					((ProgramConfiguration)config).ConfigFile = this;
					Configurations.Add(configurationType.FullName, (ProgramConfiguration)config);
					((ProgramConfiguration)config).ConfigLoadComplete();
				}
			}
		}

		List<XmlNode> GetUnknownSections()
		{
			List<XmlNode> unknownSections = new List<XmlNode>();

			/** load the XML file */
			XmlDocument doc = new XmlDocument();
			doc.Load(_fileName);

			/** save off any sections we can't resolve in this assembly */
			foreach (XmlNode node in doc.DocumentElement.ChildNodes)
			{
				String configType;
				if (node.TryGetAttribute(SerializationXML.ATTR_TYPE, out configType) &&
					KanoopSerializableObject.GetTypeFromAssembly(configType) == null)
				{
					unknownSections.Add(node);
				}
			}
			return unknownSections;
		}

		public void Save()
		{
			List<XmlNode> unknownSections = GetUnknownSections();

			XmlDocument doc = new XmlDocument();
			XmlNode rootNode = doc.CreateElement(_rootSectionName);
			doc.AppendChild(rootNode);
			doc.Save(_fileName);

			foreach(KeyValuePair<String, ProgramConfiguration> kvp in _configurations)
			{
//				XmlNode sectionNode = doc.AddSubNode(kvp.Key);
				XmlNode subNode = KanoopSerializableObject.Serialize(kvp.Value, doc.DocumentElement);
//				sectionNode.AddSubNode(subNode);
//				doc.AddSubNode(subNode);
			}

			foreach (XmlNode node in unknownSections)
			{
				doc.AddSubNode(node);
			}

			doc.Save(_fileName);
		}

		static Dictionary<SoftwareVersion, String> GetVersions(List<String> fileNames)
		{
			Dictionary<SoftwareVersion, String> values = new Dictionary<SoftwareVersion, String>();

			foreach (String fileName in fileNames)
			{
				int index1 = Path.GetFileName(fileName).LastIndexOf('-');
				int index2 = Path.GetFileName(fileName).LastIndexOf('.');
				if (index1 > 0 && index2 > index1)
				{
					index1++;
					String versionString = Path.GetFileName(fileName).Substring(index1, index2 - index1);
					SoftwareVersion version = new SoftwareVersion(versionString);
					values.Add(version, fileName);
				}

			}


			return values;
		}

		#endregion

		#region Public Access Methods

		public ProgramConfiguration GetConfiguration(Type configurationType)
		{
			ProgramConfiguration ret = null;
			if (!Configurations.TryGetValue(configurationType.FullName, out ret))
			{
				ConstructorInfo constructor = configurationType.GetConstructor(new Type[] { });
				if (constructor == null)
				{
					throw new Exception("The program configuration MUST have an empty constructor");
				}
				Object config = constructor.Invoke(new object[] { });
				ret = (ProgramConfiguration)config;
				((ProgramConfiguration)config).ConfigFile = this;
				_configurations.Add(configurationType.FullName, ret);
				Save();
			}
			return ret;
		}

		public static ConfigFile OpenConfigFile(String baseFileName = null)
		{
			String fileName = GetDefaultConfigFileName();

			if (Directory.Exists(Path.GetDirectoryName(fileName)))
			{
				Directory.SetCurrentDirectory(Path.GetDirectoryName(fileName));
				if (File.Exists(fileName) == false && ConfigFile.TryCopyPreviousVersion(fileName))
				{
					/** copied new config file... don't do anything */
				}
			}
			else
				Directory.CreateDirectory(Path.GetDirectoryName(fileName));

			ConfigFile configFile = new ConfigFile(fileName);
			return configFile;

		}

		public static bool TryCopyPreviousVersion(String newFileName)
		{
			bool result = false;

			int index = Path.GetFileName(newFileName).LastIndexOf('-');
			if (index > 0)
			{
				String path = Path.GetDirectoryName(newFileName);
				String wildcard = String.Format("{0}*.config", Path.GetFileName(newFileName).Substring(0, index));
				List<String> files = new List<String>(Directory.GetFiles(path, wildcard));

				Dictionary<SoftwareVersion, String> indexedFiles = GetVersions(files);
				if (indexedFiles.Count > 0)
				{
					List<SoftwareVersion> versions = new List<SoftwareVersion>(indexedFiles.Keys);
					versions.Sort(new SoftwareVersion.VersionSorter());
					versions.Reverse();		/** get highest version already there */
					String fromFile = indexedFiles[versions[0]];
					File.Copy(fromFile, newFileName);
					result = true;
				}
			}


			return result;
		}

		public static String GetDefaultConfigFileName()
		{
			SoftwareVersion version = new SoftwareVersion(Assembly.GetEntryAssembly().GetName().Version);
			String name = Assembly.GetEntryAssembly().GetName().Name;
			String fileName = String.Format("{0}-{1}.{2}.config", name, version.Major, version.Minor);
			return PathUtil.GetPersonalConfigurationFilePath(fileName);
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			return Path.GetFileName(_fileName);
		}

		#endregion
	}
}
