using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;
using System.Data.Odbc;
using System.ComponentModel;
using KanoopCommon.Database;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Web;
using KanoopCommon.Geometry;
using MySql.Data.Types;

namespace KanoopCommon.Conversions
{
	public class DataReaderConverter
	{
		class PropertyLookup: Dictionary<string,PropertyInfo>
		{
		}
		class ClassMap 
		{
			PropertyLookup _dbProperties = new PropertyLookup();
			public PropertyLookup DBProperties
			{
				get { return _dbProperties; }
				set { _dbProperties = value; }
			}
			PropertyLookup _classProperties = new PropertyLookup();
			public PropertyLookup ClassProperties
			{
				get { return _classProperties; }
				set { _classProperties=value; }
			}
			bool _attributed = false;
			public bool IsAttributed
			{
				get { return _attributed; }
				set { _attributed=value; }
			}
		}
		
		static Dictionary<Type, ClassMap> _classDictionary = new Dictionary<Type, ClassMap>();

		private static ClassMap CacheReflection(Type type)
		{
			ClassMap classMap = new ClassMap();
			lock (_classDictionary)
			{
				if (_classDictionary.ContainsKey(type))
					return _classDictionary[type];

				_classDictionary.Add(type, classMap);
			}
//			System.Console.WriteLine("Building PropertyLookup for "+type.ToString());

			// first get the properties
			PropertyInfo[] propertyInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (PropertyInfo prop in propertyInfo)
			{
				if (!prop.CanWrite)
					continue;

				bool attributed = false;
				foreach (Object attribute in prop.GetCustomAttributes(false))
				{
					if (attribute is ColumnNameAttribute)
					{
						foreach (string fieldName in ((ColumnNameAttribute)attribute).NameList)
							classMap.DBProperties.Add(fieldName, prop);

						attributed = classMap.IsAttributed = true;
						break;
					}
				}

				if (!attributed && prop.PropertyType.IsClass && !prop.PropertyType.IsPrimitive && !prop.PropertyType.Namespace.StartsWith("System"))
				{
					CacheReflection(prop.PropertyType);
					classMap.ClassProperties.Add(prop.Name, prop);
				}

				/*
				if (strFieldName != null)
					hshMap.Add(strFieldName, prop);
				else
					hshMap.Add(prop.Name, prop);
				 */
			}
			return classMap;

		}

		public static T CreateClassFromDataReader<T>(IDataReader reader) where T: new()
		{
			T item = new T();
			LoadClassFromDataReader(item, reader, false);
			return item;
		}

		public static bool LoadClassFromDataReader(Object objClass, IDataReader reader)
		{
			return LoadClassFromDataReader(objClass, reader, true);
		}

		public static bool LoadClassFromDataReader(Object objClass, IDataReader reader, bool bTryToLoadMemberClasses)
		{
			bool bRet = false;
			Type t = objClass.GetType();
			if (t.IsPrimitive)
			{
			}
			else
			{
				ClassMap classMap = CacheReflection(t);
				if (classMap.IsAttributed)
				{
					// Roll through data reader field by field and assign values to the object.
					GetValuesByColumn(objClass, reader, classMap, bTryToLoadMemberClasses);
				}

			}
			return bRet;
		}

		private static void GetValuesByColumn(Object objClass, IDataReader reader, ClassMap classMap, bool bTryToLoadMemberClasses)
		{

			bool bFound = false;
			for (int i = 0; i < reader.FieldCount; i++)
			{
				string strColumnName = reader.GetName(i);

				PropertyInfo prop = null;
				if (classMap.DBProperties.TryGetValue(strColumnName, out prop))
				{
					bFound = true;
					if (reader.IsDBNull(i))
					{
						prop.SetValue(objClass, null, null);
					}
					else
					{
						try
						{
							if (prop.PropertyType.IsEnum)
							{
								Type fieldType =  reader.GetFieldType(i);
								Object value = reader[i];
								if(fieldType == typeof(String))
								{
									value = Convert.ToInt32(((String)value)[0]);
								}
								prop.SetValue(objClass, Enum.ToObject(prop.PropertyType, value), null);
							}
							else if(prop.PropertyType == typeof(PointD) && reader.GetFieldType(i) == typeof(byte[]))
							{
								Byte[] value = reader[i] as Byte[];
								MySqlGeometry point = new MySqlGeometry(MySql.Data.MySqlClient.MySqlDbType.Geometry, (byte[])value);
								prop.SetValue(objClass, new PointD((Double)point.XCoordinate, (Double)point.YCoordinate));
							}
							else if(prop.PropertyType.IsClass && 
									prop.PropertyType.IsPrimitive == false && 
									(prop.PropertyType.Namespace == null || prop.PropertyType.Namespace.StartsWith("System") == false))
							{
								TypeConverter tc = TypeDescriptor.GetConverter(prop.PropertyType);
								object obj = tc.ConvertFrom(reader[i]);
								prop.SetValue(objClass, obj, null);
//								prop.SetValue(objClass, Convert.ChangeType(tc.ConvertFrom(reader[i]), prop.PropertyType), null);
							}
							else
							{
								// Bug Fix: 250
								Type type = reader[i].GetType();
								if (prop.PropertyType == typeof(System.DateTime))
								{
									if(type == typeof(Decimal) || type == typeof(Double))
									{
										// Special Handling For Decimal(17,3) Database Timestamps
										prop.SetValue(objClass, DBUtil.GetDateTime(reader[i]), null);
									}
									else
									{
										try
										{
											Object o = reader[i];
											if(o is MySql.Data.Types.MySqlDateTime && ((MySql.Data.Types.MySqlDateTime)o).IsValidDateTime == false)
											{
												prop.SetValue(objClass, DateTime.MinValue, null);
											}
											else
											{
												prop.SetValue(objClass, Convert.ChangeType(reader[i], prop.PropertyType), null);
											}
										}
										catch(Exception)
										{
											prop.SetValue(objClass, DateTime.MinValue, null);
										}
									}
								}
								else
								{
									Object o = reader[i];
									/** special handling for boolean 0/1 */
									if(prop.PropertyType == typeof(bool) && Char.IsDigit(o.ToString()[0]))
									{
										prop.SetValue(objClass, o.ToString()[0] == '0' ? false : true, null);
									}
									/** special handling for blob as string */
									else if(prop.PropertyType == typeof(String) && o.GetType() == typeof(byte[]))
									{
										prop.SetValue(objClass, ASCIIEncoding.UTF8.GetString((byte[])o), null);
									}
									else
									{
										prop.SetValue(objClass, Convert.ChangeType(reader[i], prop.PropertyType), null);
									}
								}
							}
						}
						catch (Exception e)
						{
							System.Console.WriteLine("Conversion Error " + e);
						}
					}
				}
			}

			if (bFound && bTryToLoadMemberClasses)
			{
				// roll through subtypes
				foreach (KeyValuePair<String, PropertyInfo> pair in classMap.ClassProperties)
				{
					ConstructorInfo constructor = pair.Value.PropertyType.GetConstructor(new Type[0]);
					if (constructor == null)
					{
						throw new Exception(String.Format("No constructor exists for target object type {0}", pair.Value.ToString()));
					}

					/**
					 * construct the target object
					 */
					Object objNew = constructor.Invoke(new Object[0]);
					pair.Value.SetValue(objClass, Convert.ChangeType(objNew, pair.Value.PropertyType), null);

					LoadClassFromDataReader(objNew, reader);
				}
			}
		}

		private static void GetValuesByProperty(Object objClass, OdbcDataReader reader, ClassMap classMap)
		{
			foreach (KeyValuePair<String, PropertyInfo> pair in classMap.DBProperties)
			{
				int i = -1;
				try
				{
					i= reader.GetOrdinal(pair.Key);
					if (i >= 0)
					{

						try
						{
							if (reader.IsDBNull(i))
								pair.Value.SetValue(objClass, null, null);
							else if (pair.Value.PropertyType.IsEnum)
							{
								pair.Value.SetValue(objClass, Enum.ToObject(pair.Value.PropertyType, reader[i]), null);
							}
							else if (!pair.Value.PropertyType.IsSerializable)
							{
								TypeConverter tc = TypeDescriptor.GetConverter(pair.Value.PropertyType);
								pair.Value.SetValue(objClass, Convert.ChangeType(tc.ConvertFrom(reader[i]), pair.Value.PropertyType), null);
							}
							else
							{
								pair.Value.SetValue(objClass, Convert.ChangeType(reader[i], pair.Value.PropertyType), null);
							}
						}
						catch (Exception e)
						{
							System.Console.WriteLine("Conversion Error " + e);
						}

					}

				}
				catch
				{
				}

			}

		}

	}

}
