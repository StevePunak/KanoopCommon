using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;
using System.Reflection;
using KanoopCommon.Logging;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using System.Xml;
using KanoopCommon.Conversions;
using KanoopCommon.Extensions;
using KanoopCommon.Addresses;

namespace KanoopCommon.Serialization
{
	public class SerializationXML
	{
		public const String ATTR_FORMAT =					"format";
		public const String ATTR_KEY =						"key";
		public const String ATTR_TYPE =						"type";
		public const String ATTR_ORIGINAL =					"original";

		public const String XML_DICT_ENTRY =				"DICT_ENTRY";
		public const String VAL_BASE_64 =					"base64";
	}

	public abstract class KanoopSerializableObject
	{
		static public Object Deserialize(XmlNode parent)
		{
			Object retObj = null;
			Type objType;
			String strType;

			String strFormat;

			if(	parent.TryGetAttribute(SerializationXML.ATTR_TYPE, out strType) &&
				(objType = GetTypeFromAssembly(strType)) != null)
			{
				/** 
				 * if this object can be deserialized with a string constructor, just use ToString 
				 */
				if(objType.GetCustomAttributes(typeof(ToStringConstructableAttribute), true).Length > 0)
				{
					Type[] parms = new Type[] { typeof(String) };

					ConstructorInfo constructor = objType.GetConstructor(parms);
					if(constructor != null)
					{
						retObj = constructor.Invoke(new object[] { parent.InnerText});
					}
				}
				/**
				 * Deserialize primitives, including strings
				 */
				else if(objType.IsPrimitive || objType.FullName == typeof(String).FullName)
				{
					retObj = Convert.ChangeType(parent.InnerText, objType);
				}
				/**
				 * Deserialize byte arrays
				 */
				else if(objType.FullName == typeof(byte[]).FullName)
				{
					byte[] data = Convert.FromBase64String(parent.InnerText);
					retObj = data;
				}
				else if(objType.FullName == "System.RuntimeType")
				{
					String strActualType;
					if(parent.TryGetAttribute(SerializationXML.ATTR_KEY, out strActualType))
					{
						retObj = GetTypeFromAssembly(strActualType);
					}

				}
				else if(objType.FullName == "System.DateTime")
				{
					Int64 value;
					if(Int64.TryParse(parent.InnerText, out value))
					{
						retObj = new DateTime(value);
					}
				}
				else if(parent.TryGetAttribute(SerializationXML.ATTR_FORMAT, out strFormat) && strFormat == SerializationXML.VAL_BASE_64)
				{
					byte[] data = Convert.FromBase64String(parent.InnerText);
					MemoryStream ms = new MemoryStream(data);
					IFormatter formatter = new BinaryFormatter();
					retObj = formatter.Deserialize(ms);
					ms.Close();
				}
				else
				{
					/** call the empty constructor */
					ConstructorInfo constructor = objType.GetConstructor(new Type[]{});
					if(constructor == null)
					{
						throw new Exception(String.Format("Object '{0}' must have an empty constructor", objType.Name));
					}
					retObj = constructor.Invoke(new object[]{});

					/**
					 * For generics, we only deserialize Dictionaries and Lists
					 */
					if(IsDictionary(retObj))
					{
						IDictionary dictionary = retObj as IDictionary;
						foreach(XmlNode itemNode in parent.ChildNodes)
						{
							String keyType;
							String keyVal;
							Type type;

							if(	itemNode.TryGetAttribute(SerializationXML.ATTR_TYPE, out keyType) &&
								itemNode.TryGetAttribute(SerializationXML.ATTR_KEY, out keyVal) &&
								(type = GetTypeFromAssembly(keyType)) != null)
							{
								Object dictObj = Deserialize(itemNode.FirstChild);
								if(dictObj != null)
								{
									try
									{
										Object key = null;
										/** allow keys of type 'Type' */
										if(keyType == "System.RuntimeType")
										{
											key = GetTypeFromAssembly(keyVal);
										}
										else if(type.IsEnum)
										{
											key = Enum.Parse(type, keyVal);
										}
										else
										{
											TypeConverter converter = TypeDescriptor.GetConverter(type);
											if(converter != null)
											{
												key = converter.ConvertFrom(keyVal);
											}
											else
											{
												key = Convert.ChangeType(keyVal, type);
											}
										}
										if(key != null)
										{
											dictionary.Add(key, dictObj);
										}
									}
									catch(Exception) {}
								}
							}
						}
					}
					else if(IsList(retObj))
					{
						IList list = retObj as IList;
						foreach(XmlNode node in parent.ChildNodes)
						{
							Object listObj = Deserialize(node);
							if(listObj != null)
							{
								list.Add(listObj);
							}
						}
					}
					else
					{
						/** Recursively deserialize all the classes properties */
						foreach(XmlNode propertyNode in parent.ChildNodes)
						{
							String strPropertyName;
							String strPropertyType;
//							Type propertyType;
							
							/** allow the xml override */
							if(!propertyNode.TryGetAttribute(SerializationXML.ATTR_ORIGINAL, out strPropertyName))
							{
								strPropertyName = propertyNode.Name;
							}
							/** get the property */
							PropertyInfo property = objType.GetProperty(strPropertyName);
							if(	property != null &&
								property.CanWrite &&
								propertyNode.TryGetAttribute(SerializationXML.ATTR_TYPE, out strPropertyType) &&
								GetTypeFromAssembly(strPropertyType) != null)
							{
								Object propertyValue = Deserialize(propertyNode);
								if(propertyValue != null && property.GetCustomAttributes(typeof(IgnoreForSerializationAttribute), true).Length == 0)
								{
									try
									{
										property.SetValue(retObj, propertyValue, null);
									}
									catch(Exception e)
									{
										Log.SysLogText(LogLevel.WARNING, "Deserialization error: {0}", e.Message);
									}
								}
							}

						}
					}
				}
			}
			return retObj;
		}
	
		static public XmlNode Serialize(Object obj, XmlNode parent)
		{
			return Serialize(obj, parent, null);
		}
		static XmlNode Serialize(Object obj, XmlNode parent, String propertyName)
		{
			/** default to failure */
			bool serializeThisNode = false;
			Type objType = obj.GetType();

			/** 
			 * see if this node is explicitly named... if so, we will always use the
			 * explicit name, otherwise, we will attempt to use the passed in name 
			 * if there is one.
			 */
			bool explicitName = false;
			Object[] attrs = objType.GetCustomAttributes(typeof(XmlNodeNameAttribute), true);
			String nodeName = objType.Name;
			if(attrs.Length > 0)
			{
				nodeName = ((StringAttribute)attrs[0]).Value;
				explicitName = true;
			}
			else if(propertyName != null)
			{
				nodeName = propertyName;
			}

			/** create the return node */
			XmlNode retNode = parent.OwnerDocument.CreateElement(nodeName);
			parent.AppendChild(retNode);
			retNode.AddAttribute(SerializationXML.ATTR_TYPE, objType.FullName);

			/** if explicitly named, add an additional attribute */
			if(explicitName)
			{
				retNode.AddAttribute(SerializationXML.ATTR_ORIGINAL, (propertyName != null) ? propertyName : objType.FullName);
			}

			/** 
			 * see if this is an explicitly serializable object e.g. has IsSerializableObject attribute.
			 * if not, we will only serialize primitives, strings and byte arrays
			 */
			if(objType.GetCustomAttributes(typeof(IsSerializableAttribute), true).Length > 0)
			{
				serializeThisNode = true;

				/** 
				 * if this object can be deserialized later with a string constructor, just use ToString 
				 */
				if(objType.GetCustomAttributes(typeof(ToStringConstructableAttribute), true).Length > 0)
				{
					retNode.InnerText = obj.ToString();
				}
				else
				{
					/** Recursively serialize all the classes properties */
					PropertyInfo[] properties = objType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
					foreach(PropertyInfo property in properties)
					{
						/** do not serialize ignored properties */
						if(property.GetCustomAttributes(typeof(IgnoreForSerializationAttribute), true).Length == 0)
						{
							Object propertyValue = property.GetValue(obj, null);
//							Type propertyType = property.PropertyType;
							if(propertyValue == null)
							{
								ConstructorInfo constructor = property.PropertyType.GetConstructor(new Type[] {} );
								if(constructor != null)
								{
									propertyValue = constructor.Invoke(new object[]{} );
								}
							}

							if(propertyValue != null)
							{
								Serialize(propertyValue, retNode, property.Name);
							}
						}
					}
				}
			}
			/** if a primitive or string type, we will serialize it here */
			else if(objType.IsPrimitive || objType.FullName == "System.String")
			{
				retNode.InnerText = obj.ToString();
				serializeThisNode = true;
			}
			/** if it's a byte array, base 64 it */
			else if(objType.FullName == typeof(byte[]).FullName)
			{
				retNode.InnerText = Convert.ToBase64String((byte[])obj, 0, ((byte[])obj).Length);
				serializeThisNode = true;
			}
			/** Serialize DateTimes */
			else if(objType.FullName == typeof(DateTime).FullName)
			{
				retNode.InnerText = ((DateTime)obj).Ticks.ToString();
				serializeThisNode = true;
			}
			/** Serialize Runtime Data Types */
			else if(objType.FullName == "System.RuntimeType")
			{
				retNode.AddAttribute(SerializationXML.ATTR_KEY, obj.ToString());
				serializeThisNode = true;
			}
			/**
			 * For generics, we only serialize Dictionaries and Lists
			 */
			else if(IsDictionary(obj))
			{
				IDictionary dictionary = obj as IDictionary;
				foreach(Object key in dictionary.Keys)
				{										
					String strEntry = SerializationXML.XML_DICT_ENTRY + "_" + nodeName;
					XmlElement subNode = ((XmlElement)retNode).AddSubNode(strEntry);
						
					subNode.AddAttribute(SerializationXML.ATTR_KEY, key.ToString());
					subNode.AddAttribute(SerializationXML.ATTR_TYPE, key.GetType().FullName);
					Serialize(dictionary[key], subNode);
				}
				serializeThisNode = true;
			}
			else if(IsList(obj))
			{
				foreach(Object element in (obj as IEnumerable))
				{
					Serialize(element, retNode);
				}
				serializeThisNode = true;
			}
			else if (IsSet(obj))
			{
				foreach (Object element in (obj as IEnumerable))
				{
					Serialize(element, retNode);
				}
				serializeThisNode = true;
			}

			/** other serializable object */
			else if (objType.IsSerializable)
			{
				/** serialize into a byte array, and store it as base64 */
				MemoryStream ms = new MemoryStream();
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ms, obj);
				byte[] data = ms.ToArray();
				retNode.InnerText = Convert.ToBase64String(data, 0, data.Length);
				ms.Close();

				retNode.AddAttribute(SerializationXML.ATTR_FORMAT, SerializationXML.VAL_BASE_64);

				serializeThisNode = true;
			}
			else
			{
				/** empty contents */
				//        bSerializeThisNode = true;
			}
			if(serializeThisNode && parent != null)
			{
				parent.AppendChild(retNode);
			}

			return retNode;
		}

		public static bool IsList(object obj)
		{
			System.Collections.IList list = obj as System.Collections.IList;
			return list != null;
		}

		public static bool IsCollection(object obj)
		{
			System.Collections.ICollection coll = obj as System.Collections.ICollection;
			return coll != null;
		}

		public static bool IsDictionary(object obj)
		{
			System.Collections.IDictionary dictionary = obj as System.Collections.IDictionary;
			return dictionary != null;
		}

		public static bool IsSet(object obj)
		{
			return obj is HashSet<IPv4AddressPort>;
		}

		public static Type GetTypeFromAssembly(String type)
		{
			Type ret = null;

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach(Assembly assembly in assemblies)
			{
				if((ret = assembly.GetType(type)) != null)
				{
					break;
				}
			}
			return ret;
		}
	}
}
