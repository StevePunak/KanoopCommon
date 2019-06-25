using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Database;
namespace KanoopCommon.Database.TableNames
{
	[DefaultSchemaName("packaging")]
	public class PackagingTables : SchemaNameDefinition
	{
		[TableName("packages")]
		public String Packages { get; private set; }

	}
}
