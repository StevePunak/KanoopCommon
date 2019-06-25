using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public class DBResult
	{
		public enum Result
		{
			Success = 0,
			PermissionsFault = 1,
			UpdateFailure = 2,
			InsertionFailure = 3,
			DeleteFailure = 4,
			NoData = 5,
			AuthenticationFailure = 6,
			ConnectionDropped = 7,
			NonExistantSchema = 8,
			IncompleteData = 9,
			Pending = 10,

			Exception = 254,
			Invalid = 255,
		}

		public DBResult()
			: this(Result.Invalid, "", 0) { }

		public DBResult(Result result, String strExtendedInfo)
			: this(result, strExtendedInfo, 0) { }

		public DBResult(Result result)
			: this(result, "", 0) { }

		public DBResult(Result result, String strExtendedInfo, UInt64 itemID)
		{
			m_nResultCode = result;
			m_strMessage = strExtendedInfo;
			m_nItemID64 = itemID;
		}

		public static DBResult Empty { get { return new DBResult(Result.Invalid, String.Empty); } }

		Result m_nResultCode;
		public Result ResultCode
		{
			get { return m_nResultCode; }
			set { m_nResultCode = value; }
		}

		String m_strMessage;
		public String Message
		{
			get { return m_strMessage; }
			set { m_strMessage = value; }
		}

		UInt64 m_nItemID64;
		public UInt32 ItemID
		{
			get { return (UInt32)m_nItemID64; }
			set { m_nItemID64 = value; }
		}

		public UInt64 ItemID64
		{
			get { return m_nItemID64; }
			set { m_nItemID64 = value; }
		}

		Int32 m_nRowsAffected;
		public Int32 RowsAffected
		{
			get { return m_nRowsAffected; }
			set { m_nRowsAffected = value; }
		}

		public bool IsSuccessOrNoData { get { return m_nResultCode == Result.Success || m_nResultCode == Result.NoData; } }

		public static DBResult Success { get { return new DBResult(Result.Success); } }

		public static DBResult NoData { get { return new DBResult(Result.NoData); } }

		public static DBResult InsertionFailure { get { return new DBResult(Result.InsertionFailure); } }

		public static DBResult UpdateFailure { get { return new DBResult(Result.UpdateFailure); } }

		public override string ToString()
		{
			return String.Format("{0}{1}{2}", m_nResultCode, (m_strMessage.Length > 0) ? " - " : "", m_strMessage);
		}
	}
}
