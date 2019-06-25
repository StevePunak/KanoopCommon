using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Database;

namespace KanoopCommon.Database.TableNames.v1_0
{
	[DatabaseVersion("1.0")]
	public class TableNames1_0 : ITableNames
	{
		const String UNDEFINED = "Undefined";

		protected PackagingTables			_packagingTables;

		public List<SchemaNameDefinition> AllSchemas { get; private set; }

		public TableNames1_0()
		{
			_packagingTables = new PackagingTables();

			AllSchemas = new List<SchemaNameDefinition>()
			{
				_packagingTables
			};

		}

		#region Methods

		public bool ChangeSchemaForTableName(String schema, String table)
		{
			bool result = false;

			foreach(SchemaNameDefinition schemaDefinition in AllSchemas)
			{
				if(schemaDefinition.ChangeSchemaForTable(schema, table))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Packaging

		public virtual String Packages  { get { return _packagingTables.Packages; } }

		#endregion

	}


}
