using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using KanoopCommon.Threading;
using KanoopCommon.Logging;
using System.Globalization;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Database
{
	public class DataSourceFactory
	{
		#region Constants

		const String COL_KEY = "name";
		const String COL_VALUE = "val";
		const String KEY_SCHEMA_VER = "schema_version";
		const String KEY_DB_TYPE = "db_type";

		#endregion

		#region Internal Classes

		public class TypeLookup
		{
			public class VersionLookup : Dictionary<SoftwareVersion, Type> { }
			
			Dictionary<Type, VersionLookup> _TypeLookup = new Dictionary<Type, VersionLookup>();

			MutexLock _typeLock = new MutexLock();

			public Type Get<T>(SoftwareVersion version)
			{
				return GetByType(typeof(T),version);
			}

			public Type GetByType(Type type, SoftwareVersion version)
			{
				Type returnType = null;
//				Type type = typeof(T);
				VersionLookup versionLookup;
				try
				{
					if (!_TypeLookup.TryGetValue(type, out versionLookup))
					{
						_typeLock.Lock();

						versionLookup = new VersionLookup();
						_TypeLookup.Add(type, versionLookup);

						List<Type> types = new List<Type>(type.Assembly.GetTypes().Where(asmType => asmType.IsClass && !asmType.IsAbstract && (asmType.IsSubclassOf(type) || asmType == type)));

						// Find all descendants
						// TODO: Add functionality to add additional assemblies where datasources may be found, 
						//      outside of the assembly containing the base, which is where all searches are currently conducted
						foreach (Type subType in types)
						{
							DatabaseVersionAttribute[] attribs = (DatabaseVersionAttribute[])subType.GetCustomAttributes(typeof(DatabaseVersionAttribute), false);
							if (attribs.Length > 0)
							{
								foreach (DatabaseVersionAttribute attr in attribs)
								{
									foreach(SoftwareVersion attributeVersion in attr.Versions)
									{
										if (versionLookup.ContainsKey(attributeVersion))
											versionLookup[attributeVersion] = subType;
										else
											versionLookup.Add(attributeVersion, subType);
									}
								}
							}
						}
					}
				}
				finally
				{
					_typeLock.Unlock();
				}

				// At this point, all versioned types realted to the requested type are indexed by version number in the lookup for this type
				if(!versionLookup.TryGetValue(version, out returnType))
				{
					// Couldn't find an exact match of the datasource requested. Look for the most recent one prior to this database version
					SoftwareVersion lastVersion = null;

					List<SoftwareVersion> keys = new List<SoftwareVersion>(versionLookup.Keys);
					keys.Sort();
					keys.Reverse();

					foreach(SoftwareVersion key in keys)
					{
						if (key > version)
							continue;

						lastVersion = key;
						break;
					}

					if(lastVersion != null)
					{
						returnType = versionLookup[lastVersion];
						versionLookup.Add(version, returnType);

						// Warn if our datasource is not marked for the current version?
					}
				}

				return returnType;

			}

		}

		public class InstanceLookup 
		{
			class CredentialLookup : Dictionary<String, Object> {}
			Dictionary<Type, CredentialLookup> _credentialLookup = new Dictionary<Type, CredentialLookup>();


			public Object GetByType(Type t,SqlDBCredentials credentials, SoftwareVersion version = null)
			{
				Object retVal= null;
				CredentialLookup lkp;
				if (_credentialLookup.TryGetValue(t, out lkp))
				{
					if (version == null)
					{
						version = new SoftwareVersion();
					}

					String key = credentials.ToParsableString() + ";DBVER=" + version.ToString();
					if (lkp.ContainsKey(key))
					{
						retVal = lkp[key];
					}
				}
				return retVal;

			}
			public Object SetByType(Type t, SqlDBCredentials credentials, SoftwareVersion version, Object obj)
			{
				CredentialLookup lkp;
				if (!_credentialLookup.TryGetValue(t, out lkp))
				{
					lkp = new CredentialLookup();
					_credentialLookup.Add(t,lkp);
				}
				String key = credentials.ToParsableString() + ";DBVER=" + version.ToString();
				if (lkp.ContainsKey(key))
					lkp[key] = obj;
				else
					lkp.Add(key,obj);
				return obj;
			}



			public T Get<T>(SqlDBCredentials credentials, SoftwareVersion version = null)
			{
				T retVal= default(T);
				CredentialLookup lkp;
				if (_credentialLookup.TryGetValue(typeof(T), out lkp))
				{
					if(version == null)
					{
						version = new SoftwareVersion();
					}
					
					String key = credentials.ToParsableString() + ";DBVER="+version.ToString();
					if (lkp.ContainsKey(key))
					{
						retVal= (T)lkp[key];
					}
				}
				return retVal;
			
			}
			
			public T Set<T>(SqlDBCredentials credentials, SoftwareVersion version, T obj)
			{
				CredentialLookup lkp;
				if (!_credentialLookup.TryGetValue(typeof(T), out lkp))
				{
					lkp = new CredentialLookup();
					_credentialLookup.Add(typeof(T),lkp);
				}
				String key = credentials.ToParsableString() + ";DBVER=" + version.ToString();
				if (lkp.ContainsKey(key))
					lkp[key] = obj;
				else
					lkp.Add(key,obj);
				return obj;
			}
		}

		#endregion

		#region Static Member Variables

		static InstanceLookup _InstanceLookup = new InstanceLookup();
		
		static TypeLookup _TypeLookup = new TypeLookup();

		static MutexLock _databaseVersionLock = new MutexLock();
		
		static MutexLock _instanceLock = new MutexLock();

		static Dictionary<String, SoftwareVersion> _databaseVersionLookup = new Dictionary<String, SoftwareVersion>();
		
		#endregion

		#region Factory Creation

		public static bool TryCreate<T>(SqlDBCredentials credentials, out T outVal, bool checkVersion = true, bool checkDeclarations = false, SoftwareVersion version = null, Type defaultDSType = null, bool readOnly = false)
		{
			bool retVal = false;
			outVal = default(T);
			try
			{
				outVal = DataSourceFactory.Create<T>(credentials, checkVersion, checkDeclarations, version, defaultDSType);
				retVal = true;
			}
			catch             
			{
			}
			return retVal;
		}


		public static T Create<T>(SqlDBCredentials credentials, bool checkVersion = false, bool checkDeclarations = false, SoftwareVersion version = null, Type defaultDSType = null, bool readOnly = false) 
		{
			/** make a copy of these */
			credentials = credentials.Clone();
			InternalDatabaseType databaseType = InternalDatabaseType.Unknown;

			/** ensure version is valid */
			if(version == null)
			{
				version = new SoftwareVersion();
			}

			T retVal = default(T);
			try
			{
				_instanceLock.Lock();

				SoftwareVersion initialVersion = version;
				_InstanceLookup.Get<T>(credentials, version);

				// No cached instance of the proper datasource for these credentials about. Try and find one.
				if (retVal == null)
				{
					if(checkVersion == false)
					{
						version = new SoftwareVersion("9.9");
						databaseType = InternalDatabaseType.Unknown;
					}
					else
					{
						// Now we get the database version from the lookup or database
						if (version == SoftwareVersion.Empty && GetDatabaseVersion(credentials, out version, out databaseType).ResultCode != DBResult.Result.Success)
						{
							throw new DataSourceException("DataSource Factory unable to obtain version from Database: {0} (1)", credentials.Host);
						}
					}

					//1. Get specified version of T. Routine will Start at T and look at descendants, caching results. 
					Type type = _TypeLookup.Get<T>(version);

					//1a. Throw exception if T has no version decorations
					if (type == null)
					{
						if (defaultDSType == null)
						{
							 throw new DataSourceException("Unable to find  version {0} of DataSource {1}", version, typeof(T));
						}
						else
						{
							type = defaultDSType;
						}
					}

					bool saveDeclarationCheck = false;
					/** save global declaration check flag */
					if (type.IsSubclassOf(typeof(DataSourceBase)))
					{
						 saveDeclarationCheck = DataSourceBase.CheckDeclarations;
						DataSourceBase.CheckDeclarations = checkDeclarations;
					}

					/** Create instance with proper parameters */
					retVal = (T)Activator.CreateInstance(type, credentials);
					_InstanceLookup.Set<T>(credentials, version, retVal);
					if (initialVersion != version)
					{
						_InstanceLookup.Set<T>(credentials, initialVersion, retVal);
					}
					
					/** restore global declaration check flag */
					if (type.IsSubclassOf(typeof(DataSourceBase)))
					{
						DataSourceBase.CheckDeclarations = saveDeclarationCheck;
					}

					if(retVal is DataSourceBase)
					{
						((DataSourceBase)(Object)retVal).ReadOnly = readOnly;
						((DataSourceBase)(Object)retVal).Version = version;
						((DataSourceBase)(Object)retVal).DatabaseType = databaseType;
					}
				}
			}
			catch(Exception e)
			{
				Log.SysLogText(LogLevel.ERROR, "DataSourceFactory Exception: {0}", e.Message);
				if(e.InnerException != null)
				{
					Log.SysLogText(LogLevel.ERROR, "   Inner Exception: {0}", e.InnerException.Message);
				}
				/** rethrow */
				throw e;
			}
			finally
			{
				_instanceLock.Unlock();
			}
			return retVal;
		}

		public static bool TryCreateDynamic(String assemblyName,String className,SqlDBCredentials credentials, out Object outVal, bool checkDeclarations = false, SoftwareVersion version = null, Type defaultDSType = null, bool readOnly = false)
		{
			bool retVal = false;
			outVal = null;
			try
			{
				outVal = DataSourceFactory.CreateDynamic(assemblyName,className, credentials, checkDeclarations, version, defaultDSType);
				retVal = true;
			}
			catch
			{
			}
			return retVal;
		}

		public static Object CreateDynamic(String assemblyName, String className, SqlDBCredentials credentials, bool checkDeclarations = false, SoftwareVersion version = null, Type defaultDSType = null, bool readOnly = false)
		{
			/** make a copy of these */
			credentials = credentials.Clone();

			/** ensure version is valid */
			if (version == null)
			{
				version = new SoftwareVersion();
			}

			InternalDatabaseType databaseType = InternalDatabaseType.Unknown;


			Object retVal = null;
			try
			{
				_instanceLock.Lock();
				SoftwareVersion initialVersion = version;
				Assembly asm = null;
				Type t = null;
				try
				{
					asm = Assembly.LoadFrom(assemblyName);
					t = asm.GetType(className);
				}
				catch (Exception e)
				{
					throw new DataSourceException("DataSource Factory unable to load Datasource from Assembly: {0} - {1}", assemblyName, e);
				}

				_InstanceLookup.GetByType(t, credentials, version);

				// No cached instance of the proper datasource for these credentials about. Try and find one.
				if (retVal == null)
				{
					// Now we get the database version from the lookup or database
					if (version == SoftwareVersion.Empty && !(GetDatabaseVersion(credentials, out version, out databaseType).ResultCode == DBResult.Result.Success))
					{
						throw new DataSourceException("DataSource Factory unable to obtain version from Database: {0} (2)", credentials.Host);
					}


					//1. Get specified version of T. Routine will Start at T and look at descendants, caching results. 
					Type type = _TypeLookup.GetByType(t, version);

					//1a. Throw exception if T has no version decorations
					if (type == null)
					{
						if (defaultDSType == null)
						{
							throw new DataSourceException("Unable to find  version {0} of DataSource {1}", version, t);
						}
						else
						{
							type = defaultDSType;
						}
					}

					bool saveDeclarationCheck = false;
					/** save global declaration check flag */
					if (type.IsSubclassOf(typeof(DataSourceBase)))
					{
						saveDeclarationCheck = DataSourceBase.CheckDeclarations;
						DataSourceBase.CheckDeclarations = checkDeclarations;
					}

					/** Create instance with proper parameters */
					retVal = Activator.CreateInstance(type, credentials);
					_InstanceLookup.SetByType(t, credentials, version, retVal);
					if (initialVersion != version)
					{
						_InstanceLookup.SetByType(t, credentials, initialVersion, retVal);
					}

					/** restore global declaration check flag */
					if (type.IsSubclassOf(typeof(DataSourceBase)))
					{
						DataSourceBase.CheckDeclarations = saveDeclarationCheck;
					}

					if (retVal is DataSourceBase)
					{
						((DataSourceBase)(Object)retVal).ReadOnly = readOnly;
						((DataSourceBase)(Object)retVal).DatabaseType = databaseType;
					}
				}
			}
			catch (Exception e)
			{
				Log.SysLogText(LogLevel.ERROR, "DataSourceFactory Exception: {0}", e.Message);
				if (e.InnerException != null)
				{
					Log.SysLogText(LogLevel.ERROR, "   Inner Exception: {0}", e.InnerException.Message);
				}

				/** rethrow */
				throw e;
			}
			finally
			{
				_instanceLock.Unlock();
			}
			return retVal;


		}


		#endregion

		#region Utlity

		public static DBResult GetDatabaseVersion(SqlDBCredentials credentials, out SoftwareVersion version, out InternalDatabaseType databaseType)
		{
			DBResult retVal = new DBResult(DBResult.Result.Success);
			try
			{
				_databaseVersionLock.Lock();
				version = null;
				databaseType = InternalDatabaseType.Unknown;
				String indexKey = credentials.ToParsableString();
				if (!_databaseVersionLookup.TryGetValue(indexKey, out version))
				{
					ISqlDataSource ds = SqlConnectionPool.Instance(credentials).GetDataSource();
					DatabaseDataReader reader;

					QueryString sql = ds.FormatTrusted("SELECT * FROM I.seq where name in ('{0}', '{1}')", KEY_DB_TYPE, KEY_SCHEMA_VER);
					retVal = ds.Query(sql, out reader);
					if (retVal.ResultCode == DBResult.Result.Success && reader.HasRows)
					{
						while(reader.Read())
						{
							String key = DBUtil.GetString(reader[COL_KEY]);
							if(key == KEY_SCHEMA_VER)
							{
								version = DBUtil.GetSoftwareVersion(reader[COL_VALUE]);
							}
							else if(key == KEY_DB_TYPE)
							{
								databaseType = (InternalDatabaseType)DBUtil.GetInt32(reader[COL_VALUE]);
							}
						}
						_databaseVersionLookup.Add(indexKey, version);
					}
				}
			}
			finally
			{
				_databaseVersionLock.Unlock();
			}
			return retVal;
		}

		public static Type GetDataSourceType<T>(SoftwareVersion version)
		{
			return _TypeLookup.Get<T>(version);
		}

		public static void FlushCache()
		{
			try
			{
				_databaseVersionLock.Lock();

				_databaseVersionLookup.Clear();
			}
			finally
			{
				_databaseVersionLock.Unlock();
			}
		}

		#endregion
	}
}
