using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.PersistentConfiguration
{
	public class IniFileSectionList : List<IniFileSection>
	{
		public IniFileSectionList()
			: base() {}

		public IniFileSectionList(IEnumerable<IniFileSection> other)
			: base(other) {}

	}

	public class IniFileSection
	{
		#region Public Proerties

		public String Name { get; private set; }

		public Dictionary<String, String> Elements { get; private set; }

		public int LineNumber { get; private set; }

		#endregion

		#region Constructors

		public IniFileSection(String name, int lineNumber)
		{
			Name = name;
			Elements = new Dictionary<String, String>();
			LineNumber = lineNumber;
		}

		#endregion

		#region Utility

		public override string ToString()
		{
			return String.Format("Section '{0}' - {1} elements", Name, Elements.Count);
		}

		#endregion
	}
}
