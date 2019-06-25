using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace KanoopCommon.Addresses
{
	public enum AddressType
	{
		NONE = 0,

		IP4_NOPORT		= 4,
		IP4_PORT		= 5,
		IP6				= 6,
		MAC				= 0x21,
		IP4_RESOLVED	= 0x22,

		INVALID = 1024
	}

	public class AddressLengths
	{
		public const int NONE = 0;
		public const int IP4_NOPORT = 4;
		public const int IP4 = 6;
		public const int MAC = 6;
	}
	
	public class AddressBase
	{
		protected String			_address;
		protected AddressType		_type = AddressType.NONE;

		public virtual int Length
		{
			get { return _address.Length; }
		}

		[XmlIgnoreAttribute]
		public virtual byte[] AddressAsByteArray
		{
			get
			{
				ASCIIEncoding encoder = new ASCIIEncoding();
				return encoder.GetBytes(_address);
			}
			set
			{
			}
		}

		/**
		 * @b          AddressBase
		 *             Constructor
		 */
		public AddressBase(String address, AddressType type)
		{
			_address = address;
			_type = type;
		}

		public AddressBase(String address)
		{
			this.Address = address;
		}

		public AddressBase()
		{
			_address = "";
			_type = AddressType.NONE;
		}

		public virtual AddressBase Clone()
		{
			AddressBase ret = new AddressBase(_address, _type);
			return ret;
		}

		public override String ToString() { return Address; }

		[XmlIgnore]
		public virtual String Address
		{
			get { return _address; }
			set { _type = DetectAddressType(value); _address = value; }
		}

		[XmlIgnore]
		public AddressType AddressType
		{
			get { return _type; }
			set { _type = value; }
		}
		
		public override bool Equals(Object other)
		{
			bool result = false;
			if(	(other is AddressBase == true && ((AddressBase)other).ToString().ToUpper() == Address.ToUpper()) ||
				(other is String && other.Equals(Address)) )
			{
				result = true;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static AddressType DetectAddressType(String strAddress)
		{
			AddressType	retType = AddressType.MAC;

			/**
			 * if we have a dot, it's assumed to be IP
			 */
			if(strAddress.IndexOf('.') != -1)
			{
				String[] strings = strAddress.Split('.');
				if (strings.Length == 4)
				{
					/**
					 * if there's a ':', it's unitAddress/port combo
					 */
					if (strAddress.IndexOf(':') != -1)
					{
						retType = AddressType.IP4_PORT;
					}
					/**
					 * otherwise, just an IP unitAddress
					 */
					else
					{
						retType = AddressType.IP4_NOPORT;
					}
				}
			}
			/**
			 * if it's the right length, and has ':', its bluetooth (MAC)
			 */
			else if(strAddress.IndexOf(':') != -1 && strAddress.Length == 17)
			{
				retType = AddressType.MAC;
			}
			return retType;
		}
		public virtual bool IsValid() { return true; }

		public static AddressBase Factory(String strAddress)
		{
			AddressType t = DetectAddressType(strAddress);
			return Factory(strAddress, t);
		}

		public static AddressBase Factory(String strAddress, byte addressType) { return Factory(strAddress, (AddressType)addressType); }

		public static AddressBase Factory(String strAddress, int addressType) { return Factory(strAddress, (AddressType)addressType); }

		public static AddressBase Factory(String strAddress, AddressType addressType)
		{
			AddressBase ret;
			switch(addressType)
			{
				case AddressType.NONE:
				{
					ret = new EmptyAddress(strAddress);
					break;
				}
				case AddressType.IP4_PORT:
				{
					ret = new IPv4AddressPort(strAddress);
					break;
				}
				case AddressType.IP4_NOPORT:
				{
					ret = new IPv4Address(strAddress);
					break;
				}
				case AddressType.MAC:
				{
					ret = new MACAddress(strAddress);
					break;
				}

				default:
					throw new Exception("INVALID ADDRESS STRING");

			}
			return ret;
		}
		
		AddressBase Clone(AddressBase address)
		{
			return AddressBase.Factory(address.Address, address.AddressType);
		}

	}
}

