using System;
using System.Runtime.Caching;
using System.Collections.Generic;
using KanoopCommon.Logging;
using System.Collections;

namespace KanoopCommon.Database
{
	public abstract class CachedDataSource : DataSourceBase
	{
		private readonly ObjectCache _cacheProvider;
		private readonly string _regionName;

		protected CachedDataSource(SqlDBCredentials credentials, ObjectCache cacheProvider, string regionName)
			: base(credentials)
		{
			if (cacheProvider == null)
			{
				throw new ArgumentNullException("cacheProvider");
			}

			if (cacheProvider is MemoryCache)
			{
				regionName = null;
			}

			_cacheProvider = cacheProvider;
			_regionName = regionName;
		}

		protected T Get<T>(string cacheKey, CacheItemPolicy cachePolicy, Func<T> getDataFromDataSource, Func<T, bool> isDataValid)
			where T : class
		{
			var data = _cacheProvider.Get(cacheKey, _regionName) as T;

			if (data == null)
			{
				data = getDataFromDataSource();

				if (isDataValid(data))
				{
					var cacheItem = new CacheItem(cacheKey, data, _regionName);
					_cacheProvider.Add(cacheItem, cachePolicy);
				}
			}

			return data;
		}

        protected T GetObject<T>(string cacheKey, CacheItemPolicy cachePolicy, Func<T> getDataFromDataSource, Func<T, bool> isDataValid)
        {
            object data = _cacheProvider.Get(cacheKey, _regionName);

            if (data == null)
            {
                data = getDataFromDataSource();

                if (isDataValid((T)data))
                {
                    var cacheItem = new CacheItem(cacheKey, data, _regionName);
                    _cacheProvider.Add(cacheItem, cachePolicy);
                }

                return (T)data;
            }

            return (T)data;
        }

		protected void Remove(string cacheKey)
		{
			_cacheProvider.Remove(cacheKey, _regionName);
		}

		protected IEnumerable<T> QueryMySql<T>(string sql, params object[] args) 
			where T : ILoadable, new()
		{
            return DB.QueryList<T>(sql, args);
		}

		protected string BuildCacheKey(string queryName, params string[] varyByParameters)
		{
			return queryName + "~" + String.Join("~", varyByParameters);
		}
	}
}
