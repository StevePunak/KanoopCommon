using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using KanoopCommon.Threading;
using KanoopCommon.Addresses;
using KanoopCommon.Logging;

namespace KanoopCommon.TCP.Clients
{
	public delegate void DataReceivedHandler(byte[] data);

	public class TcpClientClient
	{
		#region Delegates

		public delegate void ConnectHandler(TcpClientClient client);
		public delegate void DisconnectHandler(TcpClientClient client);
		public delegate void ExceptionHandler(TcpClientClient client, Exception e);

		#endregion

		#region Events

		public event DataReceivedHandler DataReceived;
		public event ConnectHandler SocketConnected;
		public event DisconnectHandler SocketDisconnected;
		public event ExceptionHandler SocketException;

		#endregion

		#region Public Properties

		public bool IsConnected { get { return Client.Connected; } }

		public TcpClient Client { get; set; }

		public IPv4AddressPort RemoteAddress { get; set; }

		public DateTime ConnectTime  { get; protected set; }

		public static bool ConnectionLoggingEnabled { get; set; }

		public static int TotalCreateCount { get; private set; }

		public static int TotalConnectCount { get; private set; }

		public static int TotalConnectErrorCount { get; private set; }

		#endregion

		#region Private Members

		byte[]			_receiveBuffer;
		IAsyncResult    _connectResult;

		#endregion

		#region Constructors

		static TcpClientClient()
		{
			ConnectionLoggingEnabled = true;
			TotalConnectCount = 0;
			TotalConnectErrorCount = 0;
			TotalCreateCount = 0;
		}

		public TcpClientClient()
		{
			Client = new TcpClient();
			_receiveBuffer = new byte[2048];
			ConnectTime = DateTime.MinValue;
			TotalConnectCount++;
		}

		#endregion

		#region Virtuals

		public virtual void Connect(IPv4AddressPort remoteAddress)
		{
			RemoteAddress = remoteAddress;
			Client.Connect(RemoteAddress.AddressAsIPEndPoint);
			if(SocketConnected != null)
			{
				SocketConnected(this);
			}
			ConnectTime = DateTime.UtcNow;
			BeginReceive();
		}

		public virtual void BeginConnect(IPv4AddressPort remoteAddress)
		{
			RemoteAddress = remoteAddress;
			_connectResult = Client.BeginConnect(remoteAddress.AddressAsIPv4Address.AddressAsIPAddress, remoteAddress.Port, new AsyncCallback(ConnectCallback), this);
		}

		public virtual void EndConnect()
		{
			try
			{
				Client.Client.EndConnect (_connectResult);
			}
			catch(Exception ex) 
			{
				if (SocketException != null)
				{
					SocketException (this, ex);
				}
			}

			// The above exception handler calls a socket disconnected event.
			// TODO Fix timing hole
			if (Client.Client.Connected == false && SocketDisconnected != null)
			{
				SocketDisconnected (this);
			}
		}

		public virtual void Disconnect()
		{
			Disconnect(false);
		}

		public virtual void Disconnect(bool reuseAddr)
		{
			if(Client.Connected)
			{
				try
				{
					Client.Client.Disconnect(false);
					Client.Client.Dispose();
				}
				catch(Exception) {}

				if(SocketDisconnected != null)
				{
					SocketDisconnected(this);
				}
			}
		}

		#endregion

		#region Public Access Methods

		public virtual int Send(String data)
		{
			return Send(ASCIIEncoding.UTF8.GetBytes(data));
		}

		public virtual int Send(byte[] data)
		{
			return Send(data, 0, data.Length);
		}

		public virtual int Send(byte[] data, int offset, int size)
		{
			int ret = size;

			try
			{
				NetworkStream ns = Client.GetStream();

				ns.Write(data, offset, size);
			}
			catch(Exception e)
			{
				if(SocketException != null)
				{
					SocketException(this, e);
				}
				if(Client.Connected == false && SocketDisconnected != null)
				{
					SocketDisconnected(this);
				}
				ret = 0;
			}
			return ret;
		}

		#endregion

		#region Asynch I/O

		void BeginReceive()
		{
			Client.Client.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveDataCallback), this);
		}

		void ReceiveDataCallback(IAsyncResult ar)
		{
			bool socketDisconnected = false;
			try
			{
				if(Client.Client != null && Client.Client.Connected == true)
				{
					int bytesRead = Client.Client.EndReceive(ar);
					if(bytesRead > 0)
					{
						if(DataReceived != null)
						{
							byte[] data = new byte[bytesRead];
							Buffer.BlockCopy(_receiveBuffer, 0, data, 0, bytesRead);
							DataReceived(data);
						}

						if(Client.Client.Connected)
						{
							BeginReceive();
						}
					}
					else
					{
						socketDisconnected = true;
					}

				}
			}
			catch(Exception e)
			{
				if(SocketException != null)
				{
					SocketException(this, e);
				}
			}

			if((Client.Client == null || Client.Client.Connected == false || socketDisconnected == true) && SocketDisconnected != null)
			{
				SocketDisconnected(this);
			}
		}

		void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				Client.Client.EndConnect(ar);

				if(SocketConnected != null)
				{
					SocketConnected(this);
				}

				ConnectTime = DateTime.UtcNow;

				BeginReceive();

				TotalConnectCount++;
			}
			catch(ObjectDisposedException e)
			{
				Log.SysLogText(LogLevel.INFO, "{0} State changed during asynchronous connect. {1}{2}{3}", this, e.Message, Environment.NewLine, e.StackTrace);

				if (SocketException != null)
				{
					SocketException(this, e);
				}

				TotalConnectErrorCount++;
			}
			catch(Exception e)
			{
				if(ConnectionLoggingEnabled)
				{
					Log.SysLogText(LogLevel.WARNING, "{0} Connect Error: {1}", this, e.Message);
				}

				if(SocketException != null)
				{
					SocketException(this, e);
				}

				TotalConnectErrorCount++;
			}
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			return String.Format("TcpClient: {0}", RemoteAddress);
		}

		#endregion
	}
}
