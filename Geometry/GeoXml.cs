using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class GeoXml
	{
		#region Node Tags

		public const String GeoLineList =			"GeoLineList";
		public const String GeoPolygon =			"GeoPolygon";
		public const String Line  =					"Line";

		#endregion

		#region Attribute Tags

		public const String Center =				"center";
		public const String MajorAxisBearing =		"major_axis_bearing";
		public const String MajorAxisLength =		"major_axis_length";
		public const String MinorAxisLength =		"minor_axis_length";

		public const String Type =					"type";

		#endregion
	}
}
