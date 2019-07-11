using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
	public static class ByteArrayExtensions
	{

		/*
		public static void MapHttpHandler<THandler>(this RouteCollection routes,
		  string url) where THandler : IHttpHandler, new()
		{
			routes.MapHttpHandler<THandler>(null, url, null, null);
		}*/
		public static String ToDebugString(this byte []  bytes)
		{
			return BitConverter.ToString(bytes);
		}
		public static String ToHexString(this byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			foreach(byte b in bytes)
			{
				sb.AppendFormat("{0:X2} ", b);
			}
			return sb.ToString().Trim();
		}
		public static byte [] GetChunk(this byte[] bytes,uint offset, uint length)
		{
			byte[] retVal = new byte[length];
			Array.Copy(bytes, offset, retVal, 0, length);
			return retVal;
		}
		public static byte[] GetChunk(this byte[] bytes, int offset, int length)
		{
			return GetChunk(bytes, (uint)offset, (uint)length);
		}
		public static byte[] GetChunk(this byte[] bytes, int offset)
		{
			return GetChunk(bytes, (uint)offset, (uint)(bytes.Length-offset));
		}

		public static bool TryGetAsciiLineStartingWith(this byte[] self, String startString, out string line)
		{
			byte[] candidate = ASCIIEncoding.UTF8.GetBytes(startString);

			bool retVal = false;
			line = null;
			int idx = IndexOf(self, candidate, 0);
			if (idx >= 0)
			{
				int lineEnd = IndexOf(self, new byte[]{0x0d},idx);
				if (lineEnd< 0)
					lineEnd = self.Length;
				line = ASCIIEncoding.UTF8.GetString(GetChunk(self, idx, lineEnd - idx));
				retVal = true;
			}
			return retVal;
		}

		public static void Populate(this byte[] arr, byte value)
		{
			for(int i = 0;i < arr.Length;i++)
			{
				arr[i] = value;
			}
		}

		public static bool IsAll(this byte[] arr, byte value)
		{
			int i;
			for(i = 0;i < arr.Length && arr[i] == value;i++) ;
			return i == arr.Length;
		}

		public static bool Contains(this byte[] haystack, byte[] needle)
		{
			return IndexOf(haystack, needle, 0) >= 0;
		}

		public static int IndexOf(this byte[] haystack, byte needle, int startIndex)
		{
			return Array.IndexOf(haystack, needle, startIndex);
		}

		public static int IndexOf(this byte[] haystack, byte[] needle)
		{
			return IndexOf(haystack, needle, 0);
		}
		public static int IndexOf(this byte[] haystack, byte[] needle, int startIndex)
		{
			int retVal = -1;
			if (!IsEmptyLocate(haystack, needle))
			{

				for (int i = startIndex; i < haystack.Length; i++)
				{
					if (!IsMatch(haystack, i, needle))
						continue;

					retVal = i;
					break;
				}
			}
			return retVal;
		}

		static bool IsMatch(byte[] haystack, int index, byte[] needle)
		{
			if (needle.Length > (haystack.Length - index))
				return false;

			for (int i = 0; i < needle.Length; i++)
				if (haystack[index + i] != needle[i])
					return false;

			return true;
		}

		static bool IsEmptyLocate(byte[] haystack, byte[] needle)
		{
			return haystack == null
					|| needle == null
					|| haystack.Length == 0
					|| needle.Length == 0
					|| needle.Length > haystack.Length;
		}

		public static String ToCSharpByteArray(this byte[] array, String variableName)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("byte[] {0} = new byte[]\n{{\n", variableName);
			for(int x = 0;x < array.Length;x += 8)
			{
				sb.AppendFormat("\t");
				for(int y = x;y < array.Length && y < x + 8;y++)
				{
					sb.AppendFormat("0x{0:x2}, ", array[y]);
				}
				sb.AppendFormat("\n");
			}
			sb.AppendFormat("}};");
			return sb.ToString();
		}

		public static String ToMySqlString(this byte[] array, String emptyValue="null")
		{
			String retVal = array.ToHexString();
			if (retVal.Length>0)
				retVal = "0x"+retVal;
			else
				retVal = emptyValue;
			return retVal;
		}

		public static String ToHexString(this byte[] array, bool zeroPad = true)
		{
			StringBuilder sb = new StringBuilder(array.Length * 3);
			foreach(byte b in array)
			{
				if(zeroPad)
					sb.AppendFormat("{0:x2} ", b);
				else
					sb.AppendFormat("{0:x} ", b);
			}
			return sb.ToString().Trim();
		}

		public static byte[] Combine(byte[] first, byte[] second)
		{
			byte[] ret = new byte[first.Length + second.Length];
			Buffer.BlockCopy(first, 0, ret, 0, first.Length);
			Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
			return ret;
		}

		public static bool EqualTo(this byte[] mine, byte[] other)
		{
			if(mine.Length != other.Length)
				return false;
			for(int x = 0;x < other.Length;x++)
			{
				if(mine[x] != other[x])
				{
					return false;
				}
			}
			return true;
		}

		public static Byte[] FromText(String text)
		{
			String[] parts = text.Split(' ');
			byte[] ret = new byte[parts.Length];
			for(int x = 0;x < parts.Length;x++)
			{
				if(!byte.TryParse(parts[x], NumberStyles.HexNumber, null, out ret[x]))
				{
					break;
				}
			}
			return ret;
		}

		public static Byte[] FromText(String[] parts, int offset)
		{
			List<byte> ret = new List<byte>();
			for(int x = offset;x < parts.Length;x++)
			{
				byte b;
				if(!byte.TryParse(parts[x], NumberStyles.HexNumber, null, out b))
				{
					break;
				}
				ret.Add(b);
			}
			return ret.ToArray();
		}
	}
}
