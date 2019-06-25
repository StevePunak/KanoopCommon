using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public enum DbType
	{
		Unknown = 0,

		BigInt,
		Binary,
		Bit,
		Char,
		DateTime,
		Decimal,
		Float,
		Image,
		Int,
		Money,
		NChar,
		NText,
		NVarChar,
		Real,
		UniqueIdentifier,
		SmallDateTime,
		SmallInt,
		SmallMoney,
		Text,
		Timestamp,
		TinyInt,
		VarBinary,
		VarChar,
		Variant,
		Xml,
		Udt,
		Structured,
		Date,
		Time,
		DateTime2,
		DateTimeOffset,
	};

	public class DatabaseParameter
	{
		DbType		m_Type;
		public DbType Type
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		String		m_strName;
		public String Name
		{
			get { return m_strName; }
			set { m_strName = value; }
		}

		Object		m_Value;
		public Object Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		int			m_nSize;
		public int Size
		{
			get { return m_nSize; }
			set { m_nSize = value; }
		}

		public DatabaseParameter(String name)
			: this(name, DbType.Unknown, 0, null) {}

		public DatabaseParameter(String name, DbType type, Object value)
			: this(name, type, 0, value) {}

		public DatabaseParameter(String name, Object value, int size)
			: this(name, DbType.Unknown, size, value) {}

		public DatabaseParameter(String name, DbType nativeType, int size, Object value)
		{
			m_Type = nativeType;
			m_strName = name;
			m_Value = value;
			m_nSize = size;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class DatabaseParameterList : List<DatabaseParameter> {}
}
