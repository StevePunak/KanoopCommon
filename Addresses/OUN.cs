using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace KanoopCommon.Addresses
{
	public class OUN : IComparable
	{
		UInt32 _value;
		public UInt32 Value 
		{ 
			get { return _value; } 
			set { _value = value; }
		}

		public OUN(String value)
		{
			if(!TryParse(value, out _value))
			{
				throw new FormatException("Invalid OUN passed to constructor");
			}
		}

		public OUN(UInt32 value)
		{
			_value = value;
		}

		public override string ToString()
		{
			return String.Format("{0:X2}:{1:X2}:{2:X2}", 
				(_value >> 16) & 0xff,
				(_value >> 8) & 0xff,
				_value & 0xff);
		}

		public int CompareTo(Object other)
		{
			int ret = 0;
			if(other is OUN)
				ret = _value.CompareTo(((OUN)other).Value);
			else if(other is UInt32)
				ret = _value.CompareTo((UInt32)other);
			else
				throw new NotSupportedException();
			return ret;
		}

		private static bool TryParse(String unparsed, out UInt32 parsed)
		{
			bool success = false;
			parsed = 0;

			String[] parts;
			int b1, b2, b3;

			/** Format: 00:01:FE */
			if(	(unparsed.Length == 8) && 
				(parts = unparsed.Split(':')).Length == 3 &&
				Int32.TryParse(parts[0], NumberStyles.HexNumber, null, out b1) &&
				Int32.TryParse(parts[1], NumberStyles.HexNumber, null, out b2) &&
				Int32.TryParse(parts[2], NumberStyles.HexNumber, null, out b3))
			{
				parsed = (UInt32)(b1 << 16 | b2 << 8 | b3);
				success = true;
			}
			/** Format: 0x0012FE */
			else if(unparsed.Length > 2 && 
					unparsed.Substring(0, 2).ToLower() == "0x" &&
					Int32.TryParse(unparsed.Substring(2), NumberStyles.HexNumber, null, out b1))
			{
				parsed = (UInt32)b1;
				success = true;
			}
			/** Format: 0012FE */
			else if(unparsed.Length > 1 && 
					Int32.TryParse(unparsed, NumberStyles.HexNumber, null, out b1))
			{
				parsed = (UInt32)b1;
				success = true;
			}
			return success;
		}

		public static bool TryParse(String value, out OUN oun)
		{
			oun = null;
			UInt32 parsed;
			if(TryParse(value, out parsed))
			{
				oun = new OUN(parsed);
			}
			return oun != null;
		}
	}
}
