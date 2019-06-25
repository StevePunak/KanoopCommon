#undef ASYNC_READ
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Logging;
using KanoopCommon.Threading;

namespace KanoopCommon.Serial
{
	public delegate void DataReceivedHandler(byte[] buffer, int length);

	public class MonoSerialPort
	{
		public event DataReceivedHandler DataReceived;

		public String PortName { get; private set; }

		public SerialPort Port { get; private set; }

		public TimeSpan BufferTime { get; set; }
		public int BufferSize { get; set; }

		public int TotalBytesReceived { get; private set; }
		public int TotalDeliveries { get; private set; }
		byte[] ReceiveBuffer { get; set; }
		int BytesInReceiveBuffer { get; set; }

		bool IsUSB { get; set; }
		ReadDataThread _thread;

		public bool IsOpen { get { return Port.IsOpen; } }

		public MonoSerialPort(String portName)
		{
			PortName = portName;
			IsUSB = portName.Contains("/dev/ttyUSB");

			Port = new SerialPort(portName, 115200);
			Port.ReadTimeout = BufferTime != TimeSpan.Zero ? (int)BufferTime.TotalMilliseconds : 1000;

			TotalBytesReceived = TotalDeliveries = 0;
			if(BufferSize == 0)
			{
				BufferSize = 0xffff;
			}

			ReceiveBuffer = new byte[BufferSize * 2];
			BytesInReceiveBuffer = 0;

			Console.WriteLine("Opened {0}", portName);

			_thread = new ReadDataThread(this);

			DataReceived += delegate { };
		}

		class ReadDataThread : ThreadBase
		{
			MonoSerialPort _parent;
			DateTime _lastDelivery;

			public ReadDataThread(MonoSerialPort parent)
				: base(typeof(ReadDataThread).Name)
			{
				_parent = parent;
			}

			protected override bool OnRun()
			{
				try
				{
					int bytes = _parent.Port.Read(_parent.ReceiveBuffer, _parent.BytesInReceiveBuffer, _parent.ReceiveBuffer.Length - _parent.BytesInReceiveBuffer);
					if(bytes > 0)
					{
						_parent.TotalBytesReceived += bytes;
						_parent.BytesInReceiveBuffer += bytes;

						if(DateTime.UtcNow > _lastDelivery + _parent.BufferTime)
						{
							DeliverData();
						}
					}
					else
					{
						if(_parent.BytesInReceiveBuffer > 0)
						{
							DeliverData();
						}
						//Console.WriteLine("read timed out");
					}
				}
				catch(Exception e)
				{
					if(_parent.BytesInReceiveBuffer > 0)
					{
						DeliverData();
					}
					//Console.WriteLine(e.Message);
				}
				return true;
			}

			void DeliverData()
			{
				_parent.TotalDeliveries++;
				byte[] receiveData = new byte[_parent.BytesInReceiveBuffer];
				Array.Copy(_parent.ReceiveBuffer, 0, receiveData, 0, _parent.BytesInReceiveBuffer);
				_parent.DataReceived(receiveData, receiveData.Length);

				//Log.SysLogText(LogLevel.DEBUG, "Delivered {0} bytes:", receiveData.Length);
				//							Log.SysLogHex(LogLevel.DEBUG, receiveData, 0, bytes);

				// clear receive buffer
				_lastDelivery = DateTime.UtcNow;
				_parent.BytesInReceiveBuffer = 0;
			}

			protected override bool OnException(Exception e)
			{
				return true;
			}
		}

		public void Open()
		{
			Port.Open();

#if ASYNC_READ
			BeginRead();
#else
			_thread.Start();
#endif
		}

		public void Close()
		{
#if !ASYNC_READ
			Port.BaseStream.Close();
			Log.SysLogText(LogLevel.DEBUG, "Aborting {0}", _thread);
			_thread.Stop();
			Log.SysLogText(LogLevel.DEBUG, "Thread stopped");

			// _thread.Stop(500);
#endif
		}

		public void BeginRead()
		{
			Port.BaseStream.BeginRead(ReceiveBuffer, 0, ReceiveBuffer.Length, ReadDataCallback, this);
		}

		private void ReadDataCallback(IAsyncResult ar)
		{
			try
			{
				int bytes = Port.BaseStream.EndRead(ar);

				if(bytes > 0)
				{
					Log.SysLogText(LogLevel.DEBUG, "ASYNC RECV {0} bytes:", bytes);
					Log.SysLogHex(LogLevel.DEBUG, ReceiveBuffer, 0, bytes);
					DataReceived(ReceiveBuffer, bytes);
					BeginRead();
				}
				else
				{
					Console.WriteLine("read timed out 2");
				}
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.WARNING, "Serial port exception: {0}", e.Message);
			}
		}

		public void Write(byte[] buffer)
		{
			Write(buffer, 0, buffer.Length);
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			Port.Write(buffer, offset, count);
		}

	}
}
