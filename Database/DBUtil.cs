using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Odbc;
using System.Threading;

using KanoopCommon.Extensions;
using KanoopCommon.Logging;
using KanoopCommon.Threading;
using KanoopCommon.Types;
using KanoopCommon.CommonObjects;
using KanoopCommon.Addresses;
using System.Globalization;
using System.Data.Common;
using KanoopCommon.Crypto;
using KanoopCommon.Conversions;
using System.ComponentModel;
using MySql.Data.Types;
using MySql.Data.MySqlClient;

namespace KanoopCommon.Database
{
	/// <summary>
	/// DBUtil 
	///     Database utility class holding all datatype conversions. Useful as an ancestor class for 
	///     Application data source objects to make easy use of conversion routines
	/// </summary>
	public class DBUtil 
	{
		public DBUtil()
		{
//            m_ConnectionLock = new MutexLock();
//            m_Connections = new ConnectionMap();
		}

		// **************************************
		// Initialization Routines.... One of which must be called prior to object use
		// **************************************
		public void Init(String strDbDriver,
					String strDbHost,
					String strDbUser,
					String strDbPass,
					String strDbSchema)
		{
			Init(new SqlDBCredentials(strDbDriver, strDbHost, strDbSchema, strDbUser, strDbPass));
		}
		public void Init(SqlDBCredentials creds)
		{
			m_Credentials = creds;
			m_sqlDataSource = SqlConnectionPool.Instance(m_Credentials).GetDataSource();

		}
		ISqlDataSource m_sqlDataSource;
		public ISqlDataSource SqlDataSource
		{
			get { return m_sqlDataSource; }
			set { m_sqlDataSource = value; }
		}


		SqlDBCredentials	m_Credentials;
		public SqlDBCredentials Credentials
		{
			get { return m_Credentials; }
			set { m_Credentials = value; }
		}

		// *****************************************************************************
		// DataReader Ojbect converters - Obtain typed values from object of known type
		// *****************************************************************************
		public static String GetString(Object val)		{	return Convert.IsDBNull(val) ? "" : val.ToString();							}
		public static Int32 GetInt32(Object val)
		{
			Int32 value;
			if(Convert.IsDBNull(val))
				value = 0;
			else if(val is String && Int32.TryParse((String)val, out value) == false)
				value = 0;
			else
				value = Convert.ToInt32(val.ToString());
			return value;
		}
		public static Int32 GetInt32(DatabaseDataReader reader, string columnName) 
		{
			return ReadValue<int>(reader, columnName, obj => Convert.ToInt32(obj));
		}
		public static MimeType GetMimeType(Object val)
		{
			return Convert.IsDBNull(val) ? MimeType.UNKNOWN : (MimeType)Convert.ToInt32(val.ToString());
		}

		public static Int32? GetNullableInt32(Object val) { return Convert.IsDBNull(val) ? (Int32?)null : Convert.ToInt32(val.ToString()); }
		public static decimal GetDecimal(Object val) { return Convert.IsDBNull(val) ? 0 : Convert.ToDecimal(val); }
		public static decimal GetDecimal(DatabaseDataReader reader, string columnName)
		{
			return ReadValue<decimal>(reader, columnName, obj => Convert.ToDecimal(obj));
		}

		public static Int64 GetInt64(Object val) { return Convert.IsDBNull(val) ? 0 : Convert.ToInt64(val.ToString()); }
		public static byte GetByte(Object val) { return Convert.IsDBNull(val) ? (byte)0 : Convert.ToByte(val.ToString()); }
		public static UInt64 GetUInt64(Object val) { return Convert.IsDBNull(val) ? 0 : Convert.ToUInt64(val.ToString()); }
		public static UInt32 GetUInt32(Object val) { return Convert.IsDBNull(val) ? 0 : Convert.ToUInt32(val.ToString()); }
		public static UInt32? GetNullableUInt32(Object val) { return Convert.IsDBNull(val) ? (UInt32?)null : Convert.ToUInt32(val.ToString()); }
		public static UInt16 GetUInt16(Object val) { return Convert.IsDBNull(val) ? (UInt16)0 : Convert.ToUInt16(val.ToString()); }
		public static Double GetDouble(Object val) { return Convert.IsDBNull(val) ? 0 : Convert.ToDouble(val.ToString()); }
		public static Double? GetNullableDouble(Object val) { return Convert.IsDBNull(val) ? (Double?)null : Convert.ToDouble(val.ToString()); }

		public static byte[] GetByteArray(Object val) { return Convert.IsDBNull(val) ? new byte[0] : (byte[])val; }
		
		public static bool GetBoolean(Object val) 
		{
			bool result = false;

			if(!Convert.IsDBNull(val))
			{
				BooleanExtensions.TryParse(val.ToString(), out result);
			}

			return result;
		}
		
		public static DateTime GetDateTimeUTC(Object val)
		{
			DateTime ret = GetDateTime(val);
			ret = DateTime.SpecifyKind(ret, DateTimeKind.Utc);
			return ret;
		}

		public static DateTime? GetNullableDateTime(Object val)
		{
			DateTime? actualRet;
			DateTime ret;

			if (!Convert.IsDBNull(val))
			{
				if (val is MySqlDateTime)
				{
					MySqlDateTime d = (MySqlDateTime)val;
					if (d.IsValidDateTime)
					{
						ret = d.Value;
					}
					else
					{
						ret = DateTime.MinValue;
					}
				}
				else if (val is DateTime)
				{
					ret = (DateTime)val;
				}
				else
				{
					string strTime = val.ToString();

					if (strTime.Length == 0)		// SPSP Why would we want to do this? || strTime[0] == '0')
					{
						ret = new DateTime();
					}
					else
					{
						// Try Parsing From Common String Format
						if (!DateTime.TryParse(val.ToString(), out ret))
						{
							// Bug Fix: 250
							// Try Parsing From Double or Decimal
							strTime = String.Format("{0:0.000}", val);

							if (DateTime.TryParseExact(strTime, "yyyyMMddHHmmss.fff", new CultureInfo("en-US", true), DateTimeStyles.None, out ret) == false)
							{
								strTime = String.Format("{0:0}", val);

								if (DateTime.TryParseExact(strTime, "yyyyMMddHHmmss", new CultureInfo("en-US", true), DateTimeStyles.None, out ret) == false)
								{
									ret = new DateTime();
								}
							}
						}
					}
				}

				actualRet = ret;
			}
			else
			{
				actualRet = null;
			}

			return actualRet;
		}

		public static DateTime GetDateTime(Object val)	
		{
			DateTime ret;

			if (!Convert.IsDBNull(val))
			{
				if (val is MySqlDateTime)
				{
					MySqlDateTime d = (MySqlDateTime)val;
					if(d.IsValidDateTime)
					{
						ret = d.Value;
					}
					else
					{
						ret = DateTime.MinValue;
					}
				} 
				else if (val is DateTime) 
				{
					ret = (DateTime)val;
				} 
				else
				{
					string strTime = val.ToString();

					if (strTime.Length == 0)		// SPSP Why would we want to do this? || strTime[0] == '0')
					{
						ret = new DateTime();
					}
					else
					{
						// Try Parsing From Common String Format
						if (!DateTime.TryParse(val.ToString(), out ret))
						{
							// Bug Fix: 250
							// Try Parsing From Double or Decimal
							strTime = String.Format("{0:0.000}", val);

							if (DateTime.TryParseExact(strTime, "yyyyMMddHHmmss.fff", new CultureInfo("en-US", true), DateTimeStyles.None, out ret) == false)
							{
								strTime = String.Format("{0:0}", val);

								if (DateTime.TryParseExact(strTime, "yyyyMMddHHmmss", new CultureInfo("en-US", true), DateTimeStyles.None, out ret) == false)
								{
									ret = new DateTime();
								}
							}
						}
					}
				}
			}
			else
			{
				ret = new DateTime();
			}

			return ret;
		}
		public static TimeSpan GetTimeSpan(Object val)
		{
			TimeSpan ret;
			if(!Convert.IsDBNull(val))
			{
				String strTime = val.ToString();
				if(strTime.Length == 0)		// SPSP Why would we want to do this? || strTime[0] == '0')
				{
					ret = new TimeSpan();
				}
				else
				{
					// Try Parsing From Common String Format
					if (!TimeSpan.TryParse(val.ToString(), out ret))
					{
						return new TimeSpan();
					}
				}
			}
			else
			{
				ret = new TimeSpan();
			}
			return ret;
		}
		public static AddressBase GetAddress(Object address, Object type)
		{
			AddressBase ret;
			if(Convert.IsDBNull(address) || Convert.IsDBNull(type))
				ret = new AddressBase();
			else
			{
				try
				{
					ret = AddressBase.Factory(address.ToString(), (AddressType)(Convert.ToInt32(type)));
				}
				catch
				{
					ret = new EmptyAddress();
				}
			}
			return ret;
		}
		public static MACAddress GetMACAddress(Object address)
		{
			MACAddress ret;
			if(Convert.IsDBNull(address))
				ret = new MACAddress();
			else
				ret = new MACAddress(address.ToString());
			return ret;
		}
		public static UUID GetUUID(Object uuid)
		{
			UUID ret = UUID.EmptyUUID;
			try
			{
				if (!Convert.IsDBNull(uuid))
				{
					ret = new UUID(uuid.ToString());
				}
			}
			catch {}

			return ret;
		}
		public static SoftwareVersion GetSoftwareVersion(Object value)
		{
			SoftwareVersion ret;
			if(Convert.IsDBNull(value))
			{
				ret = new SoftwareVersion();
			}
			else
				ret = new SoftwareVersion(value.ToString());
			return ret;
		}
		public static OperatingSystemType GetOperatingSystem(Object value)
		{
			OperatingSystemType ret;
			int val;
			if(Convert.IsDBNull(value) || Int32.TryParse(value.ToString(), out val) == false)
			{
				ret = OperatingSystemType.UNKNOWN;
			}
			else
			{
				ret = (OperatingSystemType)val;
			}
			return ret;
		}
		public static IPv4AddressPort GetIPv4AddressPort(Object address)
		{
			IPv4AddressPort ret;
			if(Convert.IsDBNull(address))
				ret = new IPv4AddressPort();
			else
				ret = new IPv4AddressPort(address.ToString());
			return ret;
		}
		public static LogLevel GetLogLevel(Object val)
		{
			return Convert.IsDBNull(val) ? LogLevel.NOTHING : (LogLevel)Convert.ToInt32(val.ToString());
		}
		// internal recurssive routine that ignores one time only check for null and SRID value

#if LATER
		private static PointD GetPointFromWKB(byte[] bytData, ref int nOffset, bool bLittleEndian)
		{
			if (bLittleEndian)
				Array.Reverse(bytData, nOffset, 8);
			double x = BitConverter.ToDouble(bytData, nOffset);
			nOffset += 8;

			if (bLittleEndian)
				Array.Reverse(bytData, nOffset, 8);
			double y = BitConverter.ToDouble(bytData, nOffset);

			return new PointD(x, y);
		}

		private static Object GetObjectFromWKB(byte[] bytData, ref Int32 nOffset)
		{
			Object retVal=null;
			// WKB Endianness
			bool bLittleEndian = (bytData[nOffset] == 0x00); // determine if data must be reversed prior to converting to number
			nOffset++;

			// WKB Type
			if (bLittleEndian)
				Array.Reverse(bytData, nOffset, 4);
			UInt32 wkbType = BitConverter.ToUInt32(bytData, nOffset);
			nOffset+=4;


			switch (wkbType)
			{
				case 1: // point
					retVal = GetPointFromWKB(bytData, ref nOffset, bLittleEndian);
					break;
				case 2: //linestring
					retVal = GetLineStringFromWKB(bytData, ref nOffset, bLittleEndian);
					break;
				case 3: // LinearRing (polygon)
					retVal = GetPolygon(bytData, ref nOffset, bLittleEndian);
					break;
				case 4: // MultiPoint
					break;
				case 5: // MultiLineString
					retVal = GetPointDListListFromWKB(bytData, ref nOffset, bLittleEndian);
					break;
				case 6: // MultiPolygon
					break;
				case 7: // GeometryCollection
					break;
			}

			return retVal;


		}

		// External entrypoint that checks for null, and skips SRID to iterate array
		public static Object GetWKB(Object val)
		{
			Object retVal = null;
			if (!Convert.IsDBNull(val))
			{
				Int32 nOffset = 4; // Ignore the first 4 bytes (SRID Value.... unique to Mysql, but not part of WKB)
				retVal = GetObjectFromWKB((byte[])val,ref nOffset);
			}
			return retVal;
		}
#endif

		// Geometry Collection function... recurses into GetObjectFromWKB because child geometries store all data including endianness and type
		public static String DBNullText(Object val) { return val == null ? "null" : val.ToString(); }
		public static String DBStringNullOrEmpty(String str) { return string.IsNullOrEmpty(str) ? "null" : str; }
		public static String GetSequencedSwitch(String fieldName, String commaDelimitedIDs,String defaultVal)
		{
			if (commaDelimitedIDs.Trim().Length==0)
				return defaultVal;

			String retVal = "CASE " + fieldName + "\n";
			String[] ids = commaDelimitedIDs.Split(',');
			for (uint i =0;i<ids.Length;i++)
			{
				retVal += String.Format("  WHEN {0} THEN {1} \n", ids[i], i + 1);
			}
			retVal += "  ELSE " + defaultVal +"\n";
			retVal += "END ";
			return retVal;
		}
		public static String GetTempTable(String commaDelimitedIDs, String columnName)
		{
			if (commaDelimitedIDs.Trim().Length == 0)
				return String.Format("select 0 SEQ, 0 {0} from dual where 1=0", columnName);

			String retVal = "";
			String[] ids = commaDelimitedIDs.Split(',');
			for (int i = 0; i < ids.Length; i++)
			{
				retVal += String.Format("select {0} SEQ, {1} {2}", (i+1),ids[i],columnName);
				if (i < ids.Length - 1)
					retVal += " \n\t UNION ALL \n";
			}
			return  String.Format("( \n{0} \n ) ", retVal);
		}
		public static T GetEnum<T>(Int32 val) where T:struct,IConvertible
		{
			return (T)(object)val;
		}

		public static T GetValue<T>(Object value)
		{
			T retVal = default(T);
			ConversionDelegate f;

			if (DatabaseDataReader.ConversionDict.TryGetValue(typeof(T).GetHashCode(), out f))
				retVal = (T)f(value);
			else
			{
				TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
				if (c!=null)
				{
					f= c.ConvertFrom;
					DatabaseDataReader.ConversionDict.Add(typeof(T).GetHashCode(),f);
					retVal = (T)f(value);
				}
			}
			return retVal;
		}

		//
		// Subscriber Content
		// 
		// *****************************************************************************
		// Formatting routines to put in a usable format for SQL statements
		// *****************************************************************************
		public static String MySQLTime(DateTime dt, bool milliseconds = true)
		{
			String format = milliseconds
				? "{0:yyyy-MM-dd HH:mm:ss.fff}"
				: "{0:yyyy-MM-dd HH:mm:ss}";
			return String.Format(format, dt);
		}
		
		public static Unescaped MySQLQuotedTime(DateTime dt)
		{
			return Unescaped.String(String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", dt));
		}
		
		public static Unescaped MySQLQuotedDateOnly(DateTime dt)
		{
			return Unescaped.String(String.Format("'{0:yyyy-MM-dd}'", dt));
		}
		
		public static Unescaped MySQLQuotedTimeOnly(DateTime dt)
		{
			return Unescaped.String(String.Format("'{0:HH:mm:ss}'", dt));
		}

		public static String MySQLEscapeString(string unescaped)
		{
			return MySqlHelper.EscapeString(unescaped);
		}
		public static String MySQLTimeDecimalTime(DateTime dt)
		{
			return String.Format("{0:yyyyMMddHHmmss.fff}", dt);
		}
		
		public static String MSSQLTime(DateTime dt)
		{
			return String.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", dt);
		}
		
		public static String CommaSeparatedIDList(uint[] array)
		{
			return CommaSeparatedIDList(new List<uint>(array));
		}
		
		public static String CommaSeparatedIDList(List<UInt32> list)
		{
			return CommaSeparatedIDList<UInt32>(list);
		}
		
		public static String CommaSeparatedIDList(IEnumerable<IHasPrimaryID> list)
		{
			List<UInt32> ids = new List<UInt32>();

			foreach(IHasPrimaryID member in list)
			{
				ids.Add(member.PrimaryID);
			}

			return CommaSeparatedIDList<UInt32>(ids);
		}
		
		public static String CommaSeparatedIDList<T>(List<T> list)
		{
			bool isEnum = typeof(T).IsEnum;

			StringBuilder str = new StringBuilder(list.Count * 6);
			for (int x = 0; x < list.Count; x++)
			{
				if(isEnum)
				{
					str.AppendFormat("{0},", Convert.ToInt32(list[x]));
				}
				else
				{
					str.AppendFormat("{0},", list[x]);
				}
			}
			return str.ToString().TrimEnd(',');
		}
		
		public static Unescaped CommaSeparatedStringList<T>(List<T> list)
		{
			StringBuilder str = new StringBuilder(255);
			for (int x = 0; x < list.Count; x++)
			{
				str.AppendFormat("'{0}',", list[x]);
			}
			return new Unescaped(str.ToString().TrimEnd(','));
		}
		
		public static String EscapeString(String inStr)
		{
			StringBuilder outString = new StringBuilder(inStr.Length * 2);

			int nOffset;
			for (nOffset = 0; nOffset < inStr.Length; nOffset++)
			{
				if (inStr[nOffset] == '\'' ||
					inStr[nOffset] == '\"' ||
					inStr[nOffset] == '\\')
				{
					outString.Append('\\');
					outString.Append(inStr[nOffset]);
				}
				else if (inStr[nOffset] == '\n')
				{
					outString.Append("\\n");
				}
				else if (inStr[nOffset] == '\r')
				{
					outString.Append("\\r");
				}
				else
				{
					outString.Append(inStr[nOffset]);
				}


			}

			return outString.ToString();
		}

		public static bool IsDBNull(Object val) { return Convert.IsDBNull(val); }

		// **************************************
		// SQL Datetime format constants
		// **************************************
		public const string FORMAT_DB_TIMESTAMP = "yyyy-MM-dd HH:mm:ss";
		public const string FORMAT_DB_DATE = "yyyy-MM-dd";

		public static uint[] SplitCommaList(string commaList)
		{
			string[] split = commaList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			uint[] result = new uint[split.Length];
			int i;
			for (i = 0; i < split.Length; i++)
			{
				if (false == uint.TryParse(split[i], out result[i]))
				{
					result[i] = 0;
				}
			}

			return result;
		}

		public static String Boolean(bool value)
		{
			return value ? "1" : "0";
		}

		private static T ReadValue<T>(DatabaseDataReader reader, string columnName, Func<object, T> callback)			
		{
			try
			{
				int ordinal = reader.GetOrdinal(columnName);
				
				if(!Convert.IsDBNull(reader[ordinal])) 
				{
					return callback(reader[ordinal]);
				}
			}
			catch (IndexOutOfRangeException rangeEx)
			{
				Log.SysLogText(LogLevel.ERROR, "DBUtil: column=" + columnName + " is not a valid column name in the returned results.");
				Log.SysLogText(LogLevel.ERROR, rangeEx.Message);
				Log.SysLogText(LogLevel.ERROR, rangeEx.StackTrace);
			}
			catch (Exception ex)
			{
				Log.SysLogText(LogLevel.ERROR, ex.Message);
				Log.SysLogText(LogLevel.ERROR, ex.StackTrace);
			}

			return default(T);
		}
	}
}
