using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KanoopCommon.Queueing;

namespace KanoopCommon.Threading
{
	public class ThreadMessageQueue : MemoryQueue<ThreadMessage> 
	{
		public ThreadMessageQueue(String name)
			: base(name) {}

		public override string ToString()
		{
			return String.Format("ThreadQ: {0} ({1} messages)", Name, Count);
		}
	}

	public enum ThreadMessageType
	{
		Unknown = 0,
		HAPUService,
		Abort = Int32.MaxValue
	}

	public class ThreadMessage
	{
		ThreadMessageType	m_MessageType;
		public ThreadMessageType Type
		{
			get { return m_MessageType; }
			set { m_MessageType = value; }
		}

		public ThreadMessage(ThreadMessageType type)
		{
			m_MessageType = type;
		}

		public ThreadMessage(Enum type)
		{
			m_MessageType = (ThreadMessageType)type;
		}
	}
}
