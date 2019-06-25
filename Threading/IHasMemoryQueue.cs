using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Threading
{
	public interface IHasMemoryQueue
	{
		int QueueMessageCount { get; }

		int QueueBlockDequeueTime { get; }
	}
}
