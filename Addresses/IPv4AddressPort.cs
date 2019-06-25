using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Xml.Serialization;
using System.ComponentModel;
using KanoopCommon.Conversions;
using System.IO;
using KanoopCommon.Logging;
using KanoopCommon.Extensions;

namespace KanoopCommon.Addresses
{
	[TypeConverterAttribute(typeof(IPv4AddressPortConverter))]
	public class IPv4AddressPort : AddressBase, IComparable
	{

		#region Constants

		internal const String EMPTY_ADDRESS = "0.0.0.0:0000";

		#endregion

		#region Private Member Variables

		private String					_hostName;
		private UInt16					_port;

		bool							_resolved;
		IPv4ResolvedAddressPort			_resolvedAddress;

		#endregion

		#region Public Properties

		public override int Length
		{
			get { return AddressLengths.IP4; }
		}
		
		public UInt16 Port
		{
			get { return _port; }
			set 
			{
				_port = value; 
				Address = String.Format("{0}:{1}", AddressAsIPv4Address.Address, _port);
			}
		}

		public String HostName
		{
			get { return _hostName; }
		}

		public static IPv4AddressPort EmptyAddress
		{
			get { return new IPv4AddressPort("0.0.0.0:0"); }
		}

		public override String Address
		{
			get 
			{
				if (!_resolved)
				{
					ResolveHostName();
				}

				return (_resolved) ? _resolvedAddress.ToString() : _address;
			}
			set 
			{
				ParseAddressString(value);
			}
		}

		public IPv4Address AddressAsIPv4Address
		{
			get 
			{
				if(_resolved == false && ResolveHostName() == false)
				{
					return new IPv4Address();
				}
				else
				{
					return _resolvedAddress.AddressAsIPv4Address;
				}
			}
			set { this.Address = value.Address; }
		}

		public override byte[] AddressAsByteArray
		{
			get 
			{
				if(_resolved == false && ResolveHostName() == false)
				{
					return new byte[0];
				}
				else
				{
					return _resolvedAddress.AddressAsByteArray;
				}
			}
			set 
			{
				if (_resolvedAddress == null)
					_resolvedAddress = new IPv4ResolvedAddressPort();
				_resolvedAddress.AddressAsByteArray = value;
				_resolved = true;
				_type = AddressType.IP4_PORT;
			}
		}
	   
		public IPEndPoint AddressAsIPEndPoint
		{
			get 
			{
				if(_resolved == false && ResolveHostName() == false)
				{
					return new IPEndPoint(IPAddress.Any, 0);
				}
				else
				{
					return _resolvedAddress.AddressAsIPEndPoint;
				}
			}
			set 
			{
				_resolvedAddress.AddressAsIPEndPoint = value;
				_resolved = true;
				_type = AddressType.IP4_PORT;
			}
		}

		#endregion

		#region Constructor(s)

		public IPv4AddressPort()
			: base(EMPTY_ADDRESS, AddressType.IP4_PORT)
		{
			_port = 0;
			_resolved = false;
			_resolvedAddress = null;
			_hostName = String.Empty;
		}

		public IPv4AddressPort(byte[] inArray) 
			: this(inArray, 0) {}

		public IPv4AddressPort(byte[] inArray, int offset)
		{
			byte[] addr = new byte[AddressLengths.IP4];
			if(inArray.Length == AddressLengths.IP4)
			{
				Array.Copy(inArray, offset, addr, 0, AddressLengths.IP4);
				this.AddressAsByteArray = addr;
			}
			else if(inArray.Length == 4)
			{
				Array.Copy(inArray, offset, addr, 0, AddressLengths.IP4_NOPORT);
				addr[4] = addr[5] = 0;
				this.AddressAsByteArray = addr;
			}
			else
			{
				throw new AddressParseException("Could not parse a byte array of length {0} into an IPv4AddressPort", inArray.Length - offset);
			}
		}
	
		public IPv4AddressPort(IPAddress address, int port)
			: this(address, (UInt16)port) {}

		public IPv4AddressPort(IPAddress address, UInt16 port) 
			: this(new IPv4Address(address), port) {}

		public IPv4AddressPort(IPEndPoint ep)
			: this(new IPv4Address(ep.Address), ep.Port) {}

		public IPv4AddressPort(String address)
			: this()
		{
			ParseAddressString(address);
		}

		public IPv4AddressPort(String address, UInt16 port) 
			: this(String.Format("{0}:{1}", address, port)) {}

		public IPv4AddressPort(String address, int port) 
			: this(String.Format("{0}:{1}", address, port)) {}

		public IPv4AddressPort(UInt64 address) 
			: this()
		{
			byte[] bytes = System.BitConverter.GetBytes(address);
			Array.Reverse(bytes);
			byte[] outBytes = new byte[6];
			Array.Copy(bytes, 2, outBytes, 0, AddressLengths.IP4);
			AddressAsByteArray = outBytes;
		}

		public IPv4AddressPort(IPv4Address addr, int port) 
			: this(addr, (UInt16)port) {}

		public IPv4AddressPort(IPv4Address addr, UInt16 port) 
			: this()
		{
			ParseAddressString(String.Format("{0}:{1}", addr, port));
		}

		#endregion

		#region Internal Private Methods

		bool ResolveHostName()
		{
			bool result = false;
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(_hostName);
				IPAddress[] addressList = hostEntry.AddressList;
				for(int x = 0;x < addressList.Length && result == false;x++)
				{
					if (addressList[x].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && addressList[x].GetAddressBytes()[0] != 169)
					{
						_resolvedAddress = new IPv4ResolvedAddressPort(String.Format("{0}:{1}", addressList[x].ToString(), _port));
						_resolved = true;
						result = true;
					}
				}
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.WARNING, "IPv4AddressPort Resolve Host '{0}' Exception {1}", _hostName, e.Message);
			}
			return result;
		}

		protected void ParseAddressString(String value)
		{
			String[] splitByPort;
			splitByPort = value.Split(':');
			_hostName = "";

			if (splitByPort.Length == 2)
			{
				_hostName = splitByPort[0];
				_port = UInt16.Parse(splitByPort[1], System.Globalization.NumberStyles.Integer);
			}

			List<String> splitByDots = new List<String>(splitByPort[0].Split('.'));
			if(splitByDots.Count == AddressLengths.IP4_NOPORT && splitByDots.AreAllDigits())
			{
				_resolvedAddress = new IPv4ResolvedAddressPort(value);
				_resolved = true;
			}
			else if(_hostName.Length == 0)
			{
				_hostName = splitByPort[0];
				_resolved = false;
			}
			_type = AddressType.IP4_PORT;
			_address = value;
		}

		#endregion

		#region Public Utility Methods

		public IPEndPoint ToEndpoint()
		{
			return AddressAsIPEndPoint;
		}

		public static bool IsValid(String strAddress)
		{
			bool result = false;
			String[] addressAndPort = strAddress.Split(':');
			String[] addressChunks = addressAndPort[0].Split('.');
			int n;
			if(	addressAndPort.Length == 2 && 
				addressChunks.Length == 4 && 
				int.TryParse(addressChunks[0], out n) &&
				int.TryParse(addressChunks[1], out n) &&
				int.TryParse(addressChunks[2], out n) &&
				int.TryParse(addressChunks[3], out n) &&
				int.TryParse(addressAndPort[1], out n) == true &&
				n > 0 && n < 65535)
			{
				result = true;
			}
			else if(addressAndPort.Length == 2 && 
					Char.IsDigit(strAddress[0]) == false &&
					int.TryParse(addressAndPort[1], out n) == true &&
					n > 0 && n < 65535)
			{
				result = true;
			}
			return result;
		}

		public static bool TryParse(String strAddress, out IPv4AddressPort outaddr)
		{
			outaddr = null;

			if(IsValid(strAddress))
			{
				outaddr = new IPv4AddressPort(strAddress);
			}
			return outaddr != null;
		}

		public static bool TryParseHostPort(String hostPort, out IPv4AddressPort outaddr)
		{
			bool ret = false;
			String[] splitHostPort = hostPort.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

			try
			{
				outaddr = new IPv4AddressPort(splitHostPort[0], int.Parse(splitHostPort[1]));

				ret = outaddr.ResolveHostName();
			}
			catch (Exception)
			{
				outaddr = null;
				ret = false;
			}

			return ret;
		}

		public int CompareTo(object other)
		{
			if(other is IPv4AddressPort == false)
			{
				throw new InvalidCastException("Can not compare to other object (not IPv4Address)");
			}
			return this.ToString().CompareTo(other.ToString());
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			bool result = false;
			try
			{
				result = ((IPv4AddressPort)obj).ToString().Equals(this.ToString());
			}
			catch
			{
				result = false;
			}
			return result;
		}

		public new IPv4AddressPort Clone()
		{
			return new IPv4AddressPort(_address);
		}

		public override string ToString()
		{
			return Address;
		}

		#endregion
	}
}


