using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Logging
{
	class BinaryLogEntry
	{
		String			_label;
		public String Label { get { return _label; } }

		byte[]			_data;
		public byte[] Data { get { return _data; } }

		public BinaryLogEntry(String label, byte[] data)
		{
			_label = label;
			_data = data;

		}
	}
}
