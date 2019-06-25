using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using KanoopCommon.Logging;
using KanoopCommon.Crypto;
using KanoopCommon.CommonObjects;
using KanoopCommon.Types;
using KanoopCommon.Conversions;
using System.ComponentModel;
using KanoopCommon.Addresses;

namespace KanoopCommon.Database
{
	public class DatabaseDataReader :	IDisposable, 
										IDataReader
	{
		DbDataReader			m_Reader;
		public DbDataReader DbDataReader { get { return m_Reader; } set { m_Reader = value; } }

		ISqlConnectionPool		m_Pool;
		public ISqlConnectionPool ConnectionPool  { get { return m_Pool; } set { m_Pool = value; } }

		DatabaseConnection		m_Connection;
		public DatabaseConnection Connection  { get { return m_Connection; } set { m_Connection = value; } }

		protected bool m_bReleaseConnectionOnClose;
		
		protected bool			m_bWasDisposed;
		public bool WasDisposed { get { return m_bWasDisposed; } }

		public DatabaseDataReader(ISqlConnectionPool pool, DatabaseConnection conn, DbDataReader rdr)
		{
			m_bWasDisposed = false;
			m_bReleaseConnectionOnClose=true;
			m_Reader = rdr;
			m_Connection = conn;
			m_Pool = pool;
		}

		public DatabaseDataReader(DbDataReader rdr)
			: this(null, null, rdr) {}

		public DatabaseDataReader()
			: this(null, null, null) {}

		~DatabaseDataReader()
		{
			Dispose(false);
		}

		public bool HasRows 
		{ 
			get 
			{ 
				if (m_Reader!=null)
					return m_Reader.HasRows;
				return false;
			} 
		}

		public int Depth 
		{ 
			get 
			{ 
				if (m_Reader!=null)
					return m_Reader.Depth;
				return 0;
			} 
		}

		public int FieldCount
		{
			get
			{
				if (m_Reader != null)
					return m_Reader.FieldCount;
				return 0;
			}
		}

		public bool IsClosed
		{
			get
			{
				if (m_Reader != null)
					return m_Reader.IsClosed;
				return true;
			}
		}

		public int RecordsAffected
		{
			get
			{
				if (m_Reader != null)
					return m_Reader.RecordsAffected;
				return 0;
			}
		}

		public Object this[Int32 idx]
		{
			get
			{
				if (m_Reader != null)
					return Reader[idx];
				return null;
			}
		}
		public Object this[String idx]
		{
			get
			{
				if (m_Reader != null)
					return Reader[idx];
				return null;
			}
		}

		public void Close()
		{
			if (m_Reader != null)
			{
				if (!m_Reader.IsClosed)
					m_Reader.Close();
				m_Reader = null;
			}
			if (m_bReleaseConnectionOnClose && m_Pool != null && m_Connection != null)
				m_Pool.ReleaseConnection(m_Connection);
			m_Pool = null;
			m_Connection = null;
		}

		public bool GetBoolean(int i)
		{
			if (m_Reader != null)
				return m_Reader.GetBoolean(i);
			else
				return false;
		}

		public byte GetByte(int i)
		{
			if (m_Reader != null)
				return m_Reader.GetByte(i);
			else
				return (byte)0;
		}
		public long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
		{
			if (m_Reader != null)
				return m_Reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
			else
				return 0;
		}

		public char GetChar(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetChar(ordinal);
			else
				return (char)0;
		}

		public long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			if (m_Reader != null)
				return m_Reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
			else
				return 0;
		}

		public IDataReader GetData(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetData(ordinal);
			else
				return null;
		}

		public string GetDataTypeName(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetDataTypeName(ordinal);
			else
				return null;
		}

		public DateTime GetDateTime(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetDateTime(ordinal);
			else
				return new DateTime(0, 0, 0);
		}

		public decimal GetDecimal(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetDecimal(ordinal);
			else
				return 0;
		}

		public double GetDouble(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetDouble(ordinal);
			else
				return 0;
		}

		public Type GetFieldType(int ordinal)
		{
			return m_Reader.GetFieldType(ordinal);
		}

		public float GetFloat(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetFloat(ordinal);
			else
				return 0;
		}

		public Guid GetGuid(int ordinal)
		{
			return m_Reader.GetGuid(ordinal);
		}

		public short GetInt16(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetInt16(ordinal);
			else
				return 0;
		}

		public Int32 GetInt32(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetInt32(ordinal);
			else
				return 0;
		}

		public Int64 GetInt64(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetInt64(ordinal);
			else
				return 0;
		}

		public String GetName(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetName(ordinal);
			else
				return null;
		}

		public int GetOrdinal(String name)
		{
			if (m_Reader != null)
				return m_Reader.GetOrdinal(name);
			else
				return 0;
		}

		public DataTable GetSchemaTable()
		{
			if (m_Reader != null)
				return m_Reader.GetSchemaTable();
			else
				return null;
		}

		public String GetString(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetString(ordinal);
			else
				return null;
		}

		public object GetValue(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.GetValue(ordinal);
			else
				return null;
		}

		public int GetValues(object[] values)
		{
			if (m_Reader != null)
				return m_Reader.GetValues(values);
			else
				return 0;
		}


		Dictionary<String, int> _Columns = null;
		private Dictionary<String, int> Columns
		{
			get
			{
				if (_Columns == null)
				{
					_Columns = new Dictionary<string, int>();
					for (int i = 0; i < Reader.FieldCount; i++)
					{
						if (!_Columns.ContainsKey(Reader.GetName(i)))
							_Columns.Add(Reader.GetName(i), i);
						else
							Log.SysLogText(LogLevel.INFO, "Column Name '" + Reader.GetName(i) + "' Already Exists");
					}
				}
				return _Columns;
			}
		}

		public T Create<T>() where T : new()
		{
			T retVal = new T();
			if (retVal is ILoadable)
				((ILoadable)retVal).LoadFrom(this);
			else
				DataReaderConverter.LoadClassFromDataReader(retVal, this);
			return retVal;
		}


		public void Load<T>(T objToLoad)
		{

			if (objToLoad is ILoadable)
				((ILoadable)objToLoad).LoadFrom(this);
			else
				DataReaderConverter.LoadClassFromDataReader(objToLoad, this);
		}

		public static Dictionary<int, ConversionDelegate> ConversionDict = new Dictionary<int, ConversionDelegate>
		{
			{typeof(String).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetString(parm);})},
			{typeof(Int32).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetInt32(parm);})},
			{typeof(Int32?).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetNullableInt32(parm);})},
			{typeof(Int64).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetInt64(parm);})},
			{typeof(byte).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetByte(parm);})},
			{typeof(UInt64).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetUInt64(parm);})},
			{typeof(UInt32).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetUInt32(parm);})},
			{typeof(UInt32?).GetHashCode(),new ConversionDelegate((parm)=> { return DBUtil.GetNullableUInt32(parm);})},
			{typeof(UInt16).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetUInt16(parm);})},
			{typeof(Double).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetDouble(parm);})},
			{typeof(Double?).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetNullableDouble(parm);})},
			{typeof(Decimal).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetDecimal(parm);})},
			{typeof(byte[]).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetByteArray(parm);})},
			//{typeof(ulong).GetHashCode(),new ConversionDelegate((parm)=> {return (ulong)DBUtil.GetUInt64(parm);})},
			{typeof(bool).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetBoolean(parm);})},
			{typeof(DateTime).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetDateTime(parm);})},
			{typeof(DateTime?).GetHashCode(),new ConversionDelegate((parm)=> { return DBUtil.GetNullableDateTime(parm);})},
			{typeof(TimeSpan).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetTimeSpan(parm);})},
			//{typeof(AddressBase).GetHashCode(),new ConversionFunc((parm)=> {return DBUtil.GetDateTime(parm);})},
			{typeof(MACAddress).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetMACAddress(parm);})},
			{typeof(UUID).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetUUID(parm);})},
			{typeof(SoftwareVersion).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetSoftwareVersion(parm);})},
			{typeof(OperatingSystem).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetOperatingSystem(parm);})},
			{typeof(IPv4AddressPort).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetIPv4AddressPort(parm);})},
			{typeof(MimeType).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetMimeType(parm);})},
			{typeof(LogLevel).GetHashCode(),new ConversionDelegate((parm)=> {return DBUtil.GetLogLevel(parm);})},
		};


		public T GetValue<T>(int ordinal)
		{
			T retVal = default(T);
			ConversionDelegate f;
			if (Columns.Count > ordinal)
			{
				if (ConversionDict.TryGetValue(typeof(T).GetHashCode(), out f))
				{
					retVal = (T)f(Reader[ordinal]);
				}
				else
				{
					TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
					if (c != null)
					{
						if (!(c is EnumConverter))
						{
							f = c.ConvertFrom;
						}
						else
						{
							f = (new SimpleEnumConverter<T>()).ConvertFrom;
						}

						ConversionDict.Add(typeof(T).GetHashCode(), f);
						retVal = (T)f(Reader[ordinal]);
					}
					else
					{
						throw new Exception("Unknown Type specified");
					}
				}
			}
			return retVal;
		}


		public T GetValue<T>(string columnName)
		{
			T retVal = default(T);
			ConversionDelegate f;
			int ordinal;
			if (Columns.TryGetValue(columnName, out ordinal))
			{
				if (ConversionDict.TryGetValue(typeof(T).GetHashCode(), out f))
					retVal = (T)f(Reader[ordinal]);
				else
				{
					TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
					if (c != null)
					{
						if (!(c is EnumConverter))
							f = c.ConvertFrom;
						else
							f = (new SimpleEnumConverter<T>()).ConvertFrom;


						ConversionDict.Add(typeof(T).GetHashCode(), f);
						retVal = (T)f(Reader[ordinal]);
					}
					//                    else
					//                        throw new Exception("Unknown Type specified");
				}
			}
			return retVal;
		}

		public bool TryGetValue<T>(string columnName, out T val)
		{
			bool retVal = false;
			val = default(T);
			ConversionDelegate f;
			int ordinal;

			if (Columns.TryGetValue(columnName, out ordinal))
			{
				if (ConversionDict.TryGetValue(typeof(T).GetHashCode(), out f))
				{
					try
					{
						val = (T)f(Reader[ordinal]);
					}
					catch { }
				}
				else
				{
					TypeConverter c = TypeDescriptor.GetConverter(typeof(T));

					if (c != null)
					{
						f = c.ConvertFrom;
						ConversionDict.Add(typeof(T).GetHashCode(), f);

						try
						{
							val = (T)f(Reader[ordinal]);
						}
						catch { }
					}
				}

				retVal = true;
			}

			return retVal;
		}

		public bool IsDBNull(int ordinal)
		{
			if (m_Reader != null)
				return m_Reader.IsDBNull(ordinal);
			else
				return false;
		}

		public bool NextResult()
		{
			if ((m_Reader != null) && (m_Reader.NextResult()))
			{
				_Columns = null;
				_ResultSet++;
				return true;
			}
			else
				return false;
		}
		/// <summary>
		/// Implementation of IDisposable
		///		MPMP - No longer used
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this); // Stops finalizer from calling dispose, because this object has already freed its own maanaged objects. (Does this prevent destructor?)

		}

		protected virtual void Dispose(bool itIsSafeToAlsoFreeManagedObjects)
		{
			//itIsSafeToAlsoFreeManagedObjects = we are currently being disposed by the dispose interface, not by the destructor
			if (m_bWasDisposed)
				return;

			if (itIsSafeToAlsoFreeManagedObjects)
			{
				// Free any other managed objects here. 
				// close the reader and free it
				// Release the connection 
				try
				{
					this.Close();

				}
				catch (ObjectDisposedException e)
				{
					Log.SysLogText(LogLevel.ERROR, "ObjectDisposedException in {0} - {1}", this, e.Message);
				}
				catch (NullReferenceException nr)
				{
					Log.SysLogText(LogLevel.ERROR, "NullReferenceException in {0} - {1}", this, nr.Message);
				}

			}

			// Free any unmanaged objects here. 
			//
			m_bWasDisposed = true;
		}
		public DbDataReader Reader
		{
			get { return m_Reader; }
		}

		public bool Read()
		{
			return Reader.Read();
		}

		int _ResultSet = 0;
		public int ResultSet
		{
			get
			{
				return _ResultSet;
			}
		}

	}
}
