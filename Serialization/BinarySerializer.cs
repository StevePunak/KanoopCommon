using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Logging;
using KanoopCommon.Reflection;

namespace KanoopCommon.Serialization
{
	public static class BinarySerializer
	{
		static Dictionary<Type, int> _typeSizes;

		static BinarySerializer()
		{
			_typeSizes = new Dictionary<Type, int>()
			{
				{  typeof(bool),            sizeof(Int32) },
				{  typeof(char),            sizeof(char) },
				{  typeof(SByte),           sizeof(SByte) },
				{  typeof(byte),            sizeof(byte) },
				{  typeof(Int16),           sizeof(Int16) },
				{  typeof(UInt16),          sizeof(UInt16) },
				{  typeof(Int32),           sizeof(Int32) },
				{  typeof(UInt32),          sizeof(UInt32) },
				{  typeof(Int64),           sizeof(Int64) },
				{  typeof(UInt64),          sizeof(UInt64) },
				{  typeof(Double),          sizeof(Double) },
			};
		}

		public static byte[] Serialize(Object thing)
		{
			int totalSize;
			List<PropertyInfo> properties = GetSortedProperties(thing.GetType(), out totalSize);

			byte[] serialized = new byte[totalSize];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(serialized)))
			{
				foreach(PropertyInfo property in properties)
				{
					Object value = property.GetValue(thing);
					switch(Type.GetTypeCode(property.PropertyType))
					{
						case TypeCode.Boolean:
							bw.Write((Int32)(((bool)value) == true ? 1 : 0));
							break;
						case TypeCode.Char:
							bw.Write((Char)value);
							break;
						case TypeCode.Byte:
							bw.Write((Char)value);
							break;
						case TypeCode.SByte:
							bw.Write((SByte)value);
							break;
						case TypeCode.Int16:
							bw.Write((Int16)value);
							break;
						case TypeCode.UInt16:
							bw.Write((UInt16)value);
							break;
						case TypeCode.Int32:
							bw.Write((Int32)value);
							break;
						case TypeCode.UInt32:
							bw.Write((UInt32)value);
							break;
						case TypeCode.Int64:
							bw.Write((Int64)value);
							break;
						case TypeCode.UInt64:
							bw.Write((UInt64)value);
							break;
						case TypeCode.Double:
							bw.Write((Double)value);
							break;
					}
				}

			}
			return serialized;
		}

		public static T Deserialize<T>(byte[] from)
		{
			int totalSize;
			List<PropertyInfo> properties = GetSortedProperties(typeof(T), out totalSize);

			T thing = Activator.CreateInstance<T>();
			using(BinaryReader br = new BinaryReader(new MemoryStream(from)))
			{
				foreach(PropertyInfo property in properties)
				{
					int size;
					if(_typeSizes.TryGetValue(property.PropertyType, out size) == true && property.CanWrite)
					{
						Object value = null;
						switch(Type.GetTypeCode(property.PropertyType))
						{
							case TypeCode.Boolean:
								value = (br.ReadInt32() != 0) ? true : false;
								break;
							case TypeCode.Char:
								value = br.ReadChar();
								break;
							case TypeCode.Byte:
								value = br.ReadByte();
								break;
							case TypeCode.SByte:
								value = br.ReadByte();
								break;
							case TypeCode.Int16:
								value = br.ReadInt16();
								break;
							case TypeCode.UInt16:
								value = br.ReadUInt16();
								break;
							case TypeCode.Int32:
								value = br.ReadInt32();
								break;
							case TypeCode.UInt32:
								value = br.ReadUInt32();
								break;
							case TypeCode.Int64:
								value = br.ReadInt64();
								break;
							case TypeCode.UInt64:
								value = br.ReadUInt64();
								break;
							case TypeCode.Double:
								value = br.ReadDouble();
								break;
						}

						if(value != null)
						{
							property.SetValue(thing, value);
						}
					}
				}
			}

			return thing;
		}

		private static List<PropertyInfo> GetSortedProperties(Type type, out int totalSize)
		{
			totalSize = 0;
			List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties());
			properties.Sort(new PropertyNameComparer());

			foreach(PropertyInfo property in properties)
			{
				int size;
				if(_typeSizes.TryGetValue(property.PropertyType, out size) == true)
				{
					totalSize += size;
				}
				else
				{
					Log.SysLogText(LogLevel.ERROR, "Trying to serialize unsupported type {0}", property.PropertyType);
				}
			}

			return properties;
		}

	}

	class PropertyNameComparer : IComparer<PropertyInfo>
	{
		public int Compare(PropertyInfo x, PropertyInfo y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}
}
