using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Database;

namespace KanoopCommon.Database.TableNames
{
	public interface ITableNames
	{
		#region	Methods

		bool ChangeSchemaForTableName(String schema, String	table);

		#endregion
		
		#region	Packaging

		String Packages { get; }

		#endregion
	}
}
