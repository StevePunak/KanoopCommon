using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Conversions
{
	class BinaryConverter
	{
		public static string ByteArrayToString(byte[] data, int index, int length)
		{
			StringBuilder ret = new StringBuilder(length * 2);
			for(int x = index;x < index + length;x++)
			{
				ret.AppendFormat("{0:X2}", data[x]);
			}
			return ret.ToString();
		}

		public static string ByteArrayToString(byte[] data)
		{
			return ByteArrayToString(data, 0, data.Length);
		}

		public static string ByteArrayToString(byte[] data, int index)
		{
			return ByteArrayToString(data, index, data.Length - index);
		}


		public static byte[] StringToByteArray(String strData, int index, int length)
		{
			byte[] ret = new byte[length >> 1];
			int x, y;
			for(x = index, y = 0;x < index + length;x += 2, y++)
			{
				ret[y] = (byte)((CharToHexNibble(strData[x]) << 4) | CharToHexNibble(strData[x + 1]));
			}
			return ret;
		}

		public static byte[] StringToByteArray(String strData)
		{
			return StringToByteArray(strData, 0, strData.Length);
		}

		public static byte[] StringToByteArray(String strData, int index)
		{
			return StringToByteArray(strData, index, strData.Length - index);
		}

		public static byte CharToHexNibble(char c)
		{
			byte ret = 0;

			if(c >= '0' && c <= '9')
				ret = (byte)(c - '0');
			else if(c >= 'A' && c <= 'F')
				ret = (byte)(c - 'A' + 10);
			else if(c >= 'a' && c <= 'f')
				ret = (byte)(c - 'a' + 10);

			return ret;

		}
		public static bool ByteArrayCompare(byte[] a, byte[] b)
		{
			bool result = false;
			if(a.Length == b.Length)
			{
				result = ByteArrayCompare(a, 0, b, 0, a.Length);
			}
			return result;
		}

		public static bool ByteArrayCompare(byte[] a, byte[] b, int nLength)
		{
			return ByteArrayCompare(a, 0, b, 0, nLength);
		}

		public static bool ByteArrayCompare(byte[] a, int nOffset1, byte[] b, int nOffset2, int nLength)
		{
			int x;
			for(x = 0;x < nLength && a[nOffset1 + x] == b[nOffset2 + x];x++);
			return x < nLength ? false : true;
		}
        public static void UInt16ToByteArrayNetOrder(UInt16 value, ref byte[] packet, int index)
        {
            byte[] ret = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                packet[index] = ret[1];
                packet[index + 1] = ret[0];
            }
            else
            {
                packet[index] = ret[0];
                packet[index + 1] = ret[1];
            }
        }

        public static void UInt32ToByteArrayHostOrder(UInt32 uInt, ref byte[] packet, int index)
        {
            byte[] temp = BitConverter.GetBytes(uInt);
            if (!BitConverter.IsLittleEndian)
            {
                packet[index] = temp[3];
                packet[index + 1] = temp[2];
                packet[index + 2] = temp[1];
                packet[index + 3] = temp[0];
            }
            else
            {
                packet[index] = temp[0];
                packet[index + 1] = temp[1];
                packet[index + 2] = temp[2];
                packet[index + 3] = temp[3];
            }
        }

	}
}
