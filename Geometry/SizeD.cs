using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
	public class SizeD
	{
		Double m_nWidth;
		public Double Width { get { return m_nWidth; } }

		Double m_nHeight;
		public Double Height { get { return m_nHeight; } }

		public SizeD()
			: this(0, 0) {}

		public SizeD(Double width, Double height)
		{
			m_nWidth = width;
			m_nHeight = height;
		}

		public override string ToString()
		{
			return String.Format("W: {0:0.000} H: {1:0.000}", m_nWidth, m_nHeight);
		}

		public static SizeD Empty { get { return new SizeD(); } }

	}
}
