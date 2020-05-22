using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.Geometry
{
	public enum Direction
	{
		None,

		Forward,
		Backward,
		Up,
		Down,
	}

	public enum MomentarySwitchMode
	{
		NormalOpen, NormalClosed
	}

	public enum SpinDirection
	{
		None,

		Clockwise,
		CounterClockwise
	}

	public enum ImageDirection
	{
		Left,
		Right,
		Top,
		Bottom
	}

}
