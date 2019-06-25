using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace KanoopCommon.Database
{
	public class SchemaNameDefinition
	{
		#region Public Properties

		String m_strSchemaName;
		public String SchemaName
		{
			get
			{
				if (m_strSchemaName == null)
				{
					throw new Exception("Base constructor never called?");
				}
				return m_strSchemaName;
			}
			set
			{
				m_strSchemaName = value;
				RebuildTableNames();
			}
		}

		#endregion

		#region Constructors

		protected SchemaNameDefinition()
		{
			Object[] attrs = GetType().GetCustomAttributes(typeof(DefaultSchemaNameAttribute), false);
			if (attrs == null || attrs.Length == 0)
			{
				throw new Exception("Schema must have DefaultSchemaName attribute");
			}

			m_strSchemaName = ((DefaultSchemaNameAttribute)attrs[0]).Value;

			RebuildTableNames();
		}

		#endregion

		#region Public Access Methods

		public bool ChangeSchemaForTable(String schema, String table)
		{
			bool result = false;
			String tableOnly = table.Contains(".") ? table.Substring(table.LastIndexOf(".") + 1) : table;

			PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo property in properties)
			{
				Object[] attrs = property.GetCustomAttributes(typeof(TableNameAttribute), false);
				if (attrs == null || attrs.Length == 0)
				{
					continue;
				}

				TableNameAttribute attr = attrs[0] as TableNameAttribute;

				Object propVal = property.GetValue(this, null);
				if(attr.Value.Equals(tableOnly))
				{
					property.SetValue(this, schema + "." + ((TableNameAttribute)attrs[0]).Value, null);
					result = true;
					break;
				}
			}
			return result;
		}

		#endregion

		#region Utility

		protected void RebuildTableNames()
		{
			PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (PropertyInfo property in properties)
			{
				Object[] attrs = property.GetCustomAttributes(typeof(TableNameAttribute), false);
				if (attrs == null || attrs.Length == 0)
				{
					continue;
				}

				property.SetValue(this, SchemaName + "." + ((TableNameAttribute)attrs[0]).Value, null);
			}
		}

		#endregion

	}
}
