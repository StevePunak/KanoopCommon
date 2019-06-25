using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using KanoopCommon.Threading;
using KanoopCommon.Logging;

namespace KanoopCommon.Queueing
{
	public class MemoryQueue<QUEUETYPE> : IMemoryQueue
	{
		#region Constants

		const int DEFAULT_MAX_QUEUE_SIZE = 25000;

		#endregion

		#region Public Properties

		String _name;
		public String Name { get { return _name; } }

		int _blockingDequeueCount;
		public int BlockingDequeueCount { get { return _blockingDequeueCount; } }

		int _unblockingDequeueCount;
		public int UnblockingDequeueCount { get { return _unblockingDequeueCount; } }

		public int Count { get  { return _list.Count; } }

		int _maxQueueSize;
		public int MaxQueueSize { get { return _maxQueueSize; } set { _maxQueueSize = value; } }

		#endregion

		#region Private Memeber Variables

		List<QUEUETYPE> _list = new List<QUEUETYPE>();

		MutexEvent _itemEnqueuedEvent = new MutexEvent();

		MutexLock _listLock = new MutexLock();

		int _waiters;
		bool _intentionalUnblock;

		#endregion

		#region Constructor(s)

		public MemoryQueue()
			: this(typeof(QUEUETYPE).Name) {}

		public MemoryQueue(String name)
		{
			if (default(QUEUETYPE) != null)
			{
				throw new Exception("Queue Elements must have default value of null");
			}
			_name = name;

			_waiters = 0;

			_blockingDequeueCount = 0;
			_unblockingDequeueCount = 0;
			_maxQueueSize = DEFAULT_MAX_QUEUE_SIZE;
			_intentionalUnblock = false;

		}

		#endregion

		#region Public Access Methods

		public QUEUETYPE Dequeue()
		{
			QUEUETYPE ret = default(QUEUETYPE);

			try
			{
				_listLock.Lock();

				ret = PopHead();
			}
			finally
			{
				_listLock.Unlock();
			}

			return ret;
		}

		public QUEUETYPE BlockDequeue()
		{
			return BlockDequeue(0);
		}

		public QUEUETYPE BlockDequeue(TimeSpan ts)
		{
			return BlockDequeue((int)ts.TotalMilliseconds);
		}


		public void Enqueue(IEnumerable<QUEUETYPE> items)
		{
			foreach(QUEUETYPE item in items)
			{
				Enqueue(item);
			}
		}

		public void Enqueue(QUEUETYPE Item)
		{
			/**
			 * Lock the entire list and add the item.
			 * 
			 * This lock also ensures that m_nWaiters is not going to be changed until
			 * after we check its value.
			 */
			_listLock.Lock();
			_list.Add(Item);

			if (_waiters > 0)
			{
				/**
				 * Let the waiter pick up the item
				 */
				_listLock.Unlock();

				/**
				 * Set an event that will allow the waiter to continue. 
				 * 
				 * NOTE: There is a possibility that the consumer has timed out and he is just waiting 
				 * for the 'm_ListLock' to become available, when he will decrement m_nWaiters and leave 
				 * with a timeout. In this case, the next consumer will get the message.
				 */
				_itemEnqueuedEvent.Set();

			}
			else
			{
				/**
				 * There is no one waiting, we are done.
				 */
				_listLock.Unlock();
			}

			/** dump any messages over the max queue size */
			if(_list.Count > _maxQueueSize)
			{
				int messagesDumped = DumpMessages();
				LogText(LogLevel.ERROR, "{0} dumped {1} messages", this, messagesDumped);
			}
		}

		public QUEUETYPE BlockDequeue(int msecs)
		{
			QUEUETYPE ret = default(QUEUETYPE);

			/**
			 * We store the total milliseconds so that we can decrease it in the event that
			 * we need to loop
			 */
			int msecsRemaining = msecs;
			bool loop;
			do
			{
				/**
				 * By default, we will not loop
				 */
				loop = false;

				/**
				 * Obtain list lock. This gets us access to the value of 'waiter' count
				 * which is not allowed to change while we own this lock.
				 */
				_listLock.Lock();
			
				/**
				 * See if we can get an item immediately. If so, we are just going to return it.
				 */
				if(_list.Count > 0)
				{
					ret = PopHead();

					/** save statistic */
					_unblockingDequeueCount++;

					/** Release the global lock on the list. */
					_listLock.Unlock();
				}
				else
				{
					/** save statistic */
					_blockingDequeueCount++;

					/** 
					 * Since there are no items in the list, we'll have to wait for a producer.
					 */
					_waiters++;

					/**
					 * Clear this mutex. The producer will set it when he Enqueues something.
					 * NOTE that there can not be a producer in the Enqueue logic at this time
					 * due to the fact that we own the m_ListLock.
					 */
					_itemEnqueuedEvent.Clear();

					/**
					 * Release the queue lock. This will allow a producer into the enqueue logic
					 * where he can put an item on the queue.
					 */
					_listLock.Unlock();

					/**
					 * Wait for a producer for the given amount of time. We save the time we started
					 * our wait so that we can decrement the time left in the event that
					 * we need to loop.
					 */
					int startWait = Environment.TickCount;

					/** blocking wait for an item to be enqueued */
					bool result = _itemEnqueuedEvent.Wait(msecsRemaining);
			
					if (!result)
					{
						/** 
						 * No producer put anything on the queue before our event timed out.
						 * 
						 * NOTE that there MAY be something on the queue if our event timed out just
						 * as the producer added something. Therefore List.Count may be greater than zero.
						 * We do not care about this, and allow the timeout to proceed.
						 * 
						 * We will return NULL in this case.
						 */

						/** 
						 * Obtain the queue lock and decrement the waiter count.
						 */
						_listLock.Lock();
						_waiters--;
						_listLock.Unlock();
					}
					else
					{
						/**
						 * A producer has put something on the queue before our timeout value expired.
						 * We will obtain the lock for our queue and take the 'head' item to return. 
						 * 
						 * No producers may pass while we have the lock.
						 * 
						 * Note that in the meantime, the queue has been unlocked, and that many 
						 * producers may have put something on the queue, and other consumers may have removed
						 * those items. Thus, there is a chance that nothing is left even though the event is set.
						 * In this case, we will go back and wait some more.
						 */
						_listLock.Lock();

						if (_list.Count > 0)
						{
							ret = PopHead();
						}
						else
						{
							/**
							 * The event is set, but nothing is left on the queue.
							 * This can happen while the main list lock was unlocked (previous logic block)
							 * during the time we were waiting on the 'enqueued' event.
							 */
							if(msecs != 0)
							{
								/** 
								 * this is a timed wait... decrement by the amount of time we waited, 
								 * then go wait some more
								 */
								msecsRemaining -= Environment.TickCount - startWait;
								if(msecsRemaining > 0)
								{
									if(_intentionalUnblock)
									{
										loop = false;		/** don't loop if this was done intentionally */
										_intentionalUnblock = false;
									}
									else
									{
										loop = true;
									}
								}
							}
							else
							{
								/** This is a 'wait forever' dequeue. Unconditionally go back and wait some more */
								loop = true;
							}
// 							Log.SysLogText(LogLevel.ERROR, "{0} {1} Popped a NULL?", Name, Thread.CurrentThread.Name);
						}

						/**
						 * Decrement the waiter count, and release the queue lock
						 */
						_waiters--;
						_listLock.Unlock();

					}
				}
			}while(loop);

			return ret;
		}

		public void Clear()
		{
			_listLock.Lock();

			_list.Clear();

			_listLock.Unlock();
		}

		public void Unblock()
		{
			_intentionalUnblock = true;
			_itemEnqueuedEvent.Set();
		}

		public List<QUEUETYPE> GetAllItems()
		{
			List<QUEUETYPE> value = new List<QUEUETYPE>();
			try
			{
				_listLock.Lock();

				value = new List<QUEUETYPE>(_list);
				_list.Clear();
			}
			finally
			{
				_listLock.Unlock();
			}

			return value;
		}

		#endregion

		#region Utility

		int DumpMessages()
		{
			int messagesRemoved = 0;
			try
			{
				_listLock.Lock();

				if(_list.Count > _maxQueueSize)
				{
					LogText(LogLevel.ERROR, "{0} first message dumped is {1}", this,  _list[_maxQueueSize]);
					messagesRemoved = _list.Count - _maxQueueSize;
					_list.RemoveRange(_maxQueueSize, messagesRemoved);
				}
			}
			finally
			{
				_listLock.Unlock();
			}

			return messagesRemoved;
		}

		QUEUETYPE PopHead()
		{
			QUEUETYPE ret = default(QUEUETYPE);

			if (_list.Count > 0)
			{
				ret = _list[0];
				_list.RemoveAt(0); 
			}

			return ret;
		}

		void LogText(LogLevel level, String format, params object[] parms)
		{
			Log.SysLogText(level, format, parms);
		}

		public override string ToString()
		{
			return String.Format("Queue: {0}", Name);
		}

		#endregion

	}
}





