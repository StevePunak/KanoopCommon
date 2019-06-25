using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KanoopCommon.Linux;
using KanoopCommon.Serialization;

namespace KanoopCommon.Database
{

	public class SqlDBCredentialsList : List<SqlDBCredentials>
	{
		public SqlDBCredentialsList()
			: base() {}

		public SqlDBCredentialsList(IEnumerable<SqlDBCredentials> other)
			: base(other) {}

		public List<String> GetHostNames()
		{
			List<String> names = new List<String>();
			foreach(SqlDBCredentials credentials in this)
			{
				names.Add(credentials.Host);
			}
			return names;
		}

		public bool TryGetByHost(String host, out SqlDBCredentials credentials)
		{
			credentials = Find(c => c.Host.Equals(host, StringComparison.InvariantCultureIgnoreCase));
			return credentials != null;
		}
	}
	
	[IsSerializable]
	public class SqlDBCredentials
	{
		#region Constants

		const int DEFAULT_RETRIES =				3;
		const int DEFAULT_OPTIMAL_POOL_SIZE =	8;

		static readonly TimeSpan DEFAULT_STALE_CONNECTION_CLOSE_TIME = TimeSpan.FromMinutes(2);

		#endregion

		#region Public Properties

		private string _dsn;
		public string DSN
		{
			get 
			{
				if(_dsn == null)
				{
					_dsn = String.Empty;
				}
				return _dsn; 
			}
			set { _dsn = value; }
		}

		private string _driver;
		public string Driver
		{
			get 
			{
				if(_driver == null)
				{
					_driver = String.Empty;
				}
				return _driver; 
			}
			set { _driver = value; }
		}

		private string _host;
		public string Host
		{
			get 
			{
				if(_host == null)
				{
					_host = String.Empty;
				}
				return _host; 
			}
			set { _host = value; }
		}

		private string _schema;
		public string Schema
		{
			get 
			{
				if(_schema == null)
				{
					_schema = String.Empty;
				}
				return _schema; 
			}
			set { _schema = value; }
		}

		private string _userName;
		public string UserName
		{
			get 
			{
				if(_userName == null)
				{
					_userName = String.Empty;
				}
				return _userName; 
			}
			set { _userName = value; }
		}

		private string _password;
		public string Password
		{
			get 
			{
				if(_password == null)
				{
					_password = String.Empty;
				}
				return _password; 
			}
			set { _password = value; }
		}

		public string Options { get; set; }

		private int _connectionRetries;
		public int ConnectionRetries
		{
			get { return _connectionRetries; }
			set { _connectionRetries = value; }
		}

		private int _OptimalPoolSize;
		public int OptimalPoolSize { get { return _OptimalPoolSize; } set { _OptimalPoolSize = value; } }

		private TimeSpan _StaleConnectionCloseTime;
		public TimeSpan StaleConnectionCloseTime { get { return _StaleConnectionCloseTime; } set { _StaleConnectionCloseTime = value; } }

		private TimeSpan _connectionRetryInterval;
		public TimeSpan ConnectionRetryInterval
		{
			get { return _connectionRetryInterval;  }
			set { _connectionRetryInterval = value; }
		}

		public string Charset { get; set; }

		public bool IsSSH { get; set; }


		public SSHCredentials SSHCredentials { get; set; }

		[IgnoreForSerialization]
		public static String PasswordToken { get { return "[password]"; } }

		#endregion

		#region Constructors

		public SqlDBCredentials()
			: this(SqlDataSource.MYSQL_NATIVE_DRIVER, String.Empty, String.Empty, String.Empty, String.Empty) {}

		public SqlDBCredentials(String parseable)
			: this(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty, String.Empty) 
		{
			TryParse(parseable);
		}

		public SqlDBCredentials(string driver, string host, string schema, string username, string password)
			: this(String.Empty, driver, host, schema, username, password) 
		{
		}

		public SqlDBCredentials(string dsn, string driver, string host, string schema, string username, string password)
			: this(dsn, driver, host, schema, username, password, new SSHCredentials()) { }

		public SqlDBCredentials(string dsn, string driver, string host, string schema, string username, string password, SSHCredentials sshCredentials)
		{
			_dsn = dsn;
			_driver = driver;
			_host = host;
			_schema = schema;
			_userName = username;
			_password = password;
			_connectionRetryInterval = TimeSpan.Zero;
			_connectionRetries = DEFAULT_RETRIES;
			Options = "";
			_OptimalPoolSize = DEFAULT_OPTIMAL_POOL_SIZE;
			_StaleConnectionCloseTime = DEFAULT_STALE_CONNECTION_CLOSE_TIME;
			SSHCredentials = sshCredentials;
			IsSSH = SSHCredentials.IsEmpty == false;
		}

		#endregion

		#region Utility

		public SqlDBCredentials Clone(String newDefaultSchema = null)
		{
			SqlDBCredentials creds = (SqlDBCredentials)this.MemberwiseClone();
			if(newDefaultSchema != null)
			{
				creds.Schema = newDefaultSchema;
			}
			return creds;
		}

		public override bool Equals(object obj)
		{
			if(obj is SqlDBCredentials == false)
			{
				throw new InvalidCastException("Other object is not SqlDBCredentials");
			}
			SqlDBCredentials credentials = obj as SqlDBCredentials;

			return	credentials.Schema == Schema &&
					credentials.Host == Host &&
					credentials.Driver == Driver &&
					credentials.UserName == UserName &&
					credentials.Password == Password;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		// Driver={MySQL ODBC 5.1 Driver};UID=service_1;PWD=passw0rd;Server=lindev01;Database=sitewerx;
		public override string ToString()
		{
			return String.Format("Driver={0};UID={1};PWD={2};Server={3};Database={4}",
				_driver, _userName, _password, _host, _schema);
		}

		public string ToParsableString()
		{
			return String.Format("Driver={0};UID={1};PWD={2};Server={3};Database={4}",
				_driver, _userName, _password, _host, _schema);
		}

		public bool IsValid()
		{
			return Host.Length > 0 && UserName.Length > 0 && Password.Length > 0 && Schema.Length > 0 && Driver.Length > 0;
		}

		public static bool IsValid(String str)
		{
			SqlDBCredentials creds;
			return TryParse(str, out creds);
		}

		public static SqlDBCredentials Empty { get { return new SqlDBCredentials(); } }

		public static bool TryParse(String str, out SqlDBCredentials creds)
		{
			creds = new SqlDBCredentials();
			return creds.TryParse(str);
		}

		private bool TryParse(String str)
		{
			/**
			 * Parse:
			 * u=user;p=password;h=spock;s=sitewerx
			 */
			
			Driver = SqlDataSource.MYSQL_NATIVE_DRIVER;	/** default to this */

			String options="";
			String[] parts = str.Split(';');

			foreach(String part in parts)
			{
				String[] keyval = part.Split('=');

				if(keyval.Length == 1)
				{
					keyval = part.Split('@');
				}

				if(keyval.Length == 2)
				{
					switch(keyval[0].ToUpper())
					{
						case "U":
						case "UID":
							UserName = keyval[1];
							break;

						case "P":
						case "PWD":
							Password = keyval[1];
							break;

						case "H":
						case "SERVER":
							Host = keyval[1];
							break;

						case "S":
						case "DATABASE":
							Schema = keyval[1];
							break;

						case "DRIVER":
						{
							Driver = keyval[1];
							break;
						}
						default:
							options+=part+";";
							break;
					}
				}

			}
			Options = options;

			return UserName.Length > 0 &&  Host.Length > 0;
		}

		#endregion

		#region Comparers

		public class HostNameComparer : IComparer<SqlDBCredentials>
		{
			public int Compare(SqlDBCredentials x, SqlDBCredentials y)
			{
				return x.Host.CompareTo(y.Host);
			}
		}

		#endregion
	}
}
