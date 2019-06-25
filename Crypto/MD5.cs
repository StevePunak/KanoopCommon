using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace KanoopCommon.Crypto
{
	public class MD5
	{

        public static string HashAsString(String data)
        {
            byte[] dataBytes = ASCIIEncoding.UTF8.GetBytes(data);
            return HashAsString(dataBytes);
        }

		public static String HashAsString(byte[] data)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			byte[] sum = md5.ComputeHash(data);

			return HashedOutput(sum);
		}

		public static String HashAsString(UInt32 data)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			byte[] sum = md5.ComputeHash(ASCIIEncoding.UTF8.GetBytes(String.Format("{0}", data)));

			return HashedOutput(sum);
		}

        public static UInt32 HashStringAsUInt32(String data)
        {
			/** hash the string, and OR in each 32-bit value */
			UInt32 result = 0;

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			byte[] bytes = md5.ComputeHash(ASCIIEncoding.UTF8.GetBytes(String.Format("{0}", data)));
			using(MemoryStream ms = new MemoryStream(bytes))
			using(BinaryReader br = new BinaryReader(ms))
			{
				for(int x = 0;x < 4;x++)
				{
					result |= br.ReadUInt32();
				}
			}

            return result;
        }

		public static byte[] HashAsByteArray(String data)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			return md5.ComputeHash(ASCIIEncoding.UTF8.GetBytes(String.Format("{0}", data)));
		}

		public static byte[] HashAsByteArray(byte[] data)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			return md5.ComputeHash(data);
		}

		static String HashedOutput(byte[] sum)
		{
			StringBuilder strRet = new StringBuilder(32);
			foreach(byte b in sum)
			{
				strRet.AppendFormat("{0:x2}", b);
			}
			return strRet.ToString();
		}

		public static String ComputeFileHash(String fileName)
		{
			MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
			byte[] bytes = null;
			using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				bytes = provider.ComputeHash(fs);
			}
			return HashedOutput(bytes);
		}
	}
}
