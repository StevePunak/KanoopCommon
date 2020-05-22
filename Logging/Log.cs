using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
// using System.Diagnostics;
using System.Reflection;
using System.Net.Sockets;
using System.Net;

using KanoopCommon.Threading;
using System.Globalization;
using System.Security.Permissions;
using KanoopCommon.Addresses;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Logging
{
	public delegate void LogOutputHandler(LogLevel logLevel, String output);
	public delegate void LogErrorOrFatalHandler(LogLevel logLevel, String message);

	public class Log
	{
		#region Events

		public static event LogOutputHandler SysLogDataReceived;
		public event LogOutputHandler LogDataOutput;

		#endregion

		#region Constants

		public const int        DEFAULT_ROTATION_SIZE = 10;
		public const int        DEFAULT_ROTATION_HOURS = 24;

		public const UInt32     DEFAULT_MAX_NETWORK_BPS = 3000;

		public const int LOG_OUTPUT_DESTINATION_FLAGS =             0x000000FF;
		public const int LOG_OUTPUT_CONTENT_FLAGS =                 0x0000FF00;

		/**
		 *  Network Logging Settings
		 */
		const String DEFAULT_MULTICAST_ADDRESS =            "226.200.0.1:2468";

		const UInt16 LOG_DEFAULT_PORT =                     2468;           // default port for network logging

		public const UInt16 MLOG_BASE_PORT =                12800;

		private static String[] _levelText =
		{
			"",
			"DBG", 		//	LOG_LVL_DEBUG 		= 1,
			"INF",		//	LOG_LVL_INFO		= 2,
			"WRN",		//	LOG_LVL_WARNING		= 3,
			"ERR",		//	LOG_LVL_ERROR		= 4,
			"FAT"		//	LOG_LVL_FATAL		= 5,
		};

		private static Dictionary<String, LogLevel> s_LevelMap = new Dictionary<String, LogLevel>()
		{
			{ "DBG", LogLevel.DEBUG },
			{ "INF", LogLevel.INFO },
			{ "WRN", LogLevel.WARNING },
			{ "ERR", LogLevel.ERROR },
			{ "FAT", LogLevel.FATAL }
		};

		public static readonly List<String> LevelStringList = new List<String>(s_LevelMap.Keys);

		#endregion

		#region Internal Classes

		class BandwidthTrackingList : List<BandwidthTrackingEntry> { }
		class BandwidthTrackingEntry
		{
			DateTime            m_Timestamp;
			public DateTime Timestamp { get { return m_Timestamp; } }

			int     m_nBytes;
			public int Bytes { get { return m_nBytes; } }

			public BandwidthTrackingEntry(int nBytes)
			{
				m_Timestamp = DateTime.Now;
				m_nBytes = nBytes;
			}
		}

		class DailyRotateTime
		{
			int _hour;
			public int Hour { get { return _hour; } }

			int _minute;
			public int Minute { get { return _minute; } }

			public DailyRotateTime(int h, int m)
			{
				_hour = h;
				_minute = m;
			}
		}

		#endregion

		#region Private Member Variables

		FileStream                              _fileStream;
		AsynchLogThread                         _asynchWriter;
		StreamWriter                            _fileTextWriter;
		BinaryWriter                            _binaryWriter;
		TextWriter                              _consoleWriter;

		MemoryLog                               _memoryLog;

		UInt32                                  _logFlags;

		bool                                    _logging;
		DateTime                                _rotateTime;
		MutexLock                               _lock = new MutexLock();

		List<String>                            _outputQueue;
		bool                                    _queueing;

		#endregion

		#region Public Properties

		LogLevel                                _level;
		public LogLevel LogLevel
		{
			get { return _level; }
			set { _level = value; }
		}

		String                                  _fileName;
		public String FileName
		{
			get { return _fileName; }
		}

		static Log                          _systemLog = null;
		public static Log SystemLog
		{
			get
			{
				if(_systemLog == null)
				{
					Log log = new Log();
					log.Open(LogLevel.ALWAYS, OpenFlags.COMBO_VERBOSE | OpenFlags.OUTPUT_TO_CONSOLE | OpenFlags.OUTPUT_TO_DEBUG);
					_systemLog = log;
				}
				return _systemLog;
			}
			set { _systemLog = value; }
		}

		TimeSpan                                _rotationInterval;
		public TimeSpan RotationInterval
		{
			get { return _rotationInterval; }
			set
			{
				_rotationInterval = value;
				_rotateTime = _logCreateTime.Add(_rotationInterval);
			}
		}

		DateTime                                    _logCreateTime;
		public DateTime CreateTime
		{
			get { return _logCreateTime; }
		}

		public bool _UTCTimestamps;
		public bool UTCTimestamps
		{
			get { return _UTCTimestamps; } set { _UTCTimestamps = value; }
		}

		public UInt32 Flags { get { return _logFlags; } }

		public LogLevel MinimumLevelForHeader { get; set; }

		public bool LogMicroseconds { get; set; }

		#endregion

		public Log()
			: this(new TimeSpan(DEFAULT_ROTATION_HOURS, 0, 0)) { }

		public Log(TimeSpan rotationInterval)
		{
			_rotationInterval = rotationInterval;

			_outputQueue = new List<string>();
			_queueing = false;
			_UTCTimestamps = true;

			_logCreateTime = DateTime.Now;
			MinimumLevelForHeader = Logging.LogLevel.ALWAYS;
		}

		public bool Open(LogLevel logLevel, String filename, UInt32 flags, IPv4AddressPort multicastAddress)
		{
			if(filename != null)
			{
				_fileName = filename;
			}

			/** setting the flags will open all our output sources */
			_logging = SetFlags(logLevel, flags);

			return _logging;
		}

		public bool Open(LogLevel logLevel, String filename, UInt32 flags)
		{
			return Open(logLevel, filename, flags, new IPv4AddressPort(DEFAULT_MULTICAST_ADDRESS));
		}

		public bool Open(LogLevel logLevel, UInt32 flags)
		{
			return Open(logLevel, null, flags, new IPv4AddressPort(DEFAULT_MULTICAST_ADDRESS));
		}

		public bool Open(LogLevel logLevel)
		{
			return Open(logLevel, OpenFlags.COMBO_VERBOSE | OpenFlags.OUTPUT_TO_CONSOLE | OpenFlags.OUTPUT_TO_DEBUG);
		}

		public bool Open()
		{
			return Open(LogLevel.ALWAYS);
		}


		public void Close()
		{
			if(_logging)
			{
				_logging = false;

				_lock.Lock();

				if(this == _systemLog)
				{
					_systemLog = null;
				}

				CloseLogFile();
				CloseLogConsole();

				_lock.Unlock();
			}
		}

		public void LogBanner(String extraString)
		{
			AssemblyTitleAttribute title = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
			AssemblyDescriptionAttribute desc = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>();
			AssemblyCompanyAttribute company = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCompanyAttribute>();
			AssemblyCopyrightAttribute copyright = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>();
			SoftwareVersion version = new SoftwareVersion(Assembly.GetEntryAssembly().GetName().Version);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(String.Empty.PadRight(84, '*'));
			String nameString = $"{title.Title} {version}  ({extraString})";
			sb.AppendLine($"* {nameString,-80}*");
			String authorString = $"{company.Company} {copyright.Copyright}";
			sb.AppendLine($"* {desc.Description,-80}*");
			sb.AppendLine(String.Empty.PadRight(84, '*'));
			String output = sb.ToString();
			LogToDevices(LogLevel.ALWAYS, output);
		}

		public void LogText(LogLevel logLevel, String format, params object[] parms)
		{
			uint flags = _logFlags;

			if(MinimumLevelForHeader >= logLevel)
			{
				flags &= ~(OpenFlags.CONTENT_TIMESTAMP | OpenFlags.CONTENT_DATESTAMP | OpenFlags.CONTENT_PRINT_LEVEL);
			}

			LogText(flags, logLevel, format, parms);
		}

		private void LogText(uint logFlags, LogLevel logLevel, String format, params object[] parms)
		{
			Exception reThrow = null;

			if((_logging) && (logLevel == LogLevel.ALWAYS || logLevel >= _level) && logLevel != LogLevel.NOTHING)
			{
				_lock.Lock();
				{
					try
					{
						DateTime sysTime = _UTCTimestamps ? DateTime.UtcNow : DateTime.Now;

						String leading = "";

						// -----------------------------------------------------------------------
						// add thread ID, if required
						// -----------------------------------------------------------------------
						if((logFlags & OpenFlags.CONTENT_THREAD_NAME) != 0)
						{
							if(Thread.CurrentThread.Name == null)
							{
								leading += String.Format("[{0}]", Thread.CurrentThread.GetHashCode());
							}
							else
							{
								leading += String.Format("[{0}]", Thread.CurrentThread.Name);
							}
						}

						// -----------------------------------------------------------------------
						// put date on, if required
						// -----------------------------------------------------------------------
						if((logFlags & OpenFlags.CONTENT_DATESTAMP) != 0)
						{
							leading += sysTime.ToString("MMM dd ");
						}

						// -----------------------------------------------------------------------
						// add timestamp, if required
						// -----------------------------------------------------------------------
						if((logFlags & OpenFlags.CONTENT_TIMESTAMP) != 0)
						{
							leading += sysTime.ToString(LogMicroseconds ? "HH:mm:ss.ffffff " : "HH:mm:ss.fff ");
						}

						/**
						 * add source file & line #
						 */
						if((logFlags & OpenFlags.CONTENT_LINE_NUMBERS) != 0)
						{
							leading += GetSourceModuleInfo();
							leading += ' ';
						}

						if((logFlags & OpenFlags.CONTENT_PRINT_LEVEL) != 0)
						{
							leading += (logLevel <= LogLevel.FATAL) ? _levelText[(int)logLevel] : "???";
							leading += ' ';
						}

						String output = parms.Length == 0
							? leading + format
							: leading + String.Format(format, parms);

						LogToDevices(logLevel, output);
					}
					catch(FormatException e)
					{
						Console.WriteLine("Illegal format '{0}'   {1}", format, e.Message);
					}
					catch(Exception e)
					{
						reThrow = e;
					}
					_lock.Unlock();
				}
			}

			if(reThrow != null)
			{
				throw reThrow;
			}
		}

		public static void SysLogText(LogLevel logLevel, String text, params object[] parms)
		{
			if(_systemLog != null)
			{
				_systemLog.LogText(logLevel, text, parms);
			}
			else
			{
				String output = String.Format(text, parms);
				output = String.Format("SYSTEMLOG: {0} {1} {2}", DateTime.UtcNow.ToString("HH:mm:ss.fff "), logLevel.ToString(), output);
				Console.WriteLine(output);
			}
		}

		public void LogHex(LogLevel logLevel, byte[] data, int start, int count)
		{
			Exception reThrow = null;
			StringBuilder strOut = new StringBuilder(count);
			int nLast = start + count;
			if(_logging && logLevel >= _level)
			{
				int	nOffset;
				_lock.Lock();
				{
					try
					{
						for(nOffset	= start;nOffset < nLast;nOffset +=	16)
						{
							int	x;
			
							strOut.AppendFormat("      {0:x6}: ",	nOffset - start);
							for(x =	nOffset;x <	nOffset	+ 16 &&	x <	nLast;x++)
							{
								strOut.AppendFormat("{0:x2} ", data[x]);
							}
							while(strOut.Length < 65)
							{
								strOut.Append(' ');
							}

							for(x =	nOffset;x <	nOffset	+ 16 &&	x <	nLast;x++)
							{
								strOut.Append((data[x] < 128 && data[x] > 0x20) ? Convert.ToChar(data[x]) : '.');
							}
							LogToDevices(logLevel, strOut.ToString());
							strOut = new StringBuilder(count);
	/**						if(m_pCallback)
								m_pCallback(zSourceFile, nLine, logLevel, m_zLogOutBuffer);*/
						}
					}
					catch(Exception e)
					{
						reThrow = e;
					}
					_lock.Unlock();
				}
			}

			if(reThrow != null)
			{
				throw reThrow;
			}
		}

		public void LogHex(LogLevel logLevel, byte[] data)
		{
			LogHex(logLevel, data, 0, data.Length);
		}

		public static void SysLogHex(LogLevel logLevel, byte[] data, int start, int count)
		{
			if(_systemLog != null)
			{
				_systemLog.LogHex(logLevel, data, start, count);
			}
		}

		public static void SysLogHex(LogLevel logLevel, byte[] data)
		{
			SysLogHex(logLevel, data, 0, data.Length);
		}

		public static void SysLogByteArray(LogLevel level, byte[] data)
		{
			_systemLog.LogByteArray(level, data, 0, data.Length);
		}

		public static void SysLogByteArray(LogLevel level, byte[] data, int start, int count)
		{
			_systemLog.LogByteArray(level, data, start, count);
		}

		public void LogByteArray(LogLevel logLevel, byte[] data, int start, int count)
		{
			const int BYTES_PER_LINE = 8;

			Exception reThrow = null;
			int nLast = start + count;
			if(_logging && logLevel >= _level)
			{
				StringBuilder sb = new StringBuilder(count);
				sb.Append("byte[] __xxx = new byte[]\n{\n");
				int	nOffset;
				_lock.Lock();
				{
					try
					{
						for(nOffset	= start;nOffset < nLast;nOffset +=	BYTES_PER_LINE)
						{
							int	x;
			
							for(x =	nOffset;x <	nOffset	+ BYTES_PER_LINE &&	x <	nLast;x++)
							{
								sb.AppendFormat("0x{0:x2}", data[x]);
								if(x != nLast - 1)
								{
									sb.Append(", ");
								}
							}

						}
					}
					catch(Exception e)
					{
						reThrow = e;
					}
					_lock.Unlock();
				}

				sb.Append("\n};");
				LogToDevices(logLevel, sb.ToString());
			}

			if(reThrow != null)
			{
				throw reThrow;
			}
		}

		public void LogByteArray(LogLevel logLevel, byte[] data)
		{
			LogByteArray(logLevel, data, 0, data.Length);
		}

		public void LogBinary(LogLevel logLevel, String strLabel, byte[] data)
		{
			LogBinary(logLevel, strLabel, data, 0, data.Length);
		}

		public void LogBinary(LogLevel logLevel, String strLabel, byte[] data, int nStart, int nCount)
		{
			Exception reThrow = null;
			/** fow now, binary only goes to file */
			if(	_logging && logLevel >= _level 
				&& _fileStream != null &&
				(_logFlags & OpenFlags.OUTPUT_TO_FILE) != 0)
			{
				_lock.Lock();
				{
					try
					{
						if((_logFlags & OpenFlags.OUTPUT_TO_ASYNCH) == 0)
						{
							String strOutput = String.Format("[{0} b:{1}/{2}]\n", DateTime.Now.ToString("HH:mm:ss.fff"), nCount, strLabel);
							_binaryWriter.Write(ASCIIEncoding.UTF8.GetBytes(strOutput));
							_binaryWriter.Write(data, nStart, nCount);
							_binaryWriter.Write('\x0a');
							_fileTextWriter.Flush();
							_binaryWriter.Flush();
						}
						else
						{
							_asynchWriter.AddBinaryData(strLabel, data, nStart, nCount);
						}
					}
					catch(Exception e)
					{
						reThrow = e;
					}
					_lock.Unlock();
				}
			}

			if(reThrow != null)
			{
				throw reThrow;
			}
		}

		public static void LogToConsoleAndDebug(String format, params object[] parms)
		{
			String dateString = DateTime.Now.ToString("HH:mm:ss.fff ");
			String output = parms != null
				? String.Format(format, parms)
				: format;
#if USE_DEBUG_OUTPUT
			Debug.WriteLine("{0}{1}", dateString, output);
#endif
			Console.WriteLine("{0}{1}", dateString, output);
		}

		public void WriteMemoryLog(String fileName)
		{
			if((_logFlags & OpenFlags.OUTPUT_TO_MEMORY) != 0 && _memoryLog != null)
			{
				_memoryLog.WriteToFile(fileName);
			}
		}

		void LogToDevices(LogLevel level, String text)
		{
			if((_logFlags & OpenFlags.OUTPUT_TO_ASYNCH) == 0)
			{
				LogToNoFailDevices(level, text);
			}
			else
			{
				_asynchWriter.AddLogLine(text);
			}

			if(LogDataOutput != null)
			{
				LogDataOutput(level, text);
			}

		}

		void LogToNoFailDevices(LogLevel level, String text)
		{
			if((_logFlags & OpenFlags.OUTPUT_TO_FILE) != 0)
				LogToFile(text);
		
			if((_logFlags & OpenFlags.OUTPUT_TO_CONSOLE) != 0)
				LogToConsole(text);
		
			if((_logFlags & OpenFlags.OUTPUT_TO_DEBUG) != 0)
				LogToDebug(text);

			if((_logFlags & OpenFlags.OUTPUT_TO_MEMORY) != 0)
				LogToMemory(text);

			if(SysLogDataReceived!= null)
				SysLogDataReceived(level, text);
		}

		/**
		 *
		 * @b         LogToConsole
		 *
		 * send string to console (stdout)
		 *
		 * @param           szLogText   - zero terminated text string
		 *
		 * @return          void
		 *
		 */
		void LogToConsole(String strLogText)
		{
			if(_consoleWriter != null)
			{
				_consoleWriter.WriteLine(strLogText);
			}
		}
		

		void LogToDebug(String strLogText)
		{
			System.Diagnostics.Debug.WriteLine(strLogText);
		}

		void LogToMemory(String strLogText)
		{
			_memoryLog.WriteLine(strLogText);
		}

		public void SetDailyRotation(int nHour, int nMinute)
		{
			_rotateTime = new DateTime(_rotateTime.Year, _rotateTime.Month, _rotateTime.Day, nHour, nMinute, 0);
			_rotationInterval = new TimeSpan(24, 0, 0);
		}
		

		/**
		 *
		 * @b         GetFlags
		 *
		 * Get current logging level and flags
		 *
		 *
		 * @param           pLevel      - Output. Current Log LogLevel
		 * @param           pFlags      - Output. Current Log Flags
		 *
		 * @return          bool
		 *
		 */
		public bool GetFlags(out LogLevel level, out UInt32 flags)
		{
			bool bRet = false;
			level = LogLevel.NOTHING;
			flags = 0;
			if(_logging)
			{
				level = _level;
				flags = _logFlags;
				bRet = true;
			}
			return bRet;
		}

		public void SetLogLevel(LogLevel level)
		{
			_level = level;
		}

		public bool SetFlags(LogLevel level, UInt32 flags)
		{
			bool result = true;

			if((flags & (UInt32)OpenFlags.OUTPUT_TO_FILE) != 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_FILE) == 0)
			{
				if(OpenLogFile(_fileName))
					_logFlags |= (UInt32)OpenFlags.OUTPUT_TO_FILE;
				else
					result = false;
			}
			else if((flags & (UInt32)OpenFlags.OUTPUT_TO_FILE) == 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_FILE) != 0)
			{
				CloseLogFile();
				_logFlags &= ~(UInt32)OpenFlags.OUTPUT_TO_FILE;
			}

			if((flags & (UInt32)OpenFlags.OUTPUT_TO_CONSOLE) != 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_CONSOLE) == 0)
			{
				if(OpenLogConsole())
					_logFlags |= (UInt32)OpenFlags.OUTPUT_TO_CONSOLE;
				else
					result = false;
			}
			else if((flags & (UInt32)OpenFlags.OUTPUT_TO_CONSOLE) == 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_CONSOLE) != 0)
			{
				CloseLogConsole();
				_logFlags &= ~(UInt32)OpenFlags.OUTPUT_TO_CONSOLE;
			}
		
			if((flags & (UInt32)OpenFlags.OUTPUT_TO_ASYNCH) != 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_ASYNCH) == 0)
			{
				_asynchWriter = new AsynchLogThread(_fileTextWriter, _binaryWriter);
				_asynchWriter.Start();

				_logFlags |= (UInt32)OpenFlags.OUTPUT_TO_ASYNCH;
			}

			if((flags & (UInt32)OpenFlags.OUTPUT_TO_DEBUG) != 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_DEBUG) == 0)
			{
				_logFlags |= (UInt32)OpenFlags.OUTPUT_TO_DEBUG;
			}
			else if((flags & (UInt32)OpenFlags.OUTPUT_TO_DEBUG) == 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_DEBUG) != 0)
			{
				_logFlags &= ~(UInt32)OpenFlags.OUTPUT_TO_DEBUG;
			}

			if((flags & (UInt32)OpenFlags.OUTPUT_TO_MEMORY) != 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_MEMORY) == 0)
			{
				if(OpenLogMemory())
					_logFlags |= (UInt32)OpenFlags.OUTPUT_TO_MEMORY;
				else
					result = false;
			}
			else if((flags & (UInt32)OpenFlags.OUTPUT_TO_MEMORY) == 0 && (_logFlags & (UInt32)OpenFlags.OUTPUT_TO_MEMORY) != 0)
			{
				CloseLogMemory();
				_logFlags &= ~(UInt32)OpenFlags.OUTPUT_TO_MEMORY;
			}

			_logFlags = (UInt32)((_logFlags & (~LOG_OUTPUT_CONTENT_FLAGS))) | (flags & LOG_OUTPUT_CONTENT_FLAGS);
			
			_level =			level;

			return result;
		}	
		
		bool OpenLogFile(String fileName)
		{
			bool bRet = false;
			try
			{

				/** get rid of unwanted characters */
				fileName = String.Format("{0}{1}", fileName.Substring(0, 2), fileName.Substring(2).Replace(":", String.Empty)); 

				/**
				 * Create the path if it does not exist
				 */
				String directory = Path.GetDirectoryName(fileName);
				if(directory != null && directory.Length > 0 && Directory.Exists(directory) == false)
				{
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fileName));
					}
					catch(Exception)
					{
						fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), Path.GetFileName(fileName));
					}

				}

				if(File.Exists(fileName))
				{
					/** open the file */
					_fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

					/** try and find a timestamp for the first entry in the log */
					DateTime firstEntryTime = DateTime.MinValue;
					TextReader tr = new StreamReader(_fileStream);
					String line;
					int linesToParse = 100;
					do
					{
						line = tr.ReadLine();
						if(line != null && line.Length > 15)
						{
							DateTime.TryParseExact(line.Substring(0, 15), "MMM dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out firstEntryTime);
						}
					}while(--linesToParse > 0 && line != null && firstEntryTime == DateTime.MinValue);
										
					if (firstEntryTime > DateTime.Now)
					{
						DateTime fileCreateTime = File.GetCreationTime(fileName);																		
						firstEntryTime = new DateTime(fileCreateTime.Year, firstEntryTime.Month, firstEntryTime.Day);						
					}

					/** if we have a time from the file, we use that as the create date, otherwise, just use 'now' */
					_logCreateTime = firstEntryTime != DateTime.MinValue ? firstEntryTime : DateTime.Now;
					_rotateTime = _logCreateTime.Add(_rotationInterval);

					/** go to the end of the file */
					_fileStream.Seek(0, SeekOrigin.End);

					_fileTextWriter = new StreamWriter(_fileStream);
					_binaryWriter = new BinaryWriter(_fileStream);

				}
				else
				{
					_fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
					_fileTextWriter = new StreamWriter(_fileStream);
					_binaryWriter = new BinaryWriter(_fileStream);
					_rotateTime = DateTime.Now + _rotationInterval;
				}

				bRet = true;
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.FATAL, "CAN NOT OPEN LOG FILE: {0}", e.Message);
			}
			return bRet;
		}

		/**
		 *
		 * @b         LogToFile
		 *
		 * Write string to file
		 *
		 * @param           strLogText   - zero terminated text string
		 *
		 * @return          void
		 *
		 */
		void LogToFile(String text)
		{
			if(_queueing == false)
			{
				if(_fileTextWriter != null)
				{
					if(DateTime.Compare(DateTime.Now, _rotateTime) > 0)
					{
						_fileTextWriter.WriteLine("Start log rotation at {0} with the following line: {1}", DateTime.Now.ToString("MMM dd HH:mm:ss.fff"), text);
						_fileTextWriter.Flush();

						/** disallow use of the file stream... all output will be queued */
						_queueing = true;

						/** start a thread which will rotate the log for us */
						LogRotationThread thread = new LogRotationThread(this, _fileTextWriter, _lock);
						thread.Start();

						_outputQueue.Add(text);
					}
					else
					{

						/** flush any queued output */
						if(_outputQueue.Count > 0)
						{
							_fileTextWriter.WriteLine("Post-rotation - flushing {0} lines of queued output at {1}",
								_outputQueue.Count, DateTime.Now.ToString("MMM dd HH:mm:ss.fff "));

							while(_outputQueue.Count > 0)
							{
								String strOut = _outputQueue[0];
								_outputQueue.RemoveAt(0);
								_fileTextWriter.WriteLine(strOut);
							}
						}
						/** write the current text, and flush output */
						_fileTextWriter.WriteLine(text);
						_fileTextWriter.Flush();
					}
				}
			}
			else
			{
				_outputQueue.Add(text);
			}
		}

		internal void LogRotationComplete()
		{
			_rotateTime = DateTime.Now.Add(_rotationInterval);
			_queueing = false;
		}
		
		/**
		 *
		 * @b         CloseLogConsole
		 *
		 * Close output file
		 *
		 *
		 * @return          void
		 */
		void CloseLogFile()
		{
			if(_fileTextWriter != null)
			{
				_fileTextWriter.Close();
				_fileTextWriter = null;
			}
		}	
		
		bool OpenLogConsole()
		{
			bool bRet = false;
			try
			{
				_consoleWriter = Console.Error;
				bRet = true;
			}
			catch
			{
			}
			return bRet;
		}

		void CloseLogConsole()
		{
			if(_consoleWriter != null)
			{
				_consoleWriter.Close();
				_consoleWriter = null;
			}
		}

		bool OpenLogMemory()
		{
			bool bRet = false;
			try
			{
				_memoryLog = new MemoryLog();
				bRet = true;
			}
			catch
			{
			}
			return bRet;
		}

		void CloseLogMemory()
		{
			if(_memoryLog != null)
			{
				_memoryLog.Stop();
				_memoryLog = null;
			}
		}

		String GetSourceModuleInfo()
		{
			String strRet = "";
#if USE_EVENT_LOG
			StackTrace stackTrace = new StackTrace(true);
			StackFrame[] stackFrames = stackTrace.GetFrames();

			bool bFirst = true;
			foreach(StackFrame stackFrame in stackFrames)
			{
				String strFileName = stackFrame.GetFileName();
				if(bFirst == false && strFileName != null && strFileName.IndexOf("Logging") == -1)
				{
					String [] strSplit = strFileName.Split('\\');
					strRet = String.Format("{0} {1}", strSplit[strSplit.Length-1].PadLeft(20,' '), stackFrame.GetFileLineNumber().ToString().PadLeft(5,' '));
					break;
				}
				bFirst = false;
			}
#endif
			return strRet;
		}
		
		public static bool TryParseLevel(String str, out LogLevel level)
		{
			return s_LevelMap.TryGetValue(str, out level);
		}

		public static string GetAbbreviation(LogLevel level)
		{
			return _levelText[(int)level];
		}

		public override string ToString()
		{
			return String.Format("Log {0}", GetHashCode());
		}
	}


}

