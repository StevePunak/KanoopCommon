using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using KanoopCommon.Conversions;
using System.IO;
using System.Net;
using KanoopCommon.CommonObjects;
using KanoopCommon.Serialization;
using Newtonsoft.Json;

namespace KanoopCommon.Addresses
{

	public class UUIDList : List<UUID>{}

	[JsonConverter(typeof(UUIDJsonConverter))]
	[TypeConverter(typeof(UUIDConverter))]
	public class UUID : IComparable
	{
		const int GUID_STRING_LENGTH =				36;
		const int STRING_FIELD_0_INDEX =			0;
		const int STRING_FIELD_1_INDEX =			9;
		const int STRING_FIELD_2_INDEX =			14;
		const int STRING_FIELD_3_INDEX =			19;
		const int STRING_FIELD_4_INDEX =			24;

		const int GUID_BYTE_ARRAY_LENGTH =			16;
		const int BYTE_FIELD_0_INDEX =				0;
		const int BYTE_FIELD_1_INDEX =				4;
		const int BYTE_FIELD_2_INDEX =				6;
		const int BYTE_FIELD_3_INDEX =				8;
		const int BYTE_FIELD_4_INDEX =				10;

		const int BYTE_FIELD_0_LENGTH =				4;
		const int BYTE_FIELD_1_LENGTH =				2;
		const int BYTE_FIELD_2_LENGTH =				2;
		const int BYTE_FIELD_3_LENGTH =				2;
		const int BYTE_FIELD_4_LENGTH =				6;

		protected byte[]									m_UUID;

		public static int Sizeof { get { return GUID_BYTE_ARRAY_LENGTH; } }

		public byte[] Field0
		{
			get
			{
				byte[] section = new byte[BYTE_FIELD_0_LENGTH];
				Buffer.BlockCopy(m_UUID, BYTE_FIELD_0_INDEX, section, 0, BYTE_FIELD_0_LENGTH);
				return section;
			}
			set
			{
				if(value.Length != BYTE_FIELD_0_LENGTH)
				{
					throw new CommonException("Invalid length passed to UUID byte field");
				}
				Buffer.BlockCopy(value, 0, m_UUID, BYTE_FIELD_0_INDEX, BYTE_FIELD_0_LENGTH);
				ParsePropertyValues();
			}
		}

		public byte[] Field1
		{
			get
			{
				byte[] section = new byte[BYTE_FIELD_1_LENGTH];
				Buffer.BlockCopy(m_UUID, BYTE_FIELD_1_INDEX, section, 0, BYTE_FIELD_1_LENGTH);
				return section;
			}
			set
			{
				if(value.Length != BYTE_FIELD_1_LENGTH)
				{
					throw new CommonException("Invalid length passed to UUID byte field");
				}
				Buffer.BlockCopy(value, 0, m_UUID, BYTE_FIELD_1_INDEX, BYTE_FIELD_1_LENGTH);
				ParsePropertyValues();
			}
		}

		public byte[] Field2
		{
			get
			{
				byte[] section = new byte[BYTE_FIELD_2_LENGTH];
				Buffer.BlockCopy(m_UUID, BYTE_FIELD_2_INDEX, section, 0, BYTE_FIELD_2_LENGTH);
				return section;
			}
			set
			{
				if(value.Length != BYTE_FIELD_2_LENGTH)
				{
					throw new CommonException("Invalid length passed to UUID byte field");
				}
				Buffer.BlockCopy(value, 0, m_UUID, BYTE_FIELD_2_INDEX, BYTE_FIELD_2_LENGTH);
				ParsePropertyValues();
			}
		}

		public byte[] Field3
		{
			get
			{
				byte[] section = new byte[BYTE_FIELD_3_LENGTH];
				Buffer.BlockCopy(m_UUID, BYTE_FIELD_3_INDEX, section, 0, BYTE_FIELD_3_LENGTH);
				return section;
			}
			set
			{
				if(value.Length != BYTE_FIELD_3_LENGTH)
				{
					throw new CommonException("Invalid length passed to UUID byte field");
				}
				Buffer.BlockCopy(value, 0, m_UUID, BYTE_FIELD_3_INDEX, BYTE_FIELD_3_LENGTH);
				ParsePropertyValues();
			}
		}

		public byte[] Field4
		{
			get
			{
				byte[] section = new byte[BYTE_FIELD_4_LENGTH];
				Buffer.BlockCopy(m_UUID, BYTE_FIELD_4_INDEX, section, 0, BYTE_FIELD_4_LENGTH);
				return section;
			}
			set
			{
				if(value.Length != BYTE_FIELD_4_LENGTH)
				{
					throw new CommonException("Invalid length passed to UUID byte field");
				}
				Buffer.BlockCopy(value, 0, m_UUID, BYTE_FIELD_4_INDEX, BYTE_FIELD_4_LENGTH);
				ParsePropertyValues();
			}
		}

		UInt64 m_FirstUInt64;
		public UInt64 FirstUInt64 { get { return m_FirstUInt64; } }

		UInt64 m_SecondUInt64;
		public UInt64 SecondUInt64 { get { return m_SecondUInt64; } }

		public UInt64 Hash64 { get { return m_FirstUInt64 ^ m_SecondUInt64; } }

		UInt32 m_nHash32;


		/** 0         1         2         3      */
		/** 012345678901234567890123456789012345 */
		/** 38a52be4-9352-453e-af97-5c3b448652f0 */
		public UUID(String guid)
		{
			if(guid.Length != GUID_STRING_LENGTH)
			{
				throw new Exception("GUID String of an invalid length");
			}

			byte[] a = BinaryConverter.StringToByteArray(guid, STRING_FIELD_0_INDEX, 8);
			byte[] b = BinaryConverter.StringToByteArray(guid, STRING_FIELD_1_INDEX, 4);
			byte[] c = BinaryConverter.StringToByteArray(guid, STRING_FIELD_2_INDEX, 4);
			byte[] d = BinaryConverter.StringToByteArray(guid, STRING_FIELD_3_INDEX, 4);
			byte[] e = BinaryConverter.StringToByteArray(guid, STRING_FIELD_4_INDEX, 12);

			m_UUID = new byte[GUID_BYTE_ARRAY_LENGTH];
			Array.Copy(a, 0, m_UUID, BYTE_FIELD_0_INDEX, a.Length);
			Array.Copy(b, 0, m_UUID, BYTE_FIELD_1_INDEX, b.Length);
			Array.Copy(c, 0, m_UUID, BYTE_FIELD_2_INDEX, c.Length);
			Array.Copy(d, 0, m_UUID, BYTE_FIELD_3_INDEX, d.Length);
			Array.Copy(e, 0, m_UUID, BYTE_FIELD_4_INDEX, e.Length);

			ParsePropertyValues();
		}

		public UUID(byte[] guid)
		{
			m_UUID = guid;
			ParsePropertyValues();
		}

		public UUID(byte[] array, int offset)
		{
			m_UUID = new byte[GUID_BYTE_ARRAY_LENGTH];
			Array.Copy(array, offset, m_UUID, 0, m_UUID.Length);
			ParsePropertyValues();
		}

		public UUID()
			: this(true) {}

		public UUID(bool bNewUUID)
		{
			m_UUID = new byte[GUID_BYTE_ARRAY_LENGTH];
			if(bNewUUID)
			{
				CreateNew();
			}
		}

		public static UUID EmptyUUID
		{
			get
			{
				UUID ret = new UUID(false);
				return ret;
			}
		}

		public static bool TryParse(String strValue, out UUID value)
		{
			value = null;
			try
			{
				value = new UUID(strValue);
			}
			catch(Exception)
			{
				value = null;
			}
			return value != null;
		}

		public static bool IsValid(String str)
		{
			String strEmptyUUID = new UUID(false).ToString();
			int x = 0;
			if(str.Length == strEmptyUUID.Length)
			{
				for(;x < strEmptyUUID.Length;x++)
				{
					if(strEmptyUUID[x] == '0' && IsHexDigit(str[x]) == false)
						break;
				}
			}
			return x == strEmptyUUID.Length;
		}

		public void Clear()
		{
			Array.Clear(m_UUID, 0, m_UUID.Length);
		}

		public UUID Clone()
		{
			UUID ret = new UUID(m_UUID);
			return ret;
		}

		public int CompareTo(Object other)
		{
			if(other is UUID == false)
			{
				throw new CommonException("Object is not UUID");
			}

			int result;

			if((result = m_FirstUInt64.CompareTo(((UUID)other).m_FirstUInt64)) == 0)
			{
				result = m_SecondUInt64.CompareTo(((UUID)other).m_SecondUInt64);
			}

			return result;
		}

		public override bool Equals(object other)
		{
			bool ret = false;

			if(other is UUID)
			{
				ret = BinaryConverter.ByteArrayCompare(((UUID)other).m_UUID, m_UUID);
			}
			else if(other is String)
			{
				ret = this.ToString() == (String)other;
			}
			else
			{
				throw new Exception("'OTHER' object is not a UUID or string");
			}
			return ret;
		}
		
		public override int GetHashCode()
		{
			return (int)m_nHash32;
		}

		/** 0         1         2         3      */
		/** 012345678901234567890123456789012345 */
		/** 38a52be4-9352-453e-af97-5c3b448652f0 */
		public override String ToString()
		{
			return String.Format("{0}-{1}-{2}-{3}-{4}",
				BinaryConverter.ByteArrayToString(m_UUID, BYTE_FIELD_0_INDEX, 4),
				BinaryConverter.ByteArrayToString(m_UUID, BYTE_FIELD_1_INDEX, 2),
				BinaryConverter.ByteArrayToString(m_UUID, BYTE_FIELD_2_INDEX, 2),
				BinaryConverter.ByteArrayToString(m_UUID, BYTE_FIELD_3_INDEX, 2),
				BinaryConverter.ByteArrayToString(m_UUID, BYTE_FIELD_4_INDEX, 6));
			
		}

		public void CreateNew()
		{
			m_UUID = System.Guid.NewGuid().ToByteArray();
			ParsePropertyValues();
		}

		static bool IsHexDigit(char c)
		{
			return (	
				(c >= 'a' && c <= 'f') ||
				(c >= 'A' && c <= 'F') ||
				(c >= '0' && c <= '9')
				);
		}

		void ParsePropertyValues()
		{
			BinaryReader br = new BinaryReader(new MemoryStream(m_UUID));
			m_FirstUInt64 = (UInt64)IPAddress.HostToNetworkOrder((long)br.ReadUInt64());
			m_SecondUInt64 = (UInt64)IPAddress.HostToNetworkOrder((long)br.ReadUInt64());

			UInt64 i1 = m_FirstUInt64 >> 32;
			UInt64 i2 = m_FirstUInt64 & 0x00000000FFFFFFFF;
			UInt64 i3 = m_SecondUInt64 >> 32;
			UInt64 i4 = m_SecondUInt64 & 0x00000000FFFFFFFF;

			m_nHash32 = (UInt32)(i1 ^ i2 ^ i3 ^ i4);
		}

	}
	public class UUIDJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			UUID user = (UUID)value;

			writer.WriteValue(user.ToString());
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			UUID uuid;
			if(String.IsNullOrEmpty((string)reader.Value))
			{
				uuid = UUID.EmptyUUID;
			}
			else
			{
				uuid = new UUID((string)reader.Value);
			}
			return uuid;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(UUID);
		}
	}
}

