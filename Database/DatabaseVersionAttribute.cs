using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Database
{
	public class DatabaseVersionAttribute : Attribute	
	{
		public List<UInt32> NumericVersions { get; private set; }

		public List<SoftwareVersion> Versions { get; private set; }

		public DatabaseVersionAttribute(params Object[] versions)
		{
			NumericVersions = new List<UInt32>();
			Versions = new List<SoftwareVersion>();

			/** validate parameter count */
			if (versions == null || versions.Length == 0)
			{
				throw new Exception("Invalid parameter to DatabaseVersion Attrbute");
			}

			/** parse each parameter */
			foreach(Object o in versions)
			{
				SoftwareVersion version = null;
				if(o is String)
				{
					/** parse textual versions */
					if(SoftwareVersion.TryParse(o as String, out version) == false)
					{
						throw new Exception("Invalid version number in Version Attribute");
					}
				}
				else if(o is int)
				{
					/** parse integral versions */
					if(SoftwareVersion.TryParse(Convert.ToUInt32(o), out version) == false)
					{
						throw new Exception("Invalid version number in Version Attribute");
					}
				}
				else
				{
					throw new Exception(String.Format("Unparsable data type {0} in Database Version Attribute", o.GetType()));
				}

				NumericVersions.Add(version.ToUInt32());
				Versions.Add(version);
			}
		}
	}

}
