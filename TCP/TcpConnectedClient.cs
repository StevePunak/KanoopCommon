using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

using System.IO;
using System.Reflection;
using System.Threading;
using KanoopCommon.Addresses;
using KanoopCommon.Logging;

namespace KanoopCommon.TCP
{
	public delegate void TcpClientDataReceivedHandler(TcpConnectedClient client, byte[] data);

	public abstract class TcpConnectedClient
	{
		public event TcpClientDataReceivedHandler	DataReceived;

		//
		// Private Constants
		//
		private const int							PACKET_MAX_LEN = 128 * 2;

		//
		// Private Variables
		//
		protected TcpClient							_tcpClient;
		protected TcpListener						_tcpListener;
		protected TcpServer							_server;
		protected IPv4AddressPort					_listenerAddress;
		private IPv4AddressPort						_localAddress;
		private IPv4AddressPort						_remoteAddress;
		private byte[]								_receiveBuffer;


		/// <summary>
		/// Returns the TCP server listen ip addr and port
		/// </summary>
		public IPv4AddressPort ServerAddress { get { return _listenerAddress; } }

		/// <summary>
		/// Returns the TCP client socket
		/// </summary>
		public TcpClient TcpClient { get { return _tcpClient; } }

		/// <summary>
		/// Returns the TCP client socket
		/// </summary>
		public Socket Socket { get { return _tcpClient.Client; } }

		/// <summary>
		/// Returns the IP Address this client is bound to (Host Address)
		/// </summary>
		public IPv4AddressPort LocalAddress { get { return _localAddress; } }

		/// <summary>
		/// Returns the IP Address this client is connected to (SiteWerxDestination Address)
		/// </summary>
		public IPv4AddressPort RemoteAddress { get { return _remoteAddress; } }

		/// <summary>
		/// Returns True if this client connection is open
		/// </summary>
		public bool IsConnected
		{
			get { return (_tcpClient.Client != null && _tcpClient.Connected); }
		}

		private DateTime	_connectTime;
		public DateTime	ConnectTime { get {return _connectTime; } }

		private UInt32		_bytesSent;
		public UInt32 BytesSent { get {return _bytesSent; } }

		private UInt32		_bytesReceived;
		public UInt32 BytesReceived { get { return _bytesReceived; } }

		//
		// Constructor
		//
		/// <summary>
		/// Create a new TcpClient with a connected TCPClient
		/// </summary>
		/// <param name="tcpClient"></param>
		protected TcpConnectedClient(TcpClient tcpClient, TcpListener listener, TcpServer parent)
		{
			_tcpClient = tcpClient;
			_listenerAddress = new IPv4AddressPort((IPEndPoint)listener.LocalEndpoint);

			// Set Reference To Parent
			_server = parent;

			// Set Local and Remote Addresses
			_localAddress = new IPv4AddressPort(((IPEndPoint)_tcpClient.Client.LocalEndPoint).Address.ToString() + ":" + ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Port.ToString());
			_remoteAddress = new IPv4AddressPort(((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address.ToString() + ":" + ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Port.ToString());

			// Server will start our I/O after client is fully constructed
			_receiveBuffer = new byte[PACKET_MAX_LEN];

			_connectTime = DateTime.UtcNow;
		}

		public virtual int Send(String str)
		{
			byte[] output = ASCIIEncoding.UTF8.GetBytes(str);
			return _Send(output, 0, output.Length);
		}

		public virtual int Send(byte[] data)
		{
			return _Send(data, 0, data.Length);
		}

		public virtual int Send(byte data)
		{
			return _Send(new byte[] { data }, 0, 1);
		}

		public virtual int Send(byte[] packet, int offset, int size)
		{
			return _Send(packet, offset, size);
		}

		private int _Send(byte[] packet, int offset, int size)
		{
			int bytesSent = 0;
			try
			{
				// Send Data if socket open
				if(IsConnected)
				{
					// Send Data
					bytesSent = _tcpClient.Client.Send(packet, offset, size, SocketFlags.None);
					_bytesSent += (UInt32)bytesSent;
				}
				// Otherwise, perform close sequence
				else
				{
					Close();
				}
			}
			catch(Exception ex)
			{
				if(_server != null)
				{
					_server.RaiseException(ex);
				}

				// Perform close sequence
				Close();
			}
			return bytesSent;
		}

		public virtual void SendAsynch(String str)
		{
			byte[] buffer = ASCIIEncoding.UTF8.GetBytes(str);
			SendAsynch(buffer, 0, buffer.Length);
		}

		public virtual void SendAsynch(byte[] packet)
		{
			SendAsynch(packet, 0, packet.Length);
		}

		public virtual void SendAsynch(byte[] packet, int offset, int size)
		{
			SendAsynch(packet, offset, size, new AsyncCallback(SocketSendCallback));
		}

		public virtual void SendAsynch(byte[] packet, int offset, int size, AsyncCallback callback)
		{
			try
			{
				SocketError errorCode;

				// Send Data
				_tcpClient.Client.BeginSend(packet, offset, size, SocketFlags.None, out errorCode, callback, this);
				_bytesSent += (UInt32)size;
			}
			catch(Exception ex)
			{
				if(_server != null)
				{
					_server.RaiseException(ex);
				}

				// Perform close sequence
				Close();
			}
		}

		public void Close()
		{
			try
			{
				if(TcpServer.ConnectionDebugLogging)
				{
					_server.Log.LogText(LogLevel.DEBUG, "{0} Client {1} Closed", _server.Name, this);
				}

				// Close the connection, if open
				if(IsConnected)
				{
					_tcpClient.Close();
				}

				// Notify the derived client
				OnDisconnect();

				// Notify the server
				_server.OnConnectionClosed(this);

				/** explicitly dispose the client */
				if(_tcpClient.Client != null)
				{
					_tcpClient.Client.Dispose();
				}

			}
			catch(Exception ex)
			{
				if(_server != null)
				{
					_server.RaiseException(ex);
				}
			}
		}

		public override string ToString()
		{
			return _remoteAddress.Address;
		}

		public virtual void BeginReceive()
		{
			/**
			 * !!!!!!!!!!!!!!!!!!
			 * WILL A NON-STATIC BUFFER FIX THIS?
			 */
			if(_tcpClient.Client != null)
			{
				_tcpClient.Client.BeginReceive(_receiveBuffer, 0, PACKET_MAX_LEN, SocketFlags.None, OnSocketReceive, _tcpClient);
			}
		}

		//
		// Private Methods
		//
		private void OnSocketReceive(IAsyncResult ar)
		{
			SocketError errorCode = new SocketError();
			bool		bConnectionClosed = true;

			try
			{
				if(IsConnected)
				{
					// Get Data From Socket And Add To XML String Buffer
					int nLength = _tcpClient.Client.EndReceive(ar, out errorCode);

					// Call Method To Process Data
					if(nLength > 0)
					{
						_bytesReceived += (UInt32)nLength;

						// Call Data Prcocessing Routine Of Derived Class
						byte[] recvData = new byte[nLength];
						Buffer.BlockCopy(_receiveBuffer, 0, recvData, 0, nLength);

						// Call the event
						if(DataReceived != null)
						{
							DataReceived(this, recvData);
						}

						// Call the override method
						OnDataReceived(recvData);

						// Start new receive
						if(IsConnected)
						{
							BeginReceive();
							bConnectionClosed = false;
						}
					}
				}
			}
			catch(ObjectDisposedException)
			{
				/**
				 * this happens when this side closes the socket
				 */
				_server.Log.LogText(LogLevel.DEBUG, "TCPClient object was violently disposed of");
			}
			catch(Exception ex)
			{
				// On all other exceptions, close the socket and raise an event
				if(_server != null)
				{
					_server.RaiseException(ex);
				}
			}

			// Perform the close sequence
			if(bConnectionClosed)
			{
				Close();
			}
		}

		void SocketSendCallback(IAsyncResult ar)
		{
		}

		//
		// Methods To Be Overridden in Derived Class
		//
		protected abstract void OnDataReceived(byte[] buffer);

		protected virtual void OnDisconnect() {}
	}
}
