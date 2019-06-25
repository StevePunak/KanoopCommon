using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System;
using System.IO;
using System.Net;
using KanoopCommon.Logging;

namespace KanoopCommon.Extensions
{
	public static class SocketExtensions
	{
		/// <summary>
		/// Sets the keep-alive interval for the socket.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="standardInterval">Time between two keep alive "pings".</param>
		/// <param name="retryInterval">Time between two keep alive "pings" when first one fails.</param>
		/// <returns>If the keep alive infos were succefully modified.</returns>
		public static bool EnableKeepAlive(this Socket socket, TimeSpan standardInterval, TimeSpan retryInterval)
		{
			return SetKeepAlive(socket, standardInterval, retryInterval);
		}

		public static bool DisableKeepAlive(this Socket socket)
		{
			return SetKeepAlive(socket, TimeSpan.Zero, TimeSpan.Zero);
		}

		private static bool SetKeepAlive(Socket socket, TimeSpan standardInterval, TimeSpan retryInterval)
		{
			int result = 0;

			try
			{
				byte[] inValue = null;
				using(MemoryStream ms = new MemoryStream(sizeof(UInt32) * 3))
				using(BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write((standardInterval == TimeSpan.Zero || retryInterval == TimeSpan.Zero) ? (UInt32)0 : (UInt32)1);
					bw.Write((UInt32)standardInterval.TotalMilliseconds);
					bw.Write((UInt32)retryInterval.TotalMilliseconds);
					inValue = ms.ToArray();
				}

				// Create bytestruct for result (bytes pending on server socket).
				byte[]	outValue = BitConverter.GetBytes((UInt32)0);

				// Write SIO_VALS to Socket IOControl.
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				socket.IOControl(IOControlCode.KeepAliveValues, inValue, outValue);

				using(MemoryStream ms = new MemoryStream(outValue))
				using(BinaryReader br = new BinaryReader(ms))
				{
					result = br.ReadInt32();
				}
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.WARNING, "EXCEPTION setting socket keepalive: {0}", e.Message);
				result = -1;
			}
			return result == 0;
		}
	}
}