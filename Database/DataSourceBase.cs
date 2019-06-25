using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Database;
using KanoopCommon.Database.TableNames;
using System.Reflection;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Database
{
	public class DataSourceBase
	{
		#region Internal Classes

		class DataSourceCachedInfo
		{
			public List<SoftwareVersion> Versions { get; private set; }

			public ITableNames TableNames { get; private set; }

			public DataSourceCachedInfo(List<SoftwareVersion> versions, ITableNames tableNames)
			{
				Versions = versions;
				TableNames = tableNames;
			}
		}

		#endregion

		#region Public Properties

		public SqlDBCredentials Credentials { get; private set; }
		
		ISqlDataSource m_DB;
		public ISqlDataSource DB { get { return m_DB; } }

		public List<SoftwareVersion> Versions { get; private set; }
		
		public ITableNames Tables { get; set; }

		public bool ReadOnly { get { return m_DB.ReadOnly; } set{ m_DB.ReadOnly = value; } }

		public bool TrustedSql { get { return m_DB.TrustedSql; } set { m_DB.TrustedSql = value; } }

		public SoftwareVersion Version { get; internal set; }

		InternalDatabaseType _internalDatabaseType;
		public InternalDatabaseType DatabaseType
		{
			get
			{
				if(_internalDatabaseType == InternalDatabaseType.Unknown)
				{
					_internalDatabaseType = InternalDatabaseType.ManagedNetwork;
				
					/* this will be replaced with a read from SEQ at startup in next version */
					String host = Credentials.Host.ToLower().Trim();

					int index = host.IndexOf("dsal");
					if(index >= 0 && Char.IsDigit(host[index - 1]))
					{
						_internalDatabaseType = InternalDatabaseType.DataStoreArchive;
					}
					else if((index = host.IndexOf("dsl")) > 0  && Char.IsDigit(host[index - 1]))
					{
						_internalDatabaseType = InternalDatabaseType.DataStore;
					}

					else if((index = host.IndexOf("qdb")) >= 0)
					{
						_internalDatabaseType = InternalDatabaseType.QueueData;
					}
				}
				return _internalDatabaseType;
			}

			internal set { _internalDatabaseType = value; }
		}


		public static bool CheckDeclarations { get; set; }

		#endregion

		#region Private Members

		static List<Type> m_VerifiedTypes;

		static Dictionary<Type, DataSourceCachedInfo>		m_TypeToCachedInfoIndex;
		static Dictionary<SoftwareVersion, ITableNames>		m_VersionToTablesIndex;

		#endregion

		#region Constructor(s)

		static DataSourceBase()
		{
			m_VerifiedTypes = new List<Type>();
			m_TypeToCachedInfoIndex = new Dictionary<Type, DataSourceCachedInfo>();

			LoadVersionToTablesIndex();

			CheckDeclarations = true;
		}

		protected DataSourceBase(SqlDBCredentials sqlCredentials)
		{
			_internalDatabaseType = InternalDatabaseType.Unknown;
			Credentials = sqlCredentials;
			m_DB = SqlConnectionPool.Instance(sqlCredentials).GetDataSource();

			if(m_DB == null)
			{
				throw new DataSourceException("Could not instantiate {0} with the following credentials: {1}", GetType(), sqlCredentials);
			}

			if(CheckDeclarations)
			{
				ValidateMethods(GetType());
			}

			LoadCachedTypeInformation();
		}

		#endregion

		#region Object Validation

		static void ValidateMethods(Type thisType)
		{
			List<String> errors = new List<String>();

			if(m_VerifiedTypes.Contains(thisType) == false)
			{
				/** ensure that each public and protected method return DBResult is mared 'virtual' */
				foreach(MethodInfo method in thisType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					if(	method.ReturnType == typeof(DBResult) && 
						method.IsVirtual == false)
					{
						errors.Add(method.Name);
					}
				}
			}
			
			if(errors.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("The following methods must be marked virtual:\n");
				foreach(String error in errors)
				{
					sb.AppendFormat("    {0}\n", error);
				}

				throw new DataSourceException("EXCEPTION in type validation for {0}. {1}", thisType, sb);
			}
			else
			{
				m_VerifiedTypes.Add(thisType);
			}
		}

		#endregion

		#region Public Utility Methods

		public virtual DBResult TestConnection()
		{
			QueryString strSQL = DB.FormatTrusted("SELECT COUNT(*) FROM I.seq");
			DatabaseDataReader resultSet;
			DBResult result = DB.Query(strSQL, out resultSet);
			using(resultSet)
			{
				/** intentionally empty */
			}
			return result;
		}

		public DBResult SetSchemaForTable(String schema, String table)
		{
			return Tables.ChangeSchemaForTableName(schema, table) == true
				? new DBResult(DBResult.Result.Success)
				: new DBResult(DBResult.Result.UpdateFailure);
		}

		public DBResult AllowZeroValueAutoincrementField()
		{
			QueryString sql = DB.Format("SET SESSION sql_mode = 'NO_AUTO_VALUE_ON_ZERO'");
			return m_DB.Update(sql);
		}

		public DBResult SetSessionModeNoAutoValueOnZero()
		{
			QueryString sql2 = m_DB.Format("SET SESSION sql_mode='NO_AUTO_VALUE_ON_ZERO';");

			DatabaseDataReader reader;
			DBResult result = m_DB.Query(sql2, out reader);
			using (reader)
			{
				// Intentionally empty
			}

			return result; 
		}

		public DBResult ClearSessionMode()
		{
			QueryString sql2 = m_DB.Format("SET SESSION sql_mode='';");

			DatabaseDataReader reader;
			DBResult result = m_DB.Query(sql2, out reader);
			using (reader)
			{
				// Intentionally empty
			}

			return result;
		}

		public override string ToString()
		{
			return String.Format("{0} on {1}", GetType().Name, Credentials.Host);
		}

		#endregion

		#region Private Utility Methods

		static void LoadVersionToTablesIndex()
		{
			m_VersionToTablesIndex = new Dictionary<SoftwareVersion,ITableNames>();

			List<Type> types = new List<Type>(Assembly.GetCallingAssembly().GetTypes());

			/** 
			 * roll through this assembly (Common) and load and index 
			 * implementers of itablenames with a version attribute 
			 */
			foreach (Type type in types)
			{
				if(typeof(ITableNames).IsAssignableFrom(type))
				{
					DatabaseVersionAttribute[] attribs = (DatabaseVersionAttribute[])type.GetCustomAttributes(typeof(DatabaseVersionAttribute), false);
					if (attribs.Length > 0)
					{
						ITableNames names = (type.GetConstructor(new Type[] {} )).Invoke(new Object[] {}) as ITableNames;
						m_VersionToTablesIndex.Add(((DatabaseVersionAttribute)attribs[0]).Versions[0], names);
					}
				}
			}
		}

		void LoadCachedTypeInformation()
		{
			/** get (or load) the versions supported by this type */
			DataSourceCachedInfo cachedInfo;
			if(m_TypeToCachedInfoIndex.TryGetValue(GetType(), out cachedInfo) == false)
			{
				DatabaseVersionAttribute[] attributes = (DatabaseVersionAttribute[])GetType().GetCustomAttributes(typeof(DatabaseVersionAttribute), false);
				DatabaseVersionAttribute versionAttribute;
				if(attributes.Length != 0)
				{
					versionAttribute = attributes[0];
				}
				else
				{
					versionAttribute = new DatabaseVersionAttribute("1.0");
				}

				Versions = versionAttribute.Versions;
		
				ITableNames tableNames;
				if(!TryLoadAppropriateTableNames(out tableNames))
				{
					throw new DataSourceException("Could not find appropriate table names for {0}", GetType());
				}

				cachedInfo = new DataSourceCachedInfo(Versions, tableNames);
				m_TypeToCachedInfoIndex.Add(GetType(), cachedInfo);
			}

			Tables = cachedInfo.TableNames;
			Versions = cachedInfo.Versions;
		}

		bool TryLoadAppropriateTableNames(out ITableNames tableNames)
		{
			/** get the highest version of the database we support */
			tableNames = null;
			SoftwareVersion highestTablesVersionSupported = new SoftwareVersion();
			SoftwareVersion highestDataSourceVersion = Versions.Max();
			foreach(SoftwareVersion tableNamesVersion in m_VersionToTablesIndex.Keys)
			{
				if(highestDataSourceVersion >= tableNamesVersion && tableNamesVersion > highestTablesVersionSupported)
				{
					highestTablesVersionSupported = tableNamesVersion;
					tableNames = m_VersionToTablesIndex[highestTablesVersionSupported];
				}
			}

			if(tableNames == null)
			{
				tableNames = m_VersionToTablesIndex[m_VersionToTablesIndex.Keys.Min()];
			}

			return tableNames != null;
		}

		#endregion
	}
}
