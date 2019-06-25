using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KanoopCommon.Extensions;
using System.ComponentModel;

namespace KanoopCommon.Conversions
{
	public static class StringToProperty
	{
		public static void SetProperty(Object what, PropertyInfo property, List<String> values)
		{
			try
			{
				if(property.PropertyType.IsGenericType)
				{
					if(	TrySetFromList(what, property, values) == true)
					{
						/** */
					}
				}
				else
				{
					SetProperty(what, property, values[0]);
				}
			}
			catch(Exception )
			{

			}
		}

		public static void SetProperty(Object what, PropertyInfo property, String value)
		{
			try
			{
				if(	TrySetFromStringConstructor(what, property, value) == true ||
					TrySetFromList(what, property, value) == true)
				{
					/** */
				}
				else if(property.PropertyType == typeof(TimeSpan))
				{
					TrySetTimeSpanFromString(what, property, value);
				}
				else if(property.PropertyType.IsEnum)
				{
					Enum e = Enum.Parse(property.PropertyType, value, true) as Enum;
					property.SetValue(what, e, null);
				}
				else if(property.PropertyType == typeof(DateTime))
				{
					DateTime dt;
					if(DateTimeExtensions.TryParse(value, out dt))
					{
						property.SetValue(what, dt, null);
					}
				}
				else
				{
					Object o = Convert.ChangeType(value, property.PropertyType);
					property.SetValue(what, o, null);
				}

			}
			catch(Exception )
			{

			}
		}

		public static bool TrySetFromList(Object what, PropertyInfo property, String values)
		{
			bool bRet = false;

			if(property.PropertyType.IsGenericType)
			{
				Type[] args = property.PropertyType.GetGenericArguments();
				if(args.Length == 1)
				{
					ConstructorInfo constructor = property.PropertyType.GetConstructor(new Type[]{} );
					if(constructor != null)
					{
						Object collection = constructor.Invoke(new Object[]{} );
						MethodInfo addMethodInfo = property.PropertyType.GetMethod("Add");

						String[] parts = values.Split(',');
						if(parts.Length > 0 && parts[0].Length > 0)
						{
							foreach(String part in parts)
							{
								Object value = Convert.ChangeType(part, args[0]);
								addMethodInfo.Invoke(collection, new object[] { value });
							}

						}
						property.SetValue(what, collection, null);
						bRet = true;
					}
				}
			}
			return bRet;
		}

		public static bool TrySetFromList(Object what, PropertyInfo property, List<String> values)
		{
			bool result = false;

			if(property.PropertyType.IsGenericType)
			{
				Type[] args = property.PropertyType.GetGenericArguments();
				if(args.Length == 1)
				{
					Type objectType = args[0];

					if(values.Count == 1 && ObjectExtensions.IsNumericType(objectType ) && TrySetFromList(what, property, values[0]))
					{
						result = true;
					}

					if(result == false)
					{
						ConstructorInfo constructor = property.PropertyType.GetConstructor(new Type[]{} );
						if(constructor != null)
						{
							Object collection = constructor.Invoke(new Object[]{} );
							MethodInfo addMethodInfo = property.PropertyType.GetMethod("Add");

							foreach(String part in values)
							{
								TypeConverter converter = TypeDescriptor.GetConverter(objectType);
								if(converter.CanConvertFrom(typeof(String)))
								{
									/** try straight conversion first */
									Object value = Convert.ChangeType(part, objectType);
									addMethodInfo.Invoke(collection, new object[] { value });
								}
								else
								{
									Object value;
									if(TryCreateFromStringConstructor(args[0], part, out value))
									{
										addMethodInfo.Invoke(collection, new object[] { value });
									}
								}
							}

							property.SetValue(what, collection, null);

							result = true;
						}
					}
				}
			}
			return result;
		}

		public static bool TrySetFromStringConstructor(Object what, PropertyInfo property, String strVal)
		{
			bool bRet = false;

			/** try find a string constructor */
			Type[] types = new Type[] { typeof(String) };
			ConstructorInfo constructor = property.PropertyType.GetConstructor(new Type[] { typeof(String) } );
			if(constructor != null)
			{
				Object value = constructor.Invoke(new object[] { strVal });
				if(value != null)
				{
					property.SetValue(what, value, null);
					bRet = true;
				}
			}
			else if(property.PropertyType == typeof(bool))
			{
				if(String.IsNullOrEmpty(strVal))
					strVal = "1";

				int val;
				bool boolVal;
				if(Int32.TryParse(strVal, out val))
				{
					property.SetValue(what, val == 1, null);
					bRet = true;
				}
				else if(Boolean.TryParse(strVal, out boolVal))
				{
					property.SetValue(what, boolVal, null);
					bRet = true;
				}
			}
			return bRet;
		}

		public static bool TryCreateFromStringConstructor(Type type, String strVal, out Object value)
		{
			value = null;

			/** try find a string constructor */
			Type[] types = new Type[] { typeof(String) };
			ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(String) } );
			if(constructor != null)
			{
				value = constructor.Invoke(new object[] { strVal });
			}

			return value != null;
		}

		public static bool TrySetTimeSpanFromString(Object what, PropertyInfo property, String stringValue)
		{
			bool result = false;
	
			TimeSpan ts;
			if(TimeSpanExtensions.TryParse(stringValue, out ts))
			{
				property.SetValue(what, ts, null);
				result = true;
			}

			return result;
		}

	}
}
