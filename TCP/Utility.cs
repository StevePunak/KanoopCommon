using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using KanoopCommon.Logging;
using KanoopCommon.Threading;
using KanoopCommon.Addresses;

namespace KanoopCommon.TCP
{
	public static class Utility
	{
		public static IPv4Address GetRoutableInterface(IPv4Address routeTo)
		{
			IPv4Address ret = new IPv4Address();
			UInt32 routeToBytes = routeTo.AddressAsUInt32();

			foreach(NetworkInterface netif in NetworkInterface.GetAllNetworkInterfaces())
			{
				if(netif.OperationalStatus == OperationalStatus.Up)
				{
					IPInterfaceProperties ipProperties = netif.GetIPProperties();
					foreach(UnicastIPAddressInformation addressInfo in ipProperties.UnicastAddresses)
					{
						if(addressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							IPv4Address ifAddress = new IPv4Address(addressInfo.Address);
							try
							{
								if(addressInfo.IPv4Mask != null)
								{
									
									byte[] maskBytes = addressInfo.IPv4Mask.GetAddressBytes();
									if(BitConverter.IsLittleEndian)
									{
										Array.Reverse(maskBytes);
									}
									UInt32 ifaddressMask = BitConverter.ToUInt32(maskBytes, 0);
									if((routeToBytes & ifaddressMask) == (ifAddress.AddressAsUInt32() & ifaddressMask))
									{
										ret = ifAddress;
										break;
									}
								}
							}
							catch(Exception)
							{
								/** Mono will except on the IPv4Mask property... just assign it this address */
								ret = ifAddress;
								Log.SysLogText(LogLevel.INFO, "GetRoutableInterface - assigning {0}", ifAddress);
							}
						}
					}
				}
			}

			return ret;
		}

		public static List<IPv4Address> GetInternalInterfaces()
		{
			List<IPv4Address> ret = new List<IPv4Address>();

			foreach(NetworkInterface netif in NetworkInterface.GetAllNetworkInterfaces())
			{
				if(netif.OperationalStatus == OperationalStatus.Up)
				{
					IPInterfaceProperties ipProperties = netif.GetIPProperties();
					foreach(UnicastIPAddressInformation addressInfo in ipProperties.UnicastAddresses)
					{
						if(addressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							IPv4Address ifAddress = new IPv4Address(addressInfo.Address);
							if(ifAddress.AddressAsByteArray[0] == 192 || ifAddress.AddressAsByteArray[0] == 10)
							{
								ret.Add(ifAddress);
							}
						}
					}
				}
			}

			return ret;
		}

		public static String NormalizeHostName(String hostName)
		{
			return Char.IsDigit(hostName[0]) ? hostName : hostName.Split('.')[0].ToLower();
		}

		public static bool IsReasonableEndpoint(IPv4AddressPort address)
		{
			return address.Port != 0 && address.AddressAsIPv4Address.IsEmpty == false;
		}

		class ListenerTest
		{
			public MutexEvent WaitEvent { get; private set; }
			
			public TcpClient Client { get; private set; }

			public bool IsConnected { get; set; }

			public ListenerTest(TcpClient client)
			{
				Client = client;
				WaitEvent = new MutexEvent();
				IsConnected = false;
			}
		}

		public static bool IsListenerAtAddress(IPv4AddressPort address)
		{
			TcpClient client = new TcpClient();
			ListenerTest test = new ListenerTest(client);

			try
			{
				client.BeginConnect(address.AddressAsIPv4Address.AddressAsIPAddress, address.Port, new AsyncCallback(ListenerTesterConnectCallback), test);
				if(test.WaitEvent.Wait(30*1000) == false)
				{
					client.Close();
				}
			}
			catch(Exception){}
			return test.IsConnected;
		}

		static void ListenerTesterConnectCallback(IAsyncResult ar)
		{
			ListenerTest test = ar.AsyncState as ListenerTest;
			try
			{
				test.Client.EndConnect(ar);

				test.IsConnected = test.Client.Connected;

				test.Client.Close();
			}
			catch(Exception){}
			finally
			{
				test.WaitEvent.Set();
			}
		}
	}
}
