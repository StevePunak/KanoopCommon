using System;
using System.Collections.Generic;
using System.Text;
using KanoopCommon.Serialization;
using System.ComponentModel;
using KanoopCommon.Extensions;
using System.Diagnostics;
using System.Globalization;

namespace KanoopCommon.CommonObjects
{
	[IsSerializable]
	[TypeConverter(typeof(SoftwareVersionConverter))]
	public class SoftwareVersion : IComparable
	{
		#region Enumerations

		public enum Format
		{
			Standard,
			ThreeDot,
			Hexadecimal
		}

		#endregion

		#region Public Properties

		int _major;
		public int Major
		{
			get { return _major; }
			set { _major = value; }
		}
		int _minor;
		public int Minor
		{
			get { return _minor; }
			set { _minor = value; }
		}
		int _revision;
		public int Revision
		{
			get { return _revision; }
			set { _revision = value; }
		}
		int _build;
		public int Build
		{
			get { return _build; }
			set { _build = value; }
		}

		[IgnoreForSerializationAttribute]
		public UInt64 Value
		{
			get
			{
				UInt64 major = (UInt64)_major;
				UInt64 minor = (UInt64)_minor;
				UInt64 rev = (UInt64)_revision;
				UInt64 build = (UInt64)_build;

				return major << 48 | minor << 32 | rev << 16 | build;
			}
		}

		String _hexOutputFormat;
		public String HexOutputFormat
		{
			get { return _hexOutputFormat; }
			set
			{
				_hexOutputFormat = value;
				ParseHexLengths();
			}
		}

		[IgnoreForSerializationAttribute]
		public static SoftwareVersion Empty
		{
			get { return new SoftwareVersion(); }
		}

		#endregion

		#region Private Members

		int[] _hexLengths;
		char[] _hexPadChars;

		#endregion

		#region Constructors

		public SoftwareVersion()
			: this(0, 0, 0, 0) { }

		public SoftwareVersion(Version v)
			: this(v.Major, v.Minor, v.Build, v.MinorRevision) { }

		public SoftwareVersion(byte[] data)
			: this(data[0], data[1], data[2], data[3]) { }

		public SoftwareVersion(ulong fullVersion)
		{
			String verString = fullVersion.ToString();
			int len = verString.Length;
			if (len > 7)
			{

				this._build = int.Parse(verString.Substring(len - 3));
				this._revision = int.Parse(verString.Substring(len - 5, 2));
				this._minor = int.Parse(verString.Substring(len - 7, 2));
				this._major = int.Parse(verString.Substring(0, len - 7));
			}
		}


		public SoftwareVersion(int major, int minor, int revision = 0, int build = 0)
		{
			_major = major;
			_minor = minor;
			_revision = revision;
			_build = build;
		}

		public SoftwareVersion(String str)
			: this(str, false) {}

		public SoftwareVersion(String str, bool hex)
		{
			Parse(str, hex);
		}

		#endregion

		#region Static Public Methods

		public static SoftwareVersion Parse(String str)
		{
			SoftwareVersion version = new SoftwareVersion(str);
			return version;
		}

		public static bool TryParse(String str, out SoftwareVersion version)
		{
			version = new SoftwareVersion();
			return version.Parse(str, false);
		}

		public static bool TryParseUDS(String major, out SoftwareVersion version)
		{
            bool result = false;
            version = null;
            try
            {
                version = new SoftwareVersion();
                result = version.ParseUDS(major);
            }
            catch(Exception)
            {

            }

            return result;
		}

		public static bool TryParse(UInt32 value, out SoftwareVersion version)
		{
			version = null;
			String strValue = value.ToString();

			int major, minor, revision, build;

			/** start at the back and work forward */
			int offset = strValue.Length - 1;
			if (offset > 3 && int.TryParse(strValue.Substring(offset - 2, 3), out build))
			{
				offset -= 3;
				if (offset > 2 && int.TryParse(strValue.Substring(offset - 1, 2), out revision))
				{
					offset -= 2;
					if (offset >= 2 && int.TryParse(strValue.Substring(offset - 1, 2), out minor))
					{
						offset = offset == 3 ? 2 : 1;
						if (offset >= 0 && int.TryParse(strValue.Substring(0, offset), out major))
						{
							version = new SoftwareVersion(major, minor, revision, build);
						}
					}
				}
			}
			return version != null;
		}

		public static bool TryFindInString(String input, out SoftwareVersion version)
		{
			version = null;
			int offset, length;

			if(TryFindInString(input, out offset, out length))
			{
				String temp = input.Substring(offset, length);
				if(SoftwareVersion.TryParse(temp, out version) == false)
				{
					version = null;
				}
			}

			return version != null;
		}

		public static bool TryFindInString(String input, out int offset, out int length)
		{
			offset = 0;
			length = 0;

			for(int x = 0;x < input.Length &&  offset == 0;x++)
			{
				if(Char.IsDigit(input[x]))
				{
					int y;
					for(y = x;y < input.Length;y++)
					{
						if(Char.IsDigit(input[y]) != true && input[y] != '.' && input[y] != '-')
						{
							break;
						}

					}

					if(y < input.Length)
					{
						offset = x;
						length = y - x;

						if(input[offset + (length - 1)] == '.' || input[offset + (length - 1)] == '-')
						{
							length--;
						}
					}
				}
			}
			return length != 0;
		}

		public static bool IsValid(String str)
		{
			String[] parts = str.Split(new char[] { '-', '.' });
			int maj, min;
			return parts.Length >= 2 &&
					Int32.TryParse(parts[0], out maj) &&
					Int32.TryParse(parts[1], out min);
		}
		public static bool IsValidHex(String str)
		{
			String[] parts = str.Split(new char[] { '-', '.' });
			int maj, min;
			return parts.Length >= 2 &&
					Int32.TryParse(parts[0], NumberStyles.HexNumber, null, out maj) &&
					Int32.TryParse(parts[1], NumberStyles.HexNumber, null, out min);
		}

		#endregion

		#region Public Access Methods

		public bool IsEmpty()
		{
			return (_major == 0 && _minor == 0 && _revision == 0);
		}

		public SoftwareVersion Clone()
		{
			return new SoftwareVersion(_major, _minor, _revision, _build);
		}

		public SoftwareVersion IncrementedMinor()
		{
			return new SoftwareVersion(_major, _minor, _revision++, _build);
		}

		public void Bump()
		{
			if (_build != 0)
			{
				_build++;
			}
			else
			{
				_revision++;
			}
		}

		public UInt32 ToUInt32()
		{
			return (UInt32)ToUInt64();
		}

		public UInt64 ToUInt64()
		{
			return UInt64.Parse(String.Format("{0}{1:00}{2:00}{3:000}", _major, _minor, _revision, _build));
		}

		public int CompareTo(Object other)
		{
			if (other is SoftwareVersion == false)
			{
				throw new Exception("Other object is not SoftwareVersion");
			}

			int result = 0;
			if (this < (SoftwareVersion)other)
			{
				result = -1;
			}
			else if (this > (SoftwareVersion)other)
			{
				result = 1;
			}
			return result;
		}

		public string ToString(bool excludeBuild, Format format = Format.Standard)
		{
			String versionString = String.Empty;
			if (_build == 0 || excludeBuild)
			{
				switch (format)
				{
					case Format.Standard:
					case Format.ThreeDot:
						if (_revision == 0)
							versionString = String.Format("{0}.{1}", _major, _minor);
						else
							versionString = String.Format("{0}.{1}.{2}", _major, _minor, _revision);
						break;

					case Format.Hexadecimal:
						if (_revision == 0)
							versionString = String.Format("{0}{1}{2}", 
								PadHex(_major, _hexLengths[0]), _hexPadChars[0],
								PadHex(_minor, _hexLengths[1]));
						else
							versionString = String.Format("{0}{1}{2}{3}{4}", 
								PadHex(_major, _hexLengths[0]), _hexPadChars[0],
								PadHex(_minor, _hexLengths[1]), _hexPadChars[1],
								PadHex(_revision, _hexLengths[2]));
						break;

					default:
						break;
				}
			}
			else
			{
				if (_build != 0)
				{
					switch (format)
					{
						case Format.Standard:
							versionString = String.Format("{0}.{1}.{2}-{3:000}", _major, _minor, _revision, _build);
							break;
						case Format.ThreeDot:
							versionString = String.Format("{0}.{1}.{2}.{3}", _major, _minor, _revision, _build);
							break;
						case Format.Hexadecimal:
							if (_revision == 0)
								versionString = String.Format("{0}{1}{2}", 
									PadHex(_major, _hexLengths[0]), _hexPadChars[0],
									PadHex(_minor, _hexLengths[1]));
							else
								versionString = String.Format("{0}{1}{2}{3}{4}", 
									PadHex(_major, _hexLengths[0]), _hexPadChars[0],
									PadHex(_minor, _hexLengths[1]), _hexPadChars[1],
									PadHex(_revision, _hexLengths[2]));
							break;
						default:
							break;
					}
				}
				else
				{
					switch (format)
					{
						case Format.Standard:
						case Format.ThreeDot:
							if (_revision != 0)
								versionString = String.Format("{0}.{1}.{2}", _major, _minor, _revision);
							else
								versionString = String.Format("{0}.{1}", _major, _minor);
							break;
						case Format.Hexadecimal:
							break;
						default:
							break;
					}
				}
			}
			return versionString;
		}

		String PadHex(Int32 value, int length, char padChar = '0')
		{
			String output = value.ToString("X");
			while(output.Length < length)
				output = padChar + output;

			return output;
		}
		public string ToString(String format)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char c in format)
			{
				switch (c)
				{
					case 'M':
						sb.AppendFormat("{0}", _major);
						break;

					case 'm':
						sb.AppendFormat("{0}", _minor);
						break;

					case 'r':
						sb.AppendFormat("{0}", _revision);
						break;

					case 'b':
						sb.AppendFormat("{0:000}", _build);
						break;

					case 'B':
						sb.AppendFormat("{0}", _build);
						break;

					default:
						sb.Append(c);
						break;

				}
			}
			return sb.ToString();
		}

		#endregion

		#region Private Methods

		void ParseHexLengths()
		{
			_hexPadChars = new Char[4];
			_hexLengths = new int[4];
			for(int x = 0;x < 4;x++)
			{
				_hexLengths[x] = 0;
				bool started = false;
				for(int y = 0;y < _hexOutputFormat.Length;y++)
				{
					if(_hexOutputFormat[y].ToString() == x.ToString())
					{
						if(!started)
							started = true;
						_hexLengths[x]++;
					}
					else
					{
						if(started)
						{
							_hexPadChars[x] = _hexOutputFormat[y];
							break;
						}
					}
				}
			}
		}

		bool ParseUDS(String major)
		{
			bool result = false;
			if(major.Contains("-"))
			{
				int nMaj, nMin, nRev;
				String[] parts = major.Split('-');
				if(parts.Length >= 3 && int.TryParse(parts[0], out nMaj) && int.TryParse(parts[1], out nMin) && int.TryParse(parts[2], out nRev))
				{
					Major = nMaj;
					Minor = nMin;
					Revision = nRev;
					Build = 0;
					result = true;
				}
			}
			return result;
		}

		bool Parse(String str, bool hex)
		{
			bool result = false;
			if(hex == false && ContainsNumericVersion(str))
			{
				result = ParseNumeric(str);
			}
			else if(hex == true)
			{
				result = ParseHexadecimal(str);
			}
			return result;
		}

		bool ParseNumeric(String str)
		{
			_major = 0;
			_minor = 0;
			_revision = 0;
			_build = 0;

			bool result = false;

			String[] parts = str.Split(new char[] { '-', '.' });
			if (parts.Length >= 2)
			{
				if (Int32.TryParse(parts[0], out _major) &&
					Int32.TryParse(parts[1], out _minor))
				{
					result = true;
				}
				if (parts.Length >= 3)
				{
					Int32.TryParse(parts[2], out _revision);
					if (parts.Length == 4)
					{
						Int32.TryParse(parts[3], out _build);
					}
					else
					{
						_build = 0;
					}
				}
			}
			else
			{
				result = Int32.TryParse(str, out _major);
			}
			return result;
		}

		bool ParseHexadecimal(String value)
		{
			_major = 0;
			_minor = 0;
			_revision = 0;
			_build = 0;

			bool result = false;

			String[] parts = value.Split(new char[] { '-', '.', '_' });
			if (parts.Length >= 2)
			{
				if (Int32.TryParse(parts[0], NumberStyles.HexNumber, null, out _major) &&
					Int32.TryParse(parts[1], NumberStyles.HexNumber, null, out _minor))
				{
					result = true;
				}
				if (parts.Length >= 3)
				{
					Int32.TryParse(parts[2], NumberStyles.HexNumber, null, out _revision);
					if (parts.Length == 4)
					{
						Int32.TryParse(parts[3], NumberStyles.HexNumber, null, out _build);
					}
					else
					{
						_build = 0;
					}
				}
			}
			else
			{
				result = Int32.TryParse(value, NumberStyles.HexNumber, null, out _major);
			}
			return result;
		}

		bool ContainsNumericVersion(string value)
		{
			int x = 0;
			for(;(x < value.Length) && (Char.IsDigit(value[x]) || value[x] == '.' || value[x] == '-');x++);
			return x >= value.Length;
		}

		bool ContainsHexadecimalVersion(string value)
		{
			int x = 0;
			for(;x < value.Length && Char.IsDigit(value[x]) || value[x].IsHex() || value[x] == '-' || value[x] == '_';x++);
			return x >= value.Length;
		}

#endregion

		#region Static Comparison Operator Overrides

		public static bool operator >(SoftwareVersion v1, SoftwareVersion v2) { return v1.Value > v2.Value; }

		public static bool operator <(SoftwareVersion v1, SoftwareVersion v2) { return v1.Value < v2.Value; }

		public static bool operator >=(SoftwareVersion v1, SoftwareVersion v2) { return v1.Value >= v2.Value; }

		public static bool operator <=(SoftwareVersion v1, SoftwareVersion v2) { return v1.Value <= v2.Value; }

		public static bool operator ==(SoftwareVersion v1, SoftwareVersion v2)
		{
			bool bRet = false;

			if ((object)v1 == null && (object)v2 == null)
			{
				bRet = true;
			}
			else if ((object)v2 == null || (object)v1 == null)
			{
				bRet = false;
			}
			else
			{
				bRet = v1.Value == v2.Value;
			}
			return bRet;
		}

		public static bool operator !=(SoftwareVersion v1, SoftwareVersion v2)
		{
			return !(v1 == v2);

		}

		#endregion

		#region Sorter Classes

		public class VersionSorter : IComparer<SoftwareVersion>
		{
			public int Compare(SoftwareVersion x, SoftwareVersion y)
			{
				int result = 0;
				if (x > y)
				{
					result = 1;
				}
				else if (x < y)
				{
					result = -1;
				}
				return result;
			}
		}

#endregion

		#region Utility

		public override bool Equals(object obj)
		{
			return this == (SoftwareVersion)obj;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return ToString(false);
		}

		public string ToString(Format format)
		{
			return ToString(false, format);
		}

		#endregion
	}

	public class SoftwareVersionConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			SoftwareVersion version = null;
			string valueString = value as string;
			if (valueString != null)
				version = new SoftwareVersion(valueString);
			else
				version = new SoftwareVersion();
			return version;
		}

	}
}
