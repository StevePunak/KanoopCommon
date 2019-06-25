using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KanoopCommon.CommonObjects;
using KanoopCommon.Extensions;
using KanoopCommon.Logging;
using System.Runtime.InteropServices;

namespace KanoopCommon.Reflection
{
	public class ReflectionUtilities
	{
		public static Object GetAttribute(Type enumType, Type attributeType, Enum value)
		{
			Object ret = null;
			foreach(Enum e in Enum.GetValues(enumType))
			{
				if(e.ToString() == value.ToString())
				{
					MemberInfo member = enumType.GetMember(e.ToString())[0];
					ret = member.GetCustomAttribute(attributeType);
				}
			}

			return ret;
		}

		public static int SizeofWriteableProperties(Type type)
		{
			int total = 0;

			foreach(PropertyInfo property in type.GetProperties())
			{
				if(property.GetSetMethod() != null)
				{
					total += Marshal.SizeOf(property.PropertyType);

				}
			}
			return total;
		}

		public static void SetProperty(Object thing, String propertyName, String value)
		{
			PropertyInfo property = thing.GetType().GetProperty(propertyName);
			if(property != null)
			{
				Object valueObject;
				if(TryParseValue(property.PropertyType.FullName, value, out valueObject))
				{
					property.SetValue(thing, valueObject);
				}
			}
		}

		public static void ClearAllEventHandlers(Object fromObject)
		{
			FieldInfo[] fields = fromObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
			foreach(FieldInfo field in fields)
			{
				if(field.FieldType.BaseType == typeof(MulticastDelegate))
				{
					EventInfo eventInfo = fromObject.GetType().GetEvent(field.Name);
					MulticastDelegate evnt = field.GetValue(fromObject) as MulticastDelegate;
					if(evnt != null)
					{
						foreach(Delegate del in evnt.GetInvocationList())
						{
							eventInfo.RemoveEventHandler(fromObject, del);
						}
					}
				}
			}
		}

		public static Type GetFirstConcreteType(Type interfaceType)
		{
			Type returnType = null;

			List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
			assemblies.Reverse();
			foreach(Assembly assembly in assemblies)
			{
				foreach(Type type in assembly.GetTypes())
				{
					if(interfaceType.IsAssignableFrom(type) && type.IsInterface == false)
					{
						returnType = type;
						break;
					}
				}

				if(returnType != null)
				{
					break;
				}
			}
			return returnType;
		}

		public static ConstructorInfo GetDefaultConcreteConstructor(Type type)
		{
			Type concreteType = type;
			if(type.IsInterface)
			{
				if((concreteType = ReflectionUtilities.GetFirstConcreteType(type)) == null)
				{
					throw new Exception(String.Format("No concrete implementation found for {0}", type.Name));
				}
			}

			ConstructorInfo constructor;
			constructor = concreteType.GetConstructor(new Type[] {} );
			if(constructor == null)
			{
				throw new Exception(String.Format("No default constructor found for {0}", type.Name));
			}

			return constructor;
		}

		public static bool IsEnumerableType(Type type)
		{
			return type.GetInterface("IEnumerable") != null;
		}

		public static int GetEnumerableCount(Object value)
		{
			int count = 0;
			Type type = value.GetType();
			MethodInfo method = type.GetMethod("Count");
			if(method != null)
			{
				count = (int)method.Invoke(value, null);
			}
			return count;
		}

		public static bool TryParseValue(String systemType, String value, out Object parsedObject)
		{
			bool parsed = false;
			parsedObject = null;
			try
			{
				Type parameterType = Type.GetType(systemType);
				if(TryGetType(systemType, out parameterType) == false)
				{
					String[] parts = systemType.Split('.');
					if(parts.Length > 1 && TryGetTypeByNameOnly(parts[parts.Length - 1], out parameterType) == false)
					{
						throw new CommonException("Cannot Instantiate Invalid System Type: {0}", systemType);
					}

					Log.SysLogText(LogLevel.ERROR, "SEVERE WARNING: Strictly named type '{0}' not found... Using '{1}' instead", systemType, parameterType);
				}

				/** special for timespans */
				if(systemType == typeof(TimeSpan).FullName)
				{
					TimeSpan ts;
					if(TimeSpanExtensions.TryParse(value, out ts))
					{
						parsedObject = ts;
						parsed = true;
					}
				}
				else if (systemType == typeof (System.String).FullName)
				{
					parsedObject = value;
					parsed = true;
				}
				else if(parameterType != null && parameterType.IsEnum)
				{
					parsedObject = Enum.Parse(parameterType, value);

					parsed = true;
				}
				else
				{
					/** find a TryParse method */
					MethodInfo tryParse = parameterType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null,
															new Type[] { typeof(String), parameterType.MakeByRefType() },
															null);
					if(tryParse == null)
					{
						throw new ReflectionParsingException("No TryParse method found for Type: {0}", systemType);
					}

					/** call the TryParse */
					Object[] args = new Object[] { value, null };
					if((parsed = (bool)tryParse.Invoke(null, args)) == true)
					{
						parsedObject = args[1];
					}
				}

				if(parsed == false)
				{
					throw new ReflectionParsingException("Could not parse {0} for Type: {1}", value, systemType);
				}
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.ERROR, "{0}", e.Message);
			}

			return parsed;
		}

		private static bool TryGetType(String systemType, out Type parameterType)
		{
			parameterType = null;

			/** if not in the system namespace, it's probably in on of ours */
			List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
			assemblies.Reverse();
			foreach(Assembly assembly in assemblies)
			{
				if((parameterType = assembly.GetType(systemType)) != null)
				{
					break;
				}
			}

			return parameterType != null;

		}

		private static bool TryGetTypeByNameOnly(String systemType, out Type parameterType)
		{
			parameterType = null;

			/** if not in the system namespace, it's probably in on of ours */
			List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
			assemblies.Reverse();
			foreach(Assembly assembly in assemblies)
			{
				foreach(Type type in assembly.GetTypes())
				{
					if(type.Name == systemType)
					{
						parameterType = type;
						break;
					}
				}
			}

			return parameterType != null;

		}

		public static bool HasAttribute(object o, Type attributeType)
		{
			Type oType = o.GetType();

			object []attrs = oType.GetCustomAttributes(attributeType, true);

			return attrs.Length > 0;
		}

		public static HashSet<object> GetEnumValuesWithAttribute(Type enumType, Type attributeType)
		{
			HashSet<object> result = new HashSet<object>();
			FieldInfo []fields = enumType.GetFields();

			foreach(FieldInfo fi in fields)
			{
				if(fi.GetCustomAttributes(attributeType, true).Length > 0)
				{
					result.Add(fi.GetValue(enumType));
				}
			}

			return result;
		}

		public static bool TryGetStringValueFromAttribute(Type enumType, Type attributeType, out string value)
		{
			if(attributeType.IsSubclassOf(typeof(StringAttribute)) == false)
			{
				throw new ReflectionParsingException("Attribute must be descendant of StringAttribute");
			}
	
			value = null;
			FieldInfo[] fields = enumType.GetFields();
			foreach(FieldInfo field in fields)
			{
				object[] attrs = field.GetCustomAttributes(attributeType, true);
				if(attrs.Length > 0)
				{
					value = ((StringAttribute)attrs[0]).Value;
					break;
				}
			}

			return value != null;
		}

		public static bool TrySetValue(String typeName, Object stringValue, out Object instance)
		{
			bool parsed = false;
			instance = null;

			/** get the type */
			Type parameterType = Type.GetType(typeName);
			if(parameterType == null)
			{
				/** if not in the system namespace, it's probably in on of ours */
				List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
				assemblies.Reverse();
				foreach(Assembly assembly in assemblies)
				{
					if((parameterType = assembly.GetType(typeName)) != null)
					{
						break;
					}
				}

				if(parameterType == null)
				{
					throw new ReflectionParsingException("Cannot Instantiate Invalid Parameter Type: {0}", typeName);
				}
			}

			/** special for timespans */
			if(parameterType == typeof(TimeSpan))
			{
				TimeSpan ts;
				if(TimeSpanExtensions.TryParse(stringValue.ToString(), out ts))
				{
					instance = ts;
					parsed = true;
				}
			}
			else if(parameterType == typeof(String))
			{
				instance = stringValue;
				parsed = true;
			}
			else if(parameterType != null && parameterType.IsEnum)
			{
				try
				{
					instance = Enum.Parse(parameterType, stringValue.ToString());
					parsed = true;
				}
				catch
				{
					parsed = false;
				}
			}
			else
			{
				/** find a TryParse method */
				MethodInfo tryParse = parameterType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null,
														new Type[] { typeof(String), parameterType.MakeByRefType() },
														null);
				if(tryParse == null)
				{
					throw new ReflectionParsingException("No TryParse method found for Type: {0}", typeName);
				}

				/** call the TryParse */
				Object[] args = new Object[] { stringValue.ToString(), null };
				if((parsed = (bool)tryParse.Invoke(null, args)) == true)
				{
					stringValue = args[1];
					instance = args [1];
				}
			}

			return parsed;
		}
	}
}
