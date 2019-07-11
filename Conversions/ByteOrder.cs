using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Conversions
{
	public static class ByteOrder
	{
		public static UInt16 NetworkToHost(UInt16 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (UInt16)IPAddress.NetworkToHostOrder((Int16)value);
		}
		public static UInt16 HostToNetwork(UInt16 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (UInt16)IPAddress.HostToNetworkOrder((Int16)value);
		}
		public static Int16 NetworkToHost(Int16 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (Int16)IPAddress.NetworkToHostOrder((Int16)value);
		}
		public static Int16 HostToNetwork(Int16 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (Int16)IPAddress.HostToNetworkOrder((Int16)value);
		}
		public static UInt32 NetworkToHost(UInt32 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (UInt32)IPAddress.NetworkToHostOrder((Int32)value);
		}
		public static UInt32 HostToNetwork(UInt32 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (UInt32)IPAddress.HostToNetworkOrder((Int32)value);
		}
		public static Int32 NetworkToHost(Int32 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (Int32)IPAddress.NetworkToHostOrder((Int32)value);
		}
		public static Int32 HostToNetwork(Int32 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (Int32)IPAddress.HostToNetworkOrder((Int32)value);
		}
		public static UInt64 NetworkToHost(UInt64 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (UInt64)IPAddress.NetworkToHostOrder((Int64)value);
		}
		public static UInt64 HostToNetwork(UInt64 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (UInt64)IPAddress.HostToNetworkOrder((Int64)value);
		}
		public static Int64 NetworkToHost(Int64 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (Int64)IPAddress.NetworkToHostOrder((Int64)value);
		}
		public static Int64 HostToNetwork(Int64 value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			return (Int64)IPAddress.HostToNetworkOrder((Int64)value);
		}
		public static Double NetworkToHost(Double value)
		{
			if(BitConverter.IsLittleEndian == false)
				return value;
			byte[] rangeBytes = BitConverter.GetBytes(value);
			byte[] newArray = new byte[8]
								{
									rangeBytes[7],
									rangeBytes[6],
									rangeBytes[5],
									rangeBytes[4],
									rangeBytes[3],
									rangeBytes[2],
									rangeBytes[1],
									rangeBytes[0],
								};
			return BitConverter.ToDouble(newArray, 0);
		}
		public static Double HostToNetwork(Double value)
		{
			return NetworkToHost(value);
		}
	}
}