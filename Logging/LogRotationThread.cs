using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Threading;
using System.IO;
using System.Diagnostics;

namespace KanoopCommon.Logging
{
	public class LogRotationThread : ThreadBase
	{
		Log					_logToRotate;
		StreamWriter		_fileWriter;
		MutexLock			_fileLock;

		public LogRotationThread(Log log, StreamWriter fileWriter, MutexLock fileLock)
			: base("LogRotationThread")
		{
			Interval = TimeSpan.FromMilliseconds(0);
			_logToRotate = log;
			_fileWriter = fileWriter;
			_fileLock = fileLock;
		}

		protected override bool OnRun()
		{
			RotateLogFile();

			_logToRotate.LogRotationComplete();

			return false;
		}

		void RotateLogFile()
		{
#if (!PocketPC)
			try
			{
				_fileLock.Lock();
				String strDestFileName = "";

				/** write a line out to signal rotation */
				if(_fileWriter != null)
				{
					_fileWriter.WriteLine(String.Format("Rotating Log File {0}", DateTime.Now.ToString("HH:mm:ss.fff ")));
					_fileWriter.Flush();
				}
			
				/**
					* Copy our current log file to a filename including the 
					* date and time it was rotated
					*/
				if(File.Exists(_logToRotate.FileName))
				{
					strDestFileName = String.Format("{0}-{1}.log", _logToRotate.FileName, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
					File.Copy(_logToRotate.FileName, strDestFileName);
					_fileWriter.BaseStream.SetLength(0);
				}
			}
			catch(Exception e)
			{
				_fileWriter.WriteLine(String.Format("EXCEPTION - LOG ROTATION FAILED: {0}", e.Message));
			}
			finally
			{
				_fileLock.Unlock();
			}

			/**
			 * we have to do this because of a bug in GetCreationTime where it returns
			 * the old creation time of a newly deleted file
			 */
#endif
		}
	
	}
}
