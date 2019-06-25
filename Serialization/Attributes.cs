using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using KanoopCommon.CommonObjects;
using System.Reflection;

namespace KanoopCommon.Serialization
{

	/// <summary>
	/// Defines this as a class that can be serialized and deserialized
	/// </summary>
	public class IsSerializableAttribute : Attribute {}

	/// <summary>
	/// Specifies that this class has a constructor which will can recreate the class from the output of ToString
	/// </summary>
	public class ToStringConstructableAttribute : Attribute {}

	/// <summary>
	/// Specifies that the property not be serialized
	/// </summary>
	public class IgnoreForSerializationAttribute : Attribute {}


	/// <summary>
	/// Use this to override the class name as the XML node name
	/// </summary>
	public class XmlNodeNameAttribute : StringAttribute 
	{ 
		public bool AutoSerializeSubTypes { get; private set; }

		public BindingFlags BindingFlags { get; private set; }

		public XmlNodeNameAttribute(String value) 
			: this(value, false) { }

		public XmlNodeNameAttribute(String value, bool autoSerializeSubTypes)
			: this(value, autoSerializeSubTypes, BindingFlags.Public | BindingFlags.Instance) {}

		public XmlNodeNameAttribute(String value, bool autoSerializeSubTypes, BindingFlags bindingFlags)
			: base(value)
		{
			AutoSerializeSubTypes = autoSerializeSubTypes;
			BindingFlags = bindingFlags;
		}
	}
}
