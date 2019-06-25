using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KanoopCommon.CommonObjects;
using KanoopCommon.Conversions;

namespace KanoopCommon.PersistentConfiguration
{

	public class IniFileIndex : List<IniFileSection>
	{
		public IniFileIndex()
			: base() {}

		public IniFileIndex(IEnumerable<IniFileSection> other)
			: base(other) { }

		#region Public Access Methods

		public bool TryGetSection(String name, out IniFileSection section)
		{
			section = this.Find(s => s.Name == name);
			return section != null;
		}

		public bool TryGetSections(String name, out IniFileSectionList sections)
		{
			sections = new IniFileSectionList(this.FindAll(s => s.Name == name));
			return sections != null && sections.Count > 0; 
		}


		#endregion
	}

	public abstract class IniFile
	{
		#region Internal Classes

		class Argument
		{
			public String Section { get; private set; }

			public String Key { get; private set; }

			public Type Type { get; private set; }

			public String PropertyName { get; private set; }

			public bool Mandatory { get; private set; }

			public Object DefaultValue { get; set; }

			public Object Value { get; set; }

			public Argument(String section, String key, Type type, String propertyName, Object defaultValue, bool mandatory)
			{
				Section = section;
				Key = key;
				Type = type;
				PropertyName = propertyName;
				DefaultValue = defaultValue;
				Mandatory = mandatory;
			}

			public override string ToString()
			{
				return String.Format("Arg: {0}", Key);
			}
		}

		#endregion

		#region Public Properties

		public String FileName { get; private set; }

		#endregion

		#region Private Member Variables

		IniFileIndex _elements;
		int _lineNumber;

		#endregion

		#region Constructor

		protected IniFile(String fileName)
		{
			FileName = fileName;
			_elements = new IniFileIndex();
			ParseFile();
		}


		#endregion

		#region Public Access Methods

		public bool TryGetSection(String name, out IniFileSection section)
		{
			return _elements.TryGetSection(name, out section);
		}

		public bool TryGetSections(String name, out IniFileSectionList sections)
		{
			return _elements.TryGetSections(name, out sections);
		}

		#endregion

		#region Protected Access Methods

		protected void PopulateProperty(String section, String key, String propertyName, Object defaultValue)
		{
			PopulateProperty(section, key, propertyName, defaultValue, false);
		}

		protected void PopulateProperty(String section, String key, String propertyName)
		{
			PopulateProperty(section, key, propertyName, null, true);
		}

		void PopulateProperty(String section, String key, String propertyName, Object defaultValue, bool mandatory)
		{
			Argument argument = new Argument(section, key, GetPropertyType(propertyName), propertyName, defaultValue, mandatory);
			SetProperty(argument);
		}

		#endregion

		#region Setting of Properties

		Type GetPropertyType(String name)
		{
			Type type = default(Type);
			PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach(PropertyInfo property in properties)
			{
				if(property.Name == name)
				{
					type = property.PropertyType;
					break;
				}
			}

			if(type == null)
			{
				throw new IniFileException("Invalid property '{0}' specified in ini file object", name);
			}
			return type;
		}

		void SetProperty(Argument argument)
		{
			PropertyInfo property = GetType().GetProperty(argument.PropertyName);
			if(property == null)
			{
				throw new IniFileException(String.Format("Property '{0}' does not exist", argument.PropertyName));
			}

			IniFileSection section;
			if(_elements.TryGetSection(argument.Section, out section) == false)
			{
				section = null;
			}

			String value = null;
			if(argument.Mandatory)
			{
				if(section == null)
				{
					throw new IniFileException("Section '{0}' does not exist", argument.Section);
				}

				if(section.Elements.TryGetValue(argument.Key, out value) == false)
				{
					throw new IniFileException("Missing mandatory key '{0}'", argument.Key);
				}
	
				if(property.PropertyType.IsGenericType)
				{
					List<String> values = new List<String>(value.Split(','));
					StringToProperty.SetProperty(this, property, values);
				}
				else
				{
					StringToProperty.SetProperty(this, property, value);
				}
		
			}
			else if(section == null)
			{
				throw new IniFileException("Section '{0}' does not exist", argument.Section);
			}
			else
			{
				List<String> values;
				if(section.Elements.TryGetValue(argument.Key, out value) == false)
				{
					if(argument.DefaultValue != null)
					{
						StringToProperty.SetProperty(this, property, argument.DefaultValue.ToString());
					}
					else
					{
						property.SetValue(this, null, null);
					}
				}
				else
				{
					if(property.PropertyType.IsGenericType)
					{
						values = new List<String>(value.Split(','));
						StringToProperty.SetProperty(this, property, values);
					}
					else
					{
						StringToProperty.SetProperty(this, property, value);
					}
				}
			}
		}

		#endregion

		#region Private Methods

		void ParseFile()
		{
			_lineNumber = 0;
			String sectionName = null;
			_elements = new IniFileIndex();

			IniFileSection currentSection = null;
			foreach(String l in File.ReadAllLines(FileName))
			{
				_lineNumber++;

				String line = l.Trim();
				if(line.Length == 0)
					continue;

				String key, value;

				if(line.StartsWith("["))
				{
					sectionName = GetSectionName(line);
					currentSection = new IniFileSection(sectionName, _lineNumber);
					_elements.Add(currentSection);
				}
				else if(TryGetElement(line, out key, out value))
				{
					if(currentSection == null)
					{
						throw new IniFileException("Element found outside of a section on line {0}", _lineNumber);
					}

					if(currentSection.Elements.ContainsKey(key))
					{
						throw new IniFileException("Duplicate key found on line {0}", _lineNumber);
					}

					currentSection.Elements.Add(key, value);
				}
			}
		}

		String GetSectionName(String line)
		{
			int index = line.IndexOf(']');
			if(index < 2)
			{
				throw new IniFileException("Invalid section header on line {0}", _lineNumber);
			}
			String sectionName = line.Substring(1, index - 1);
			return sectionName;
		}

		bool TryGetElement(String line, out String key, out String value)
		{
			key = value = null;

			if(line.StartsWith("#") == false)
			{
				int index = line.IndexOf('=');
				if(index <= 0)
				{
					throw new IniFileException("Parsing error on line {0}", _lineNumber);
				}

				key = line.Substring(0, index);
				if(line.Length == index)
				{
					value = String.Empty;
				}
				else
				{
					value = line.Substring(index + 1);
				}
			}

			return value != null;
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			return FileName;
		}

		#endregion


	}
}
