using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace KanoopCommon.Crypto
{
	public class Crc32 : HashAlgorithm
	{
		public const UInt32 DefaultPolynomial = 0xedb88320;
		public const UInt32 DefaultSeed = 0xffffffff;

		private UInt32 hash;
		private UInt32 seed;
		private UInt32[] table;
		private static UInt32[] defaultTable;

		public Crc32() 
		{
			table = InitializeTable(DefaultPolynomial);
			seed = DefaultSeed;
			Initialize();
		}

		public Crc32(UInt32 polynomial, UInt32 seed)
		{
			table = InitializeTable(polynomial);
			this.seed = seed;
			Initialize();
		}

		public override void Initialize()
		{
			hash = seed;
		}

		protected override void HashCore(byte[] buffer, int start, int length)
		{
			hash = CalculateHash(table, hash, buffer, start, length);
		}

		protected override byte[] HashFinal()
		{
			byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
			this.HashValue = hashBuffer;
			return hashBuffer;
		}

		public override int HashSize 
		{
			get { return 32;}
		}

		public static UInt32 Compute(String str)
		{
			return Compute(ASCIIEncoding.UTF8.GetBytes(str));
		}

		public static UInt32 Compute(byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
		}

		public static UInt32 Compute(byte[] buffer, int offset, int length)
		{
			return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, offset, length);
		}

		public static UInt32 Compute(UInt32 seed, byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
		}

		public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
		}

		private static UInt32[] InitializeTable(UInt32 polynomial)
		{
			if (polynomial == DefaultPolynomial && defaultTable != null)
				return defaultTable;

			UInt32[] createTable = new UInt32[256];
			for (int i = 0; i < 256; i++)
			{
				UInt32 entry = (UInt32)i;
				for (int j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ polynomial;
					else
						entry = entry >> 1;
				createTable[i] = entry;
			}

			if (polynomial == DefaultPolynomial)
				defaultTable = createTable;

			return createTable;
		}

		private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
		{
			UInt32 crc = seed;
			for (int i = start; i < start + size; i++)
				unchecked
				{
					crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
				}
			return crc;
		}

		private byte[] UInt32ToBigEndianBytes(UInt32 x)
		{
			return new byte[] {
				(byte)((x >> 24) & 0xff),
				(byte)((x >> 16) & 0xff),
				(byte)((x >> 8) & 0xff),
				(byte)(x & 0xff)
			};
		}
	}

	public class Crc16
	{
		const ushort polynomial = 0xA001;
		static ushort[] table = new ushort[256];

		public static ushort Compute(byte[] bytes, int start, int length) 
		{
			ushort crc = 0;
			for(int i = start; i < start + length; ++i) 
			{
				byte index = (byte)(crc ^ bytes[i]);
				crc = (ushort)((crc >> 8) ^ table[index]);
			}
			return crc;
		}

		public static ushort Compute(byte[] bytes) 
		{
			return Compute(bytes, 0, bytes.Length);
		}

		static Crc16() 
		{
			ushort value;
			ushort temp;
			for(ushort i = 0; i < table.Length; ++i) 
			{
				value = 0;
				temp = i;
				for(byte j = 0; j < 8; ++j) 
				{
					if(((value ^ temp) & 0x0001) != 0) 
					{
						value = (ushort)((value >> 1) ^ polynomial);
					}
					else 
					{
						value >>= 1;
					}
					temp >>= 1;
				}
				table[i] = value;
			}
		}
	}

}

	
