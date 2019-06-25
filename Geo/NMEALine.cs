using KanoopCommon.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Geo
{
	//$GPGSA,A,3,27,08,23,05,09,07,30,16,,,,,2.28,1.43,1.78*01
	//$GPRMC,002638.000,A,3326.7549,N,11738.6933,W,0.01,318.27,020119,,,D*7B
	//$GPVTG,318.27,T,,M,0.01,N,0.02,K,D*34
	//$GPGGA,002639.000,3326.7549,N,11738.6933,W,2,08,1.43,15.3,M,-34.7,M,0000,0000*6F
	//$GPGSA,A,3,27,08,23,05,09,07,30,16,,,,,2.28,1.43,1.78*01
	//$GPRMC,002639.000,A,3326.7549,N,11738.6933,W,0
	public abstract class NMEALine
	{
		#region Enumerations

		public enum NMEALineType
		{
			Invalid = 0,

			MinimumTransitData
		}

		#endregion

		#region Public Properties

		public abstract NMEALineType LineType { get; }

		#endregion

		#region Private Member Variables

		static Dictionary<String, NMEALineType> _typeIndex;

		#endregion

		#region Constructors

		static NMEALine()
		{
			_typeIndex = new Dictionary<string, NMEALineType>()
			{
				{  "$GPRMC",   NMEALineType.MinimumTransitData }
			};
		}

		#endregion

		#region Public Methods

		public static bool TryParse(String input, out NMEALine line)
		{
			line  = null;

			String[] parts = input.Split(',');
			NMEALineType type;
			if (parts.Length > 0 && _typeIndex.TryGetValue(parts[0], out type))
			{
				switch(type)
				{
					case NMEALineType.MinimumTransitData:
						line = new MinimumTransitData(parts);
						break;
				}
			}

			return line != null;
		}

		#endregion

		#region Utility

		protected Double ParseDegrees(String input)
		{
			Double ret = 0;

			int index = input.IndexOf('.');
			index -= 2;

			if (index > 0)
			{
				Double degrees = Double.Parse(input.Substring(0, index));

				String minuteString = input.Substring(index);
				Double decValue = Double.Parse(minuteString) / 60;

				ret = Math.Round(degrees + decValue, 6);
			}

			return ret;
		}

		#endregion
	}

	public class MinimumTransitData : NMEALine
	{
		#region Public Properties

		public override NMEALineType LineType { get { return NMEALineType.MinimumTransitData; } }

		public DateTime TimeOfFix { get; private set; }
		public Double Latitude { get; private set; }
		public Double Longitude { get; private set; }
		public CardinalDirection LatitudeNS { get; private set; }
		public bool Valid { get; private set; }

		#endregion

		internal MinimumTransitData(String[] parts)
		{
			//$GPRMC,002639.000,A,3326.7549,N,11738.6933,W,0
			DateTime now = DateTime.UtcNow;
			int hour = int.Parse(parts[1].Substring(0, 2));
			int minute = int.Parse(parts[1].Substring(2, 2));
			int second = int.Parse(parts[1].Substring(4, 2));
			TimeOfFix = new DateTime(now.Year, now.Month, now.Day, hour, minute, second);

			Valid = parts[2] == "A" ? true : false;

			Latitude = ParseDegrees(parts[3]);
			LatitudeNS = parts[4] == "N" ? CardinalDirection.North : CardinalDirection.South;

			Longitude = ParseDegrees(parts[5]);
			if(parts[6] == "W")
			{
				Longitude *= -1;
			}
		}
	}
}
