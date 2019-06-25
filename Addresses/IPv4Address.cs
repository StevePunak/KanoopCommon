using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Xml.Serialization;
using KanoopCommon.Conversions;

namespace KanoopCommon.Addresses
{
	public class IPv4Address : AddressBase, IComparable
	{
		#region Private Member Variables

		private byte[] _binaryAddress = new byte[AddressLengths.IP4_NOPORT];

		#endregion

		#region AddressBase Overrides

		public override int Length
		{
			get { return AddressLengths.IP4_NOPORT; }
		}

		public override String Address
		{
			get { return _address; }
			set 
			{
                String[] strBytes;
                strBytes = value.Split('.');

				for (int x = 0; x < AddressLengths.IP4_NOPORT; x++)
				{
					_binaryAddress[x] = (byte)int.Parse(strBytes[x], System.Globalization.NumberStyles.Integer);
				}
				_type = AddressType.IP4_NOPORT;
                _address = value;
			}
		}
		
		public override byte[] AddressAsByteArray
		{
			get { return _binaryAddress; }
			set 
			{
				Array.Copy(value, _binaryAddress, AddressLengths.IP4_NOPORT);
				_address = String.Format("{0}.{1}.{2}.{3}",
										 _binaryAddress[0],
										 _binaryAddress[1],
										 _binaryAddress[2],
										 _binaryAddress[3]);
				_type = AddressType.IP4_NOPORT;
			}
		}

		#endregion

		#region Public Properties
		
		public Byte Byte1 { get { return _binaryAddress[0]; } }

		public Byte Byte2 { get { return _binaryAddress[1]; } }

		public Byte Byte3 { get { return _binaryAddress[2]; } }

		public Byte Byte4 { get { return _binaryAddress[3]; } }

		public UInt32 AddressAsUInt32()
		{
			byte[] retBytes = (byte[])_binaryAddress.Clone();
			if(BitConverter.IsLittleEndian)
			{
				Array.Reverse(retBytes);
			}
			UInt32 ret = BitConverter.ToUInt32(retBytes, 0);
			return ret;
		}
        
		public IPAddress AddressAsIPAddress
        {
            get { return new System.Net.IPAddress(_binaryAddress); }
            set { this.Address = value.ToString(); }
        }

		#endregion

		#region Constructors

		public IPv4Address(byte[] inArray) : this(inArray, 0) {}
		public IPv4Address(byte[] inArray, int nOffset)
		{
			byte[] addr = new byte[AddressLengths.IP4_NOPORT];
			Array.Copy(inArray, nOffset, addr, 0, AddressLengths.IP4_NOPORT);
			this.AddressAsByteArray = addr;
		}

		public IPv4Address(IPEndPoint ep) 
			: this(ep.Address.ToString()) {}

		public IPv4Address(IPAddress ip)
			: this(ip.ToString()) {}

		public IPv4Address(String address)
		{
			this.Address = address;
		}

		public IPv4Address()
		{
			_address = "";
			_type = AddressType.IP4_NOPORT;
		}

		#endregion

		#region Static Utility

		public static bool TryParse(String strAddress, out IPv4Address outaddr)
		{
			outaddr = null;

			if(IsValid(strAddress))
			{
				outaddr = new IPv4Address(strAddress);
			}
			return outaddr != null;
		}

		public static IPv4Address Empty { get { return new IPv4Address(IPAddress.Any); } }

		public static IPv4Address ResolveHostName(String strHostName)
		{
			IPv4Address address = null;
			return address;
		}

		public static bool IsValid(String strAddress)
		{
			bool bRet = false;
			String[] s1 = strAddress.Split('.');
			int n;
			if(	s1.Length == 4 && 
				Parser.TryParse(s1[0], out n) &&
				Parser.TryParse(s1[1], out n) &&
				Parser.TryParse(s1[2], out n) &&
				Parser.TryParse(s1[3], out n))
			{
				bRet = true;
			}
			return bRet;
		}

		public bool IsEmpty
		{
			get { return _binaryAddress[0] == 0 && _binaryAddress[1] == 0 && _binaryAddress[2] == 0 && _binaryAddress[3] == 0; }
		}

		#endregion

		#region IComparable Overrides

		public override bool Equals(object obj)
        {
            try
            {
                return ((IPv4Address)obj)._address == this._address;
            }
            catch
            {
                return false;
            }
        }

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public int CompareTo(object other)
		{
			if(other is IPv4Address == false)
			{
				throw new InvalidCastException("Can not compare to other object (not IPv4Address)");
			}
			return this.ToString().CompareTo(other.ToString());
		}

		#endregion

	}

}
