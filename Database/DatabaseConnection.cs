using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace KanoopCommon.Database
{
	public class DatabaseConnection //  : IDisposable
	{
		DbConnection m_Connection;
		public DbConnection DbConnection
		{
			get { return m_Connection; }
			set { m_Connection = value; }
		}

		
		DatabasePerformanceTimer		m_SelectTimer;
		public DatabasePerformanceTimer SelectTimer
		{ 
			get 
			{
				if(m_SelectTimer == null)
				{
					m_SelectTimer = new DatabasePerformanceTimer("SELECT");
				}
				return m_SelectTimer; 
			} 
		}

		DatabasePerformanceTimer		m_InsertTimer;
		public DatabasePerformanceTimer InsertTimer
		{ 
			get 
			{
				if(m_InsertTimer == null)
				{
					m_InsertTimer = new DatabasePerformanceTimer("INSERT");
				}
				return m_InsertTimer; 
			} 
		}

		DatabasePerformanceTimer		m_UpdateTimer;
		public DatabasePerformanceTimer UpdateTimer
		{ 
			get 
			{
				if(m_UpdateTimer == null)
				{
					m_UpdateTimer = new DatabasePerformanceTimer("UPDATE");
				}
				return m_UpdateTimer; 
			} 
		}

		private int m_nConnectionRetries;
		public int ConnectionRetries
		{
			get { return m_nConnectionRetries; }
			set { m_nConnectionRetries = value; }
		}

		private TimeSpan m_ConnectionRetryInterval;
		public TimeSpan ConnectionRetryInterval
		{
			get { return m_ConnectionRetryInterval;  }
			set { m_ConnectionRetryInterval = value; }
		}


		DateTime						m_LastUse;
		public DateTime LastUse
		{ 
			get { return m_LastUse; }
			set { m_LastUse = value; }
		}

		public DatabaseConnection(DbConnection connection, SqlDBCredentials credentials)
		{
			m_Connection = connection;
			m_LastUse = DateTime.UtcNow;
			m_nConnectionRetries = credentials.ConnectionRetries;
			m_ConnectionRetryInterval = credentials.ConnectionRetryInterval;
		}

		~DatabaseConnection()
		{
			//if (m_Connection is Mysql)
			//    m_Connection.Close();
			if (!(m_Connection is System.Data.SqlClient.SqlConnection))
				m_Connection.Dispose();
		}

		public override string ToString()
		{
			return String.Format("DbConn ({0})", GetHashCode());
		}

		//public void Dispose()
		//{
		//}
	}
}
