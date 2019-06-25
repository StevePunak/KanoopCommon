using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System;

namespace KanoopCommon.CommonObjects
{
	/// <summary>
	/// This class defines a pair of colors (background/foreground) is a good use case
	/// </summary>
	public class ColorPair
	{
		Color	m_ForeColor;
		public Color ForeColor { get; set; }
		public Color BackColor { get; set; }

		public ColorPair(Color fore, Color back)
		{
			ForeColor = fore;
			BackColor = back;
		}

		public ColorPair(String str)
		{
			String[] parts = str.Split(',');
			ForeColor = Color.FromArgb(Convert.ToInt32(parts[0]));
			BackColor = Color.FromArgb(Convert.ToInt32(parts[1]));
		}

		public bool Equals(Color foreColor, Color backColor)
		{
			return m_ForeColor.Equals(foreColor) && BackColor.Equals(backColor);
		}

		public bool Equals(ColorPair other)
		{
			return m_ForeColor.Equals(other.ForeColor) && BackColor.Equals(other.BackColor);
		}

		public override string ToString()
		{
			return String.Format("{0},{1}", m_ForeColor.ToArgb(), BackColor.ToArgb());
		}

		public ColorPair Clone()
		{
			return new ColorPair(m_ForeColor, BackColor);
		}
	}
}
