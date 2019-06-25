using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Encoding
{
	public class UTF8
	{
		public static String Decode(byte[] buffer)
		{
			String value = String.Empty;
			using(BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
			{
				UInt16 length = (UInt16)IPAddress.NetworkToHostOrder((Int16)br.ReadUInt16());
				value = ASCIIEncoding.UTF8.GetString(br.ReadBytes(length));
			}
			return value;
		}

		public static List<String> DecodeStrings(byte[] buffer)
		{
			List<String> values = new List<string>();
			using(BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
			{
				do
				{
					UInt16 length = (UInt16)IPAddress.NetworkToHostOrder((Int16)br.ReadUInt16());
					String value = ASCIIEncoding.UTF8.GetString(br.ReadBytes(length));
					values.Add(value);
				} while(br.BaseStream.Position < br.BaseStream.Length);

			}
			return values;
		}

		public static byte[] Encode(String value)
		{
			byte[] buffer = new byte[value.Length + 2];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(buffer)))
			{
				bw.Write((UInt16)(IPAddress.HostToNetworkOrder((Int16)value.Length)));
				bw.Write(value.ToCharArray(), 0, value.Length);
			}
			return buffer;
		}

		public static byte[] Encode(List<String> values)
		{
			byte[] buffer = new byte[Length(values)];
			using(BinaryWriter bw = new BinaryWriter(new MemoryStream(buffer)))
			{
				foreach(String value in values)
				{
					bw.Write((UInt16)(IPAddress.HostToNetworkOrder((Int16)value.Length)));
					bw.Write(value.ToCharArray(), 0, value.Length);
				}
			}
			return buffer;
		}

		public static String ReadEncoded(BinaryReader br)
		{
			int length = IPAddress.NetworkToHostOrder((Int16)br.ReadUInt16());
			String value = ASCIIEncoding.UTF8.GetString(br.ReadBytes(length));
			return value;
		}

		public static int Length(String value)
		{
			if(value == null)
				return 0;
			else
				return 2 + value.Length;
		}

		public static int Length(List<String> values)
		{
			int length = values.Sum(v => v.Length) + (values.Count * 2);
			return length;
		}

	}
}
