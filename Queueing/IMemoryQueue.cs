using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Queueing
{
	interface IMemoryQueue
	{
		String Name { get; }

		int Count { get; }

		int BlockingDequeueCount { get; }

		int UnblockingDequeueCount { get; }
	}
}
