using KanoopCommon.Conversions;
using KanoopCommon.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System;
using KanoopCommon.Crypto;

namespace KanoopCommon.Addresses
{
	[TypeConverterAttribute(typeof(AddressConverter))]
	public class MACAddress : AddressBase, IComparable
	{
		const int				MAC_STD_FORMAT_LENGTH =	17;
		const int				MAC_NO_COLON_FORMAT_LENGTH = 12;
		const int				MAC_OXO_NOCOLON_FORMAT_LENGTH =	14;

		protected const String	EMPTY_MAC_ADDRESS =	"00:00:00:00:00:00";

		protected byte[]		m_BinaryAddress = new byte[AddressLengths.MAC];
		protected UInt64		m_IntegerAddress = 0;

		public override int Length
		{
			get { return AddressLengths.MAC; }
		}

		public override String Address
		{
			get { return _address; }
			set { ParseIntoAddress(value); }
		}

		public override byte[]	AddressAsByteArray
		{
			get { return m_BinaryAddress; }
			set
			{
				Array.Copy(value, m_BinaryAddress, AddressLengths.MAC);
				_address = String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}",
				                          m_BinaryAddress[0],
				                          m_BinaryAddress[1],
				                          m_BinaryAddress[2],
				                          m_BinaryAddress[3],
				                          m_BinaryAddress[4],
				                          m_BinaryAddress[5]);
				_type = AddressType.MAC;
			}
		}

		public OUN OUN
		{
			get
			{
				UInt32 oun = (UInt32)(AddressAsUInt64 >> 32);
				return new OUN(oun);
			}
		}
		public UInt16 FirstWord  { get { return (UInt16)IPAddress.NetworkToHostOrder((Int16)BitConverter.ToUInt16(m_BinaryAddress, 0)); } }
		public UInt16 SecondWord { get { return (UInt16)IPAddress.NetworkToHostOrder((Int16)BitConverter.ToUInt16(m_BinaryAddress, 2)); } }
		public UInt16 ThirdWord { get { return (UInt16)IPAddress.NetworkToHostOrder((Int16)BitConverter.ToUInt16(m_BinaryAddress, 4)); } }
		public UInt32 First24 { get { UInt32 ret = (UInt32)m_BinaryAddress[0] << 16 | (UInt32)m_BinaryAddress[1] << 8 | (UInt32)m_BinaryAddress[2]; return ret; } }
		public UInt32 Last24 { get { UInt32 ret = (UInt32)m_BinaryAddress[3] << 16 | (UInt32)m_BinaryAddress[4] << 8 | (UInt32)m_BinaryAddress[5]; return ret; } }

		public UInt64 AddressAsUInt64
		{
			get
			{
				if(m_IntegerAddress == 0)
				{
					m_IntegerAddress = ToUInt64();
				}
				return m_IntegerAddress;
			}
		}

		public override bool IsValid()
		{
			return (_address.Length == MAC_STD_FORMAT_LENGTH);
		}

		/**
		 * @b          AddressBase
		 *             Constructor
		 */
		public MACAddress(byte[] inArray)
		{
			this.AddressAsByteArray = inArray;
		}

		public MACAddress(byte[] inArray, int nOffset)
		{
			byte[] addr = new byte[AddressLengths.MAC];
			Array.Copy(inArray, nOffset, addr, 0, AddressLengths.MAC);
			this.AddressAsByteArray = addr;
		}

		public MACAddress(String strAddress)
		{
			ParseIntoAddress(strAddress);
		}

		public MACAddress(UInt64 address)
		{
			ParseUInt64(address);
		}

		public MACAddress(MACAddress address)
			: this(address.ToString()) {}

		public MACAddress(PhysicalAddress address)
			: this(address.ToString()) {}

		public UInt64 ToUInt64()
		{
			if(m_IntegerAddress == 0)
			{
				byte[] binaryAddr = new byte[8];
				Array.Copy(m_BinaryAddress, 0, binaryAddr, 2, AddressLengths.MAC);
				Array.Reverse(binaryAddr);
				m_IntegerAddress = BitConverter.ToUInt64(binaryAddr, 0);
				//            Console.Out.WriteLine("Address out is " + unitAddress);
			}
			return m_IntegerAddress;
		}

		public MACAddress()
		{
			_address = EMPTY_MAC_ADDRESS;
			_type = AddressType.MAC;
		}

		void ParseIntoAddress(String strAddress)
		{
			strAddress = strAddress.Trim();
			if(strAddress.Length == 0)
			{
				strAddress = EMPTY_MAC_ADDRESS;
				_type = AddressType.MAC;
			}

			/** Format: 000102030405 */
			else if(strAddress.Length == MAC_NO_COLON_FORMAT_LENGTH)
			{
				int x;

				for(x = 0; x < strAddress.Length && Uri.IsHexDigit(strAddress[x]); x++) ;
				if(x < strAddress.Length)
				{
					throw new Exception("MAC Address Format Exception");
				}
				strAddress = String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
				                           strAddress.Substring(0, 2),
				                           strAddress.Substring(2, 2),
				                           strAddress.Substring(4, 2),
				                           strAddress.Substring(6, 2),
				                           strAddress.Substring(8, 2),
				                           strAddress.Substring(10, 2));
				_type = AddressType.MAC;
			}
			/** Format: 00:01:02:03:04:05 */
			else if(strAddress.Length == MAC_STD_FORMAT_LENGTH)
			{
				strAddress = strAddress.Replace('-', ':');

				int x;

				for(x = 0; x < strAddress.Length; x++)
				{
					if((x + 1) % 3 == 0)
					{
						if(strAddress[x] != ':')
						{
							throw new Exception("MAC Address Format Exception");
						}
					}
					else if(!Uri.IsHexDigit(strAddress[x]))
					{
						throw new Exception("MAC Address Format Exception");
					}
				}
				_type = AddressType.MAC;
			}
			/** Format: 0x000102030405 */
			else if(strAddress.Length == MAC_OXO_NOCOLON_FORMAT_LENGTH)
			{
				int x;

				for(x = 2; x < strAddress.Length && Uri.IsHexDigit(strAddress[x]); x++) ;
				if(x < strAddress.Length || strAddress[0] != '0' || strAddress[1] != 'x')
				{
					throw new Exception("MAC Address Format Exception");
				}
				strAddress = String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
				                           strAddress.Substring(2, 2),
				                           strAddress.Substring(4, 2),
				                           strAddress.Substring(5, 2),
				                           strAddress.Substring(8, 2),
				                           strAddress.Substring(10, 2),
				                           strAddress.Substring(12, 2));
			}
			else
			{
				strAddress = EMPTY_MAC_ADDRESS;
				_type = AddressType.MAC;
			}

			_address = strAddress.ToUpper();

			LoadAddressAsByteArray();
		}

		/** check address validity with no exceptions */
		public static bool IsValid(String address)
		{
			address = address.Trim();
			bool bRet = false;
			if(address == null)
			{
				/** leave it bad */
			}
			/** Format: 000102030405 */
			else if(address.Length == MAC_NO_COLON_FORMAT_LENGTH)
			{
				int x;

				for(x = 0; x < address.Length && Uri.IsHexDigit(address[x]); x++) ;
				if(x == address.Length)
				{
					bRet = true;
				}
			}

			/** Format: 00:01:02:03:04:05 */
			else if(address.Length == MAC_STD_FORMAT_LENGTH)
			{
				int x;

				for(x = 0; x < address.Length; x++)
				{
					if((x + 1) % 3 == 0)
					{
						if(address[x] != ':')
						{
							break;
						}
					}
					else if(!Uri.IsHexDigit(address[x]))
					{
						break;
					}
				}

				bRet = x == address.Length;
			}
			/** Format: 0x000102030405 */
			else if(address.Length == MAC_OXO_NOCOLON_FORMAT_LENGTH)
			{
				int x;

				for(x = 2; x < address.Length && Uri.IsHexDigit(address[x]); x++) ;
				if(x < address.Length || address[0] != '0' || address[1] != 'x')
				{
					/** */
				}
				else
				{
					bRet = true;
				}
			}

			return bRet;
		}

		public bool IsEmpty
		{
			get { return Address.Equals(EMPTY_MAC_ADDRESS); }
		}

		public static MACAddress Empty { get { return new MACAddress(); } }

		public new MACAddress Clone()
		{
			MACAddress ret = new MACAddress();
			ret._address = _address;
			ret.m_BinaryAddress = (byte[])m_BinaryAddress.Clone();
			return ret;
		}

		void ParseUInt64(UInt64 value)
		{
			String strMac = value.ToString("X");
			strMac = strMac.PadLeft(12, '0');

			for(int x = 0; x < AddressLengths.MAC; x++)
				m_BinaryAddress[x] = (byte)int.Parse(strMac.Substring(x * 2,2), NumberStyles.HexNumber);
			AddressAsByteArray = m_BinaryAddress;
			_type = AddressType.MAC;
		}

		void LoadAddressAsByteArray()
		{
			/** parse the binary address */
			if(_address.Length != MAC_STD_FORMAT_LENGTH)
			{
				throw new Exception("Logic exception in MAC Address parser");
			}

			m_BinaryAddress = new byte[AddressLengths.MAC];
			int nOffset = 0;

			for(int x = 0; x < _address.Length; x += 3)
			{
				m_BinaryAddress[nOffset++] = (byte)Int32.Parse(_address.Substring(x, 2), NumberStyles.HexNumber);
			}
		}

		public byte[] ToByteArray()
		{
			return m_BinaryAddress;
		}

		public static bool TryParse(String strAddress, out MACAddress address)
		{
			address = null;
			if(IsValid(strAddress))
			{
				address = new MACAddress(strAddress);
			}
			return address != null;
		}

		public static MACAddress Parse(Object value)
		{
			MACAddress address = null;
			if(!TryParse(value.ToString(), out address))
			{
				throw new FormatException("Invalid MAC address");
			}
			return address;
		}

		public override int GetHashCode()
		{
			return (int)Crc32.Compute(m_BinaryAddress);
		}

		/// <summary>
		/// Performs a series of checks to see if the Bluetooth address is a 'reasonable' and valid address
		/// </summary>
		/// This is used to perform a sanity check on addresses coming off the internet.
		/// We may want to expand this logic.
		public bool IsReasonable
		{
			get
			{
				bool bRet = true;

				if(IsEmpty)
				{
					/** empty address is no good */
					bRet = false;
				}
				else
				{
					/** more than 4 zeroes is no good */
					int zeroes = 0;

					for(int i = 0; i < AddressLengths.MAC; i++)
					{
						if(m_BinaryAddress[i] == 0)
							zeroes++;
					}
					if(zeroes >= 5)
						bRet = false;
				}
				return bRet;
			}
		}

		public int CompareTo(object other)
		{
			if(other is MACAddress == false)
			{
				throw new InvalidCastException("Can not compare to other object");
			}
			return this.ToString().CompareTo(other.ToString());
		}

		public override string ToString()
		{
			return base.ToString().ToLower();
		}
	}
}

