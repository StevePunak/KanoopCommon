using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KanoopCommon.Geometry
{
	public class ExtendedRectangle
	{
		Rectangle		m_Rectangle;
		public Rectangle Rectangle
		{
			get { return m_Rectangle; }
		}

		public Line TopEdge
		{
			get { return new Line(new Point(m_Rectangle.Left, m_Rectangle.Top), new Point(m_Rectangle.Right, m_Rectangle.Top)); }
		}

		public Line BottomEdge
		{
			get { return new Line(new Point(m_Rectangle.Left, m_Rectangle.Bottom), new Point(m_Rectangle.Left, m_Rectangle.Bottom)); }
		}

		public Line LeftEdge
		{
			get { return new Line(new Point(m_Rectangle.Left, m_Rectangle.Top), new Point(m_Rectangle.Left, m_Rectangle.Bottom)); }
		}

		public Line RightEdge
		{
			get { return new Line(new Point(m_Rectangle.Right, m_Rectangle.Top), new Point(m_Rectangle.Right, m_Rectangle.Bottom)); }
		}

		public ExtendedRectangle(Rectangle rect)
		{
			m_Rectangle = rect;
		}

		public bool Contains(Point p)
		{
			return	p.X >= m_Rectangle.Left &&
					p.X <= m_Rectangle.Right &&
					p.Y >= m_Rectangle.Top &&
					p.Y <= m_Rectangle.Bottom;
		}


	}
}
