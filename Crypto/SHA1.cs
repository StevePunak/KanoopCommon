using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace KanoopCommon.Crypto
{
	public class SHA1
	{

        public static string HashAsString(String data)
        {
            byte[] dataBytes = ASCIIEncoding.UTF8.GetBytes(data);
            return HashAsString(dataBytes);
        }

		public static String HashAsString(byte[] data)
		{
			SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
			byte[] sum = sha1.ComputeHash(data);

			return HashedOutput(sum);
		}

		public static String HashAsString(UInt32 data)
		{
			SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
			byte[] sum = sha1.ComputeHash(ASCIIEncoding.UTF8.GetBytes(String.Format("{0}", data)));

			return HashedOutput(sum);
		}

        public static UInt32 HashStringAsUInt32(String data)
        {
			/** hash the string, and OR in each 32-bit value */
			UInt32 result = 0;

			SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
			byte[] bytes = sha1.ComputeHash(ASCIIEncoding.UTF8.GetBytes(String.Format("{0}", data)));
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
			SHA1CryptoServiceProvider disgest = new SHA1CryptoServiceProvider();
			return disgest.ComputeHash(ASCIIEncoding.UTF8.GetBytes(String.Format("{0}", data)));
		}

		public static byte[] HashAsByteArray(byte[] data)
		{
			SHA1CryptoServiceProvider digest = new SHA1CryptoServiceProvider();
			return digest.ComputeHash(data);
		}

		static String HashedOutput(byte[] sum)
		{
			StringBuilder ret = new StringBuilder(32);
			foreach(byte b in sum)
			{
				ret.AppendFormat("{0:x2}", b);
			}
			return ret.ToString();
		}

		public static String ComputeFileHash(String fileName)
		{
			SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
			byte[] bytes = null;
			using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				bytes = provider.ComputeHash(fs);
			}
			return HashedOutput(bytes);
		}

		public static bool ComputeFileHash(String fileName, out byte[] bytes)
		{
			SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
			bytes = null;
			using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				bytes = provider.ComputeHash(fs);
			}
			return true;
		}
	}
}
