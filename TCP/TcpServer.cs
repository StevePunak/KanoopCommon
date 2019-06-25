using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using KanoopCommon.Conversions;
using KanoopCommon.Threading;
using KanoopCommon.Logging;
using KanoopCommon.Addresses;

namespace KanoopCommon.TCP
{
	#region Delegates

	public delegate void TcpClientConnectHandler(TcpConnectedClient client);
	public delegate void TcpClientDisconnectHandler(TcpConnectedClient client);
	public delegate void TcpExceptionHandler(Exception e);

	#endregion

	public abstract class TcpServer : ThreadBase
	{
		#region Events

		public event TcpClientConnectHandler ConnectionOpened;
		public event TcpClientDisconnectHandler ConnectionClosed;
		public event TcpExceptionHandler TcpException;

		#endregion

		#region Public Properties

		//
		// Client Socket option misc
		//
		private int _sendTimeout = 0;
		public int SendTimeout
		{
			get { return _sendTimeout; }
			set { _sendTimeout = value; }
		}

		private int _lingerTime = 0;
		public int LingerTime
		{
			get { return _lingerTime; }
			set { _lingerTime = value; }
		}

		private UInt32 _keepAliveInterval = 0;
		public UInt32 KeepAliveInterval
		{
			get { return _keepAliveInterval; }
			set { _keepAliveInterval = value; }
		}

		private bool _shuttingDown = false;
		public bool ShuttingDown
		{
			get { return _shuttingDown; }
			set { _shuttingDown = value; }
		}

		private Log _debugLog;
		public Log DebugLog
		{
			get { return _debugLog; }
			set { _debugLog = value; }
		}

		private bool _clientsHandleIO;
		public bool ClientsHandleIO { get { return _clientsHandleIO; } set { _clientsHandleIO = value; } }

		private static bool _connectionDebugLogging;
		public static bool ConnectionDebugLogging
		{
			get { return _connectionDebugLogging; }
			set { _connectionDebugLogging = value; }
		}

		public String FormattedListenAddresses
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach (TcpListener listener in _listeners.Values)
				{
					sb.AppendFormat("{0},", listener.LocalEndpoint);
				}
				return sb.ToString().TrimEnd(new char[] { ',' });
			}
		}

		private Dictionary<int, TcpListener> _listeners;
		protected List<TcpListener> Listeners { get { return new List<TcpListener>(_listeners.Values); } }

		public int ClientCount
		{
			get
			{
				int retVal = _clientList.Count;

				return retVal;
			}
		}

		public List<TcpConnectedClient> Clients
		{
			get
			{
				List<TcpConnectedClient> ret = null;
				try
				{
					_clientLock.Lock();
					ret = new List<TcpConnectedClient>(_clientList.Values);
				}
				finally
				{
					_clientLock.Unlock();
				}
				return ret;
			}
		}

		public static int TotalConnectCount { get; private set; }

		#endregion

		#region Private Properties

		MutexLock _clientLock;
		Dictionary<String, TcpConnectedClient> _clientList = new Dictionary<String, TcpConnectedClient>();

		#endregion

		#region Constructor(s)

		static TcpServer()
		{
			_connectionDebugLogging = false;
			TotalConnectCount = 0;
		}

		//
		// Constructor & Destructor
		//
		/// <summary>
		/// Specify the local IP and Port to listen for connections on.  Call start to begin listening for connections.
		/// </summary>
		TcpServer(String name)
			: base(name)
		{
			_listeners = new Dictionary<int, TcpListener>();
			_clientLock = new MutexLock();
			_clientsHandleIO = false;

			if (Log == null)
			{
				Log = Log.SystemLog;
			}
		}

		public TcpServer(IPv4AddressPort address, String name)
			: this(name)
		{
			AddListener(address);
		}

		public TcpServer(IPv4Address address, UInt16 port, String name)
			: this(new IPv4AddressPort(address, port), name) { }

		public TcpServer(IPEndPoint endPoint, String name)
			: this(new IPv4AddressPort(endPoint), name) { }

		public TcpServer(IPAddress address, UInt16 port, String name)
			: this(new IPv4AddressPort(address, port), name) { }

		#endregion

		#region Public Access Methods

		// Called by the tcp client
		public virtual void OnConnectionClosed(TcpConnectedClient client)
		{
			TcpConnectedClient existingClient;

			try
			{
				_clientLock.Lock();

				// Make sure it is the corresponding client
				if (_clientList.TryGetValue(client.RemoteAddress.Address, out existingClient) &&
					 existingClient.GetHashCode() == client.GetHashCode())
				{
					Log.LogText(LogLevel.DEBUG, "OnConnectionClosed  Found it");
					_clientList.Remove(client.RemoteAddress.Address);

					if (_connectionDebugLogging)
					{
						Log.LogText(LogLevel.DEBUG, "{0} Client {1} Removed", Name, client);
					}
				}
				else
				{
					Log.LogText(LogLevel.DEBUG, "OnConnectionClosed  Did not found it");
					existingClient = null;
				}
			}
			finally
			{
				_clientLock.Unlock();
			}

			// Notify Consumer Of Event
			if (existingClient != null && ConnectionClosed != null)
			{
				ConnectionClosed(client);
			}
		}

		public void AddListener(IPv4AddressPort address)
		{
			if (_listeners.ContainsKey(address.Port))
			{
				throw new Exception(String.Format("Attempt to add listener port {0}. Port already exists", address.Port));
			}

			TcpListener listener = new TcpListener(address.AddressAsIPEndPoint);
			_listeners.Add(address.Port, listener);

			if (State == ThreadState.Started)
			{
				StartListener(listener);
			}
		}

		public void Shutdown()
		{
			/** set flag to allow clean shutdown of listeners */
			ShuttingDown = true;

			/** close all listeners */
			foreach (TcpListener listener in _listeners.Values)
			{
				listener.Stop();
			}
			_listeners.Clear();

			/** close all clients */
			foreach (TcpConnectedClient client in Clients)
			{
				client.Close();
			}
			_clientList.Clear();
		}

		/// <summary>
		/// Sends data to an existing open client connection.
		/// </summary>
		/// <returns>Number of bytes sent or -1 if an Exception occurred</returns>
		public int SendToExistingConnection(IPv4AddressPort address, byte[] packet, int offset, int size)
		{
			int retVal = 0;

			// Find The Existing Client By IPAddress:Port
			TcpConnectedClient client;
			try
			{
				_clientLock.Lock();
				if (_clientList.TryGetValue(address.Address, out client) == false)
				{
					client = null;
				}
			}
			finally
			{
				_clientLock.Unlock();
			}

			try
			{
				if (client != null)
				{
					// Client Found...
					retVal = SendToExistingConnection(client, packet, offset, size);
				}
				else
				{
					// Throw Exception If Client Does Not Exists
					throw new Exception("Cannot send because the client connection does not exist.");
				}
			}
			catch (Exception e)
			{
				// Notify Caller Of Exception
				RaiseException(e);
				retVal = -1;  // Indicate An Error Occurred
			}
			finally
			{
				_clientLock.Unlock();
			}
			return retVal;
		}

		public int SendToExistingConnection(TcpConnectedClient client, byte[] packet, int offset, int size)
		{
			int retVal = 0;
			try
			{
				// Verify Connection Is Still Open Before Sending
				retVal = client.Send(packet, offset, size);
			}
			catch (Exception e)
			{
				// Notify Caller Of Exception
				RaiseException(e);
				retVal = -1;  // Indicate An Error Occurred
			}
			return retVal;
		}

		public bool ClientExists(IPv4AddressPort address)
		{
			bool bRet;

			_clientLock.Lock();
			try
			{
				bRet = _clientList.ContainsKey(address.Address);
			}
			finally
			{
				_clientLock.Unlock();
			}

			return bRet;
		}

		#endregion

		#region Protected Access Methods

		protected internal void RaiseException(Exception e)
		{
			if (TcpException != null)
			{
				TcpException(e);
			}
		}

		#endregion

		#region Thread Overrides

		protected override bool OnStart()
		{
			bool bRet = false;

			try
			{
				StringBuilder ports = new StringBuilder();

				foreach (TcpListener listener in _listeners.Values)
				{
					ports.AppendFormat("{0},", ((IPEndPoint)listener.LocalEndpoint).Port);
				}
				Log.LogText(LogLevel.INFO, "{0} Starting - listening on port(s) {1}", Name, ports.ToString().TrimEnd(new char[] { ',' }));

				// Create listeners
				foreach (TcpListener listener in _listeners.Values)
				{
					StartListener(listener);
				}

				Interval = TimeSpan.FromMilliseconds(1000);
				ShuttingDown = false;  // Well yeah....
				bRet = true;
			}
			catch (Exception e)
			{
				Log.LogText(LogLevel.ERROR, "{0} Could not be started {1}", Name, e.Message);
			}
			return bRet;
		}

		protected override bool OnRun()
		{
			return true;
		}

		protected override bool OnStop()
		{
			// Close all listeners and clients
			Shutdown();

			return base.OnStop();
		}

		#endregion

		#region Listener Creation

		void StartListener(TcpListener listener)
		{
			try
			{
				// Set socket options
				listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				// Start Listening
				listener.Start();
				listener.BeginAcceptSocket(new AsyncCallback(OnConnectionRequestReceived), listener);

				// Log the start
				Log.LogText(LogLevel.DEBUG, "{0} Listening at {1}", Name, listener.LocalEndpoint);
			}
			catch (Exception e)
			{
				Log.LogText(LogLevel.ERROR, "{0} Could not be started {1}", Name, e.Message);
			}
		}

		#endregion

		#region Asynch Callbacks

		private void OnConnectionRequestReceived(IAsyncResult ar)
		{
			TcpListener tcpServer = null;

			try
			{
				if ((State == ThreadState.Started || State == ThreadState.Starting) && ShuttingDown == false)
				{
					tcpServer = (TcpListener)ar.AsyncState;
					TcpClient tcpClient = tcpServer.EndAcceptTcpClient(ar);

					if (_connectionDebugLogging)
					{
						Log.LogText(LogLevel.DEBUG, "{0} Incoming connection from {1}", Name, tcpClient.Client.RemoteEndPoint);
					}

					// Check If Connection From This Client Is Allowed
					if (ConnectionAllowed(tcpClient))
					{
						// Set Client KeepAlive Parms
						if (_keepAliveInterval != 0)
						{
							byte[] outValue = BitConverter.GetBytes(_keepAliveInterval);
							byte[] options = new byte[12];

							// enable/disable
							BinaryConverter.UInt32ToByteArrayHostOrder((UInt32)1, ref options, 0);
							// idle timeout
							BinaryConverter.UInt32ToByteArrayHostOrder(_keepAliveInterval, ref options, 4);
							// poll interval
							BinaryConverter.UInt32ToByteArrayHostOrder(_keepAliveInterval, ref options, 8);

							/** added for mono compatibility */
							try
							{
								tcpClient.Client.IOControl(IOControlCode.KeepAliveValues, options, outValue);
							}
							catch (Exception e)
							{
								Log.LogText(LogLevel.WARNING, "Could not set Keep-Alive: {0}", e.Message);
							}
						}

						// Set Send timeout
						tcpClient.Client.SendTimeout = _sendTimeout;

						// Set Liinger option
						tcpClient.Client.LingerState = new LingerOption(_lingerTime == 0 ? false : true, _lingerTime);

						TotalConnectCount++;

						// Add The To The List Of Clients
						//      All disconnected will be shutdown, closed and disposed 
						//      after each connection is lost and when the server is stopped
						TcpConnectedClient newClient = OnConnectionReceived(tcpClient, tcpServer);
						TcpConnectedClient existingClient = null;
						if (newClient != null)
						{
							try
							{
								_clientLock.Lock();

								// Remove existing client with same IPV4:Port
								if (_clientList.TryGetValue(newClient.RemoteAddress.Address, out existingClient))
								{
									_clientList.Remove(existingClient.RemoteAddress.Address);
								}
								else
								{
									existingClient = null;
								}

								_clientList.Add(newClient.RemoteAddress.Address, newClient);
							}
							catch (Exception e)
							{
								Log.LogText(LogLevel.WARNING, "{0} Failure Adding Client {1} {2}", Name, newClient.RemoteAddress.Address, e.Message);
							}
							finally
							{
								_clientLock.Unlock();
							}

							// Close existing client connection
							if (existingClient != null)
							{
								Log.LogText(LogLevel.DEBUG, "{0} Removing and Closing Existing Client {1} closed", Name, existingClient.RemoteAddress.Address);
								existingClient.Close();
							}

							// Raise New Connection Event
							OnConnectionOpened(newClient);

							if (_connectionDebugLogging)
							{
								Log.LogText(LogLevel.DEBUG, "{0} Connection Accepted {1}", Name, newClient.RemoteAddress.Address);
							}

							/** start receive unless our clients will be handling their own I/O */
							if (_clientsHandleIO == false)
							{
								newClient.BeginReceive();
							}
						}
					}
					else
					{
						Log.LogText(LogLevel.INFO, "{0} Connection Not Allowed {1}", Name, tcpClient.Client.RemoteEndPoint);
						// Reject The Connection
						tcpClient.Close();
					}
				}
				else
				{
					Log.LogText(LogLevel.DEBUG, "{0} Was Shutdown", Name);
				}
			}
			catch (Exception ex)
			{
				Log.LogText(LogLevel.ERROR, "{0} OnConnectionRequestReceived EXCEPTION: {1}", Name, ex.Message);
				RaiseException(ex);
			}

			if (tcpServer != null && tcpServer.Server != null && tcpServer.Server.IsBound && (State == ThreadState.Started || State == ThreadState.Starting))
			{
				tcpServer.BeginAcceptSocket(new AsyncCallback(OnConnectionRequestReceived), tcpServer);
			}
		}

		#endregion

		#region Abstract / Virtual Methods
		//
		// Methods To Be Overridden in Derived Class
		//
		protected abstract TcpConnectedClient OnConnectionReceived(TcpClient client, TcpListener listener);

		protected virtual bool ConnectionAllowed(TcpClient tcpClient)
		{
			return true;
		}

		protected virtual void OnConnectionOpened(TcpConnectedClient client)
		{
			//Log.LogText(LogLevel.INFO, "{0} Client {1} opened", Name, client);

			if (ConnectionOpened != null)
			{
				ConnectionOpened(client);
			}
		}

		#endregion
	}
}
