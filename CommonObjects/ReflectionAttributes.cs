using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;

namespace KanoopCommon.CommonObjects
{
	/// <summary>
	/// Base class for attributes containing only a string.
	/// It has additional methods for retrieving string value from objects
	/// </summary>
	public class StringAttribute : Attribute
	{
		#region Public Properties

		protected String _value;
		public String Value { get { return _value; } set { _value = value; } }
		
		#endregion

		#region Constructor

		public StringAttribute(String s) 
		{ 
			_value = s; 
		}

		#endregion

		#region Public Static Methods

		public static String GetAttributeValue(Object it, Type attrType)
		{
			object[] attrs = it.GetType().GetField(it.ToString()).GetCustomAttributes(attrType, false);
			return ((StringAttribute)attrs[0]).Value;
		}

		public static String GetAttributeValue(Object it, Type attrType, String def)
		{
			string result;
			if (!TryGetAttributeValue(it, attrType, out result))
			{
				result = def;
			}

			return result;
		}
		
		public static bool TryGetAttributeValue(Object it, Type attrType, out String retString)
		{
			retString = null;
			if(it is Enum == false)
			{
				Object[] attrs = it.GetType().GetCustomAttributes(attrType, false);
				if(attrs != null && attrs.Length > 0)
				{
					retString = ((StringAttribute)attrs[0]).Value;
				}
			}
			else
			{
				FieldInfo field = it.GetType().GetField(it.ToString());
				if(field != null)
				{
					object[] attrs = field.GetCustomAttributes(attrType, false);
					if(attrs != null && attrs.Length > 0)
					{
						retString = ((StringAttribute)attrs[0]).Value;
					}
				}
			}
			return retString != null;
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			return Value;
		}

		#endregion
	}

	public class ObjectTypeAttribute : Attribute
	{
		protected readonly Type m_Value;
		public Type Value { get { return m_Value; }  }
		public ObjectTypeAttribute(Type s) { this.m_Value = s; }

		public static bool TryGetAttributeValue(Object it, Type attrType, out Type retValue)
		{
			retValue = null;
			object[] attrs = it.GetType().GetField(it.ToString()).GetCustomAttributes(attrType, false);
			if(attrs != null && attrs.Length > 0)
			{
				retValue = ((ObjectTypeAttribute)attrs[0]).Value;
			}
			return retValue != null;
		}
		
	}

	public class IntegerAttribute : Attribute
	{
		protected readonly int m_Value;
		public int Value { get { return m_Value; }  }
		public IntegerAttribute(int value) { this.m_Value = value; }

		public static int GetAttributeValue(Object it, Type attrType)
		{
			object[] attrs = it.GetType().GetField(it.ToString()).GetCustomAttributes(attrType, false);
			return ((IntegerAttribute)attrs[0]).Value;
		}

		public static bool TryGetAttributeValue(Object it, Type attrType, out int value)
		{
			bool found = false;
			value = 0;
			FieldInfo field = it.GetType().GetField(it.ToString());
			if(field != null)
			{
				object[] attrs = field.GetCustomAttributes(attrType, false);
				if(attrs != null && attrs.Length > 0)
				{
					value = ((IntegerAttribute)attrs[0]).Value;
					found = true;
				}
			}
			return found;
		}

	}

	public class UInt32Attribute : Attribute
	{
		public UInt32 Value { get; private set; }
		
		public UInt32Attribute(UInt32 value) { Value = value; }

		public static UInt32 GetAttributeValue(Object it, Type attrType)
		{
			object[] attrs = it.GetType().GetField(it.ToString()).GetCustomAttributes(attrType, false);
			return ((UInt32Attribute)attrs[0]).Value;
		}

		public static bool TryGetAttributeValue(Object it, Type attrType, out UInt32 value)
		{
			bool found = false;
			value = 0;
			FieldInfo field = it.GetType().GetField(it.ToString());
			if(field != null)
			{
				object[] attrs = field.GetCustomAttributes(attrType, false);
				if(attrs != null && attrs.Length > 0)
				{
					value = ((UInt32Attribute)attrs[0]).Value;
					found = true;
				}
			}
			return found;
		}

	}

	public class PointAttribute : Attribute
	{
		protected readonly Point m_Point;
		public Point Value { get { return m_Point; }  }
		public PointAttribute(int x, int y) { this.m_Point = new Point(x, y); }

		public static Point GetAttributeValue(Object it, Type attrType)
		{
			object[] attrs = it.GetType().GetField(it.ToString()).GetCustomAttributes(attrType, false);
			return ((PointAttribute)attrs[0]).Value;
		}
	}

	public class StringToEnum : Dictionary<String, Enum> { }
	public class EnumToString : Dictionary<Enum, String> { }
	public class EnumToType : Dictionary<Enum, Type> { }
	public class TypeToEnum : Dictionary<Type, Enum> { }

	public class EnumStringAttribute : StringAttribute 
	{ 
		public EnumStringAttribute(String val) 
			: base(val) {} 

		public static bool TryGetEnum(Type type, String stringValue, out Enum value)
		{
			bool result = false;
			value = default(Enum);

			foreach(Enum e in Enum.GetValues(type))
			{
				String desc;
				if(TryGetAttributeValue(e, typeof(EnumStringAttribute), out desc) && desc == stringValue)
				{
					value = e;
					result = true;
					break;
				}
			}

			return result;
		}
	}
	public class UriAttribute : StringAttribute { public UriAttribute(String val) : base(val) {} }
	public class TenantAttribute : StringAttribute { public TenantAttribute(String val) : base(val) {} }

}
