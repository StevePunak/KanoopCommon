using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Database
{
	public class TableNameAttribute : StringAttribute	
	{
		public Type RepresentativeType { get; private set; }

		public TableNameAttribute(String value) 
			: this(value, null) {}

		public TableNameAttribute(String value, Type representativeType) 
			: base(value) 
		{
			RepresentativeType = representativeType;
		} 
	}

	public class FunctionNameAttribute : TableNameAttribute
	{
		public FunctionNameAttribute(String value) 
			: this(value, null) {}

		public FunctionNameAttribute(String value, Type representativeType) 
			: base(value, representativeType) {} 
	}


	public class DefaultSchemaNameAttribute : StringAttribute { public DefaultSchemaNameAttribute(String value) : base(value) { } }

}
