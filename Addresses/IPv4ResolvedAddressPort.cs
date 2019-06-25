using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Xml.Serialization;
using KanoopCommon.Extensions;
using KanoopCommon.Conversions;

namespace KanoopCommon.Addresses
{
	public class IPv4ResolvedAddressPort : AddressBase
	{
		#region Public Methods

		public override int Length { get { return AddressLengths.IP4; } }
		
		public UInt16 Port{ get { return _port; } set { _port = value; } }

		public override String Address
		{
			get { return _address; }
			set 
			{
				ResolveAddress(value);
			}
		}

        public IPv4Address AddressAsIPv4Address
		{
			get { return new IPv4Address(m_BinaryAddress); }
			set { this.Address = value.Address; }
		}

		public override byte[] AddressAsByteArray
		{
			get { return m_BinaryAddress; }
			set 
			{
				Array.Copy(value, m_BinaryAddress, AddressLengths.IP4);
				int tmp = (m_BinaryAddress[4] << 8) |  m_BinaryAddress[5];
				_port = (UInt16)IPAddress.HostToNetworkOrder(tmp);
				_address = String.Format("{0}.{1}.{2}.{3}:{4}",
										 m_BinaryAddress[0],
										 m_BinaryAddress[1],
										 m_BinaryAddress[2],
										 m_BinaryAddress[3],
										 _port);
				_type = AddressType.IP4_RESOLVED;
			}
		}

        public System.Net.IPEndPoint AddressAsIPEndPoint
        {
            get { return new System.Net.IPEndPoint(new System.Net.IPAddress(AddressAsIPv4Address.AddressAsByteArray), _port); }
            set { this.Address = value.Address.ToString() + ":" + value.Port.ToString(); }
        }

		#endregion

		#region Private Member Variables

		private byte[] m_BinaryAddress;
		private UInt16 _port = 0;

		#endregion

		#region Constructor(s)

		public IPv4ResolvedAddressPort() 
		{	
			_address = IPv4AddressPort.EMPTY_ADDRESS;
			_type = AddressType.IP4_RESOLVED;
			_port = 0;
			m_BinaryAddress = new byte[AddressLengths.IP4];
		}

		public IPv4ResolvedAddressPort(String address)
			: this()
		{
			this.Address = address; 
		}

        public IPv4ResolvedAddressPort(IPv4Address addr, UInt16 port) 
			: this(String.Format("{0}:{1}", addr, port)) {}

		public IPv4ResolvedAddressPort(byte[] inArray) 
			: this(new IPv4AddressPort(inArray)) {}
		
		public IPv4ResolvedAddressPort(byte[] inArray, int offset)
			: this(new IPv4AddressPort(inArray, offset)) {}

		public IPv4ResolvedAddressPort(IPAddress address, UInt16 port)
			: this(String.Format("{0}:{1}", address, port)) {}

        public IPv4ResolvedAddressPort(UInt64 address)
			: this(new IPv4AddressPort(address)) {}

        public IPv4ResolvedAddressPort(IPv4AddressPort addr)
			: this(addr.ToString()) {}

		#endregion

		#region Address Resolution

		void ResolveAddress(String value)
		{
			String[] splitByPort;
			splitByPort = value.Split(':');
			if(splitByPort.Length == 2)
			{
				_port = UInt16.Parse(splitByPort[1], System.Globalization.NumberStyles.Integer);
			}

			List<String> splitByAddressParts = new List<String>(splitByPort[0].Split('.'));

			/** if they're all numeric, that's the final resolution */
			if(splitByAddressParts.AreAllDigits())
			{
				if(splitByAddressParts.Count != 4)
				{
					throw new AddressParseException("Invalid number of octects passed ({0}}", splitByAddressParts.Count);
				}
				int x;
				for (x = 0; x < AddressLengths.IP4_NOPORT; ++x)
				{
					m_BinaryAddress[x] = (byte)int.Parse(splitByAddressParts[x], System.Globalization.NumberStyles.Integer);
				}
				BinaryConverter.UInt16ToByteArrayNetOrder(_port, ref m_BinaryAddress, x);

				_type = AddressType.IP4_RESOLVED;
				_address = value;
			}
		}

		#endregion

		#region Utility

		public override bool Equals(object obj)
        {
            try
            {
                return ((IPv4ResolvedAddressPort)obj)._address == this._address;
            }
            catch
            {
                return false;
            }
        }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

	}
}


