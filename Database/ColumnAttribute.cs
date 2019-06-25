using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace KanoopCommon.Database
{
	public class ColumnNameAttribute : Attribute
	{
		private readonly List<String> m_NameList = new List<string>();
		
		public ColumnNameAttribute(params String[] names)
		{
			if(names == null || names.Length == 0)
				throw new Exception("Invalid parameter to Column Name Attrbute");

			for (int i = 0; i < names.Length; i++)
				m_NameList.Add(names[i]);
		}
		public List<String> NameList
		{
			get { return m_NameList; }
		}

		public String ActualName { get { return m_NameList[0]; } }
	}

}
