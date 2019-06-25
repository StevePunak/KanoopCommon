using System;
using System.Collections.Generic;
using System.Text;

using KanoopCommon.Threading;
using KanoopCommon.CommonObjects;
using KanoopCommon.Conversions;
using KanoopCommon.Logging;

namespace KanoopCommon.Performance
{
	public class ObjectCaching
	{
		static ObjectCaching()
		{
			_enabled = false;
			_cachedItemDuration = new TimeSpan(0, 1, 0);
			_cleanupInterval = TimeSpan.FromMinutes(2);
		}

		static bool			_enabled;
		public static bool Enabled
		{
			get { return _enabled;	}
			set { _enabled = value;	}
		}

		static TimeSpan _cachedItemDuration;
		public static TimeSpan CachedItemDuration
		{
			get { return _cachedItemDuration; }
			set { _cachedItemDuration = value; }
		}

		static bool _trackYoungestEntry;
		public static bool TrackYoungestEntry
		{
			get { return _trackYoungestEntry; }
			set { _trackYoungestEntry  = value; }
		}

		static TimeSpan _cleanupInterval;
		public static TimeSpan CleanupInterval
		{
			get { return _cleanupInterval; }
			set { _cleanupInterval = value; }
		}
	}

	public class ObjectCache<_KEY_TYPE, _OBJECT_TYPE>
	{
		class CacheEntry<_ENTRY_OBJECT_TYPE>
		{
			DateTime		_entryTime;
			public DateTime EntryTime 
			{ 
				get { return _entryTime; }
				set { _entryTime = value; } 
			}

			_ENTRY_OBJECT_TYPE	_cachedObject;
			public _ENTRY_OBJECT_TYPE CachedObject 
			{
				get { return _cachedObject; } 
				set { _cachedObject = value; } 
			}

			public CacheEntry(_ENTRY_OBJECT_TYPE o)
			{
				_cachedObject = o;
				_entryTime = DateTime.UtcNow;
			}
		}

		/**
		 * this is a dictionary of dictionaries
		 */
		class TypeCache : Dictionary<_KEY_TYPE, CacheEntry<_OBJECT_TYPE>>{}

		TypeCache			_cache;

		MutexLock			_lock;

		DateTime			_lastCleanupTime;

		CacheEntry<_OBJECT_TYPE> _youngestEntry;
		public DateTime YoungestEntry
		{
			get
			{
				DateTime retVal;
				if (ObjectCaching.TrackYoungestEntry && _youngestEntry != null)
				{
					retVal = _youngestEntry.EntryTime;
				}
				else
				{
					retVal = DateTime.MinValue;
				}
				return retVal;
			}
		}

		public ObjectCache()
		{
			_lastCleanupTime = DateTime.UtcNow;
			_cache = new ObjectCache<_KEY_TYPE, _OBJECT_TYPE>.TypeCache();
			_lock = new MutexLock();
		}

		public _OBJECT_TYPE this[_KEY_TYPE key]
		{
			get { return _cache[key].CachedObject; }
		}

		public int Count
		{
			get
			{
				_lock.Lock();

				int ret = _cache.Count;

				_lock.Unlock();

				return ret;
			}
		}

		public List<_OBJECT_TYPE> List
		{
			get
			{
				_lock.Lock();

				List<_OBJECT_TYPE> ret = new List<_OBJECT_TYPE>();
				foreach (KeyValuePair<_KEY_TYPE, CacheEntry<_OBJECT_TYPE>> kvp in _cache)
				{
					ret.Add(kvp.Value.CachedObject);
				}

				_lock.Unlock();

				return ret;
			}
		}

		#region Public Access Methods

		public virtual void Update(_KEY_TYPE key, _OBJECT_TYPE updated)
		{
			if(ObjectCaching.Enabled)
			{
				try
				{
				_lock.Lock();

				if(!_cache.ContainsKey(key))
				{
					_cache.Add(key, new CacheEntry<_OBJECT_TYPE>(updated));
				}
				else
				{
					if (updated is ICopyable<_OBJECT_TYPE>)
					{
						((ICopyable<_OBJECT_TYPE>)_cache[key].CachedObject).CopyFrom(updated);
						_cache[key].EntryTime = DateTime.UtcNow;
					}
					else
					{
						_cache[key] = new CacheEntry<_OBJECT_TYPE>(updated);
					}
				}

				if (ObjectCaching.TrackYoungestEntry)
				{
					_youngestEntry = _cache[key];
				}
				}
				finally
				{
					_lock.Unlock();
				}
			}
		}

		private void DeleteNoLock(_KEY_TYPE key)
		{
			/** NOTE: expects list locked on entry! */
			bool result = _cache.Remove(key);

			if (result && ObjectCaching.TrackYoungestEntry)
			{
				// Set Next Youngest Entry
				DateTime dteYoung = DateTime.MinValue;
				foreach (KeyValuePair<_KEY_TYPE, CacheEntry<_OBJECT_TYPE>> kvp in _cache)
				{
					if (kvp.Value.EntryTime > dteYoung)
					{
						_youngestEntry = kvp.Value;
					}
				}
			}
		}

		public virtual void Delete(_KEY_TYPE key)
		{
			_lock.Lock();

			bool bolVal = _cache.Remove(key);

			if (bolVal && ObjectCaching.TrackYoungestEntry)
			{
				// Set Next Youngest Entry
				DateTime dteYoung = DateTime.MinValue;
				foreach (KeyValuePair<_KEY_TYPE, CacheEntry<_OBJECT_TYPE>> kvp in _cache)
				{
					if (kvp.Value.EntryTime > dteYoung)
					{
						_youngestEntry = kvp.Value;
					}
				}
			}

			_lock.Unlock();
		}

		public virtual void Clear()
		{
			_lock.Lock();

			_cache.Clear();
			_youngestEntry = null;

			_lock.Unlock();
		}

		public bool ContainsKey(_KEY_TYPE key)
		{
			bool result = false;
			if (ObjectCaching.Enabled)
			{
				result = _cache.ContainsKey(key);
			}
			return result;
		}

		public bool IsExpired(_KEY_TYPE key)
		{
			bool result = true;
			if (ObjectCaching.Enabled)
			{
				CacheEntry<_OBJECT_TYPE> entry;
				if (_cache.TryGetValue(key, out entry))
				{
					result = (entry.EntryTime <= (DateTime.UtcNow - ObjectCaching.CachedItemDuration));
				}
			}
			return result;
		}

		public bool TryGetValue(_KEY_TYPE key, out _OBJECT_TYPE val)
		{
			bool result = false;
			val = default(_OBJECT_TYPE);

			if(ObjectCaching.Enabled)
			{
				try
				{
					_lock.Lock();

				CacheEntry<_OBJECT_TYPE> entry;
				if(_cache.TryGetValue(key, out entry) == true)
				{
					if(entry == null)
					{
						Log.SysLogText(LogLevel.WARNING, "Removing null entry from cache, the key was {0}", key);
							DeleteNoLock(key);
					}
					else if(entry.EntryTime > DateTime.UtcNow - ObjectCaching.CachedItemDuration)
					{
						val = entry.CachedObject;
						result = true;
					}
					else
					{
							DeleteNoLock(key);
					}
				}

				/** maybe do cleanup */
				if(DateTime.UtcNow > _lastCleanupTime + ObjectCaching.CleanupInterval)
				{
					Cleanup();
					}
				}
				finally
				{
					_lock.Unlock();
				}
			}

			return result;
		}

		#endregion

		#region Private Methods

		private void Cleanup()
		{
			/** NOTE: Expects lock held on entry */

			List<_KEY_TYPE> deleteList = new List<_KEY_TYPE>();
			DateTime now = DateTime.UtcNow;

			foreach(KeyValuePair<_KEY_TYPE, CacheEntry<_OBJECT_TYPE>> kvp in _cache)
			{
				if(now > ((CacheEntry<_OBJECT_TYPE>)kvp.Value).EntryTime + ObjectCaching.CachedItemDuration)
				{
					deleteList.Add(kvp.Key);
				}
			}

			foreach(_KEY_TYPE key in deleteList)
			{
				_cache.Remove(key);
			}

			_lastCleanupTime = DateTime.UtcNow;
		}

		#endregion
	}

}
