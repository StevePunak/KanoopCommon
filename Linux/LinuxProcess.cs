using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;
using System.Reflection;
using KanoopCommon.Logging;

namespace Linux
{
	public class LinuxProcess
	{
		public enum LinuxProcessStates
		{
			[TextToEnum("D")]
			UninterruptibleSleep,
			[TextToEnum("R")]
			Running,
			[TextToEnum("S")]
			InterruptibleSleep,
			[TextToEnum("T")]
			Stopped,
			[TextToEnum("W")]
			Paging,
			[TextToEnum("X")]
			Dead,
			[TextToEnum("Z")]
			Zombie,
			[TextToEnum("<")]
			HighPriority,
			[TextToEnum("N")]
			LowPriority,
			[TextToEnum("L")]
			HasPagedLocked,
			[TextToEnum("s")]
			SessionLeader,
			[TextToEnum("l")]
			MultiThreaded,
			[TextToEnum("+")]
			Foreground
		}

		static Dictionary<Char, LinuxProcessStates> _processStateIndex;

		public String User { get; set; }

		public UInt32 Pid { get; set; }

		public Double Cpu { get; set; }

		public Double Memory { get; set; }
		
		public UInt32 VirtualSize { get; set; }

		public UInt32 ResidentSize { get; set; }

		public String Tty { get; set; }

		public List<LinuxProcessStates> ProcessStates { get; set; }

		public DateTime StartTime { get; set; }

		public TimeSpan ElapsedTime { get { return DateTime.Now - StartTime; } }

		public String Command { get; set; }

		public String Name { get; set; }

		LinuxProcess()
			: this(String.Empty, String.Empty, 0) {}

		public LinuxProcess(String name, String user, UInt32 pid)
			: this(name, user, pid, 0, 0, 0, 0, String.Empty, new List<LinuxProcessStates>(), DateTime.MinValue, String.Empty) {}

		public LinuxProcess(String name, 
							String user, 
							UInt32 pid, 
							Double cpu, 
							Double memory, 
							UInt32 virtualSize, 
							UInt32 residentSize, 
							String tty, 
							List<LinuxProcessStates> states, 
							DateTime startTime,
							String command)
		{
			Name = name;
			User = user;
			Pid = pid;
			Cpu = cpu;
			Memory = memory;
			VirtualSize = virtualSize;
			ResidentSize = residentSize;
			Tty = tty;
			ProcessStates = states;
			StartTime = startTime;
			Command = command;
		}

		static LinuxProcess()
		{
			_processStateIndex = new Dictionary<Char, LinuxProcessStates>();
			foreach(Enum value in Enum.GetValues(typeof(LinuxProcessStates)))
			{
				String name = value.ToString();
				FieldInfo field =  typeof(LinuxProcessStates).GetField(name);
				Object[] attribs;
				if((field != null) && (attribs = field.GetCustomAttributes(typeof(TextToEnumAttribute), false)).Length > 0)
				{
					_processStateIndex.Add(((TextToEnumAttribute)attribs[0]).Value[0], (LinuxProcessStates)value);
				}
			}
		}

		public static bool TryParse(String line, out LinuxProcess info)
		{
			info = new LinuxProcess();
			bool result = false;

			try
			{
				List<LinePart> parts = LineSplitter.Split(line);
				UInt32 test;
				if(parts.Count > 5 && UInt32.TryParse(parts[0].Text, out test) == true)
				{
					for(int x = 0;parts.Count >= 10 && x < parts.Count && result == false;x++)
					{
						switch(x)
						{
							case 0:
								info.Pid = UInt32.Parse(parts[x].Text);
								break;

							case 1:
								info.User = parts[x].Text;
								break;

							case 2:
								info.Cpu = Double.Parse(parts[x].Text);
								break;

							case 3:
								info.Memory = Double.Parse(parts[x].Text);
								break;

							case 4:
								info.VirtualSize = UInt32.Parse(parts[x].Text);
								break;

							case 5:
								info.ResidentSize = UInt32.Parse(parts[x].Text);
								break;

							case 6:
								info.Tty = parts[x].Text;
								break;

							case 7:
								foreach(Char c in parts[x].Text)
								{
									LinuxProcessStates state;
									if(_processStateIndex.TryGetValue(c, out state))
										info.ProcessStates.Add(state);
								}
								break;

							case 8:
								{
									TimeSpan elapsed;
									if(TryParseTimeSpan(parts[x].Text, out elapsed) == false)
										elapsed = TimeSpan.Zero;

									info.StartTime = DateTime.Now - elapsed;
								}
								break;

							case 9:
								info.Command = line.Substring(parts[x].Index).Trim();
								info.Name = info.Command.Split(' ')[0];
								result = true;
								break;
						}
					}
				}
			}				
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.DEBUG, "Parse error: {0}", e.Message);
			}


			return result;
		}

		static bool TryParseTimeSpan(String value, out TimeSpan ts)
		{
			bool success = true;

			ts = TimeSpan.Zero;
			int days = 0;
			int index = value.IndexOf('-');
			if(index > 0)
			{
			    days = int.Parse(value.Substring(0, index));
				value = value.Substring(index + 1);
			}
									
			String[] parts = value.Split(':');
			if(parts.Length == 3)
			{
				ts = new TimeSpan(days, Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]));
			}
			else if(parts.Length == 2)
			{
				ts = new TimeSpan(0, Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]));
			}
			else
			{
				success = false;
			}
			return success;
		}

		public override string ToString()
		{
			return String.Format("({0}) {1}", Pid, Name);
		}

	}

	public class LinuxProcessList : List<LinuxProcess>{}

}
