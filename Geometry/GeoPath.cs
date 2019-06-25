using KanoopCommon.Conversions;
using KanoopCommon.Database;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System;
using KanoopCommon.LocationAlgorithms;

namespace KanoopCommon.Geometry
{
	public class GeoPath : GeoLineList
	{
		#region Public Properties

		public GeoPointList GeoPoints
		{
			get
			{
				GeoPointList retVal = new GeoPointList();

				for(int i = 0; i < this.Count; i++)
				{
					if(i == 0 || this[i].P1.Equals(this[i - 1].P2) == false)
					{
						retVal.Add(this[i].P1 as GeoPoint);
					}
					retVal.Add(this[i].P2 as GeoPoint);
				}
				return retVal;
			}
		}

		public GeoPointList Vertices
		{
			get
			{
				GeoPointList vertices = new GeoPointList();
				foreach(GeoLine line in this)
				{
					if(vertices.Contains(line.P1) == false)
						vertices.Add(line.P1 as GeoPoint);
					if(vertices.Contains(line.P2) == false)
						vertices.Add(line.P2 as GeoPoint);

				}
				return vertices;
			}
		}

		public Double Length
		{
			get
			{
				Double length = 0;

				for(int x = 0; x < Count; x++)
				{
					length += this[x].Length;
				}
				return length;
			}
		}

		#endregion

		#region Constructor(s)

		public GeoPath()
			: base() {}

		public GeoPath(GeoPath other)
			: base(other) {}

		public GeoPath(GeoPointList points)
			: base(points) {}

		public GeoPath(GeoLineList lines)
			: base(lines) {}

		public GeoPath(PointDList list)
			: base(list) {}

		#endregion

		#region Public Geometric Functions

		public void MoveGeo(Double bearing, Double distance)
		{
			foreach(GeoLine ln in this)
			{
				ln.P1.Move(bearing, distance);
				ln.P2.Move(bearing, distance);
			}
		}

		public bool TryGetPointAtDistanceFromEnd(Double distance, out GeoPoint point)
		{
			point = null;

			Double		totalLength = 0;
			GeoLineList list = new GeoLineList(this);
			list.Reverse();

			for(int index = 0; point == null && index < list.Count; index++)
			{
				GeoLine line = list[index];
				Double	newLength = totalLength + line.Length;
				if(newLength > distance)
				{
					Double segLength = distance - totalLength;
					point = EarthGeo.GetPoint(line.P2 as GeoPoint, Angle.Reverse(line.Bearing), segLength);
				}
				else
				{
					totalLength = newLength;
				}
			}
			return point != null;
		}

		public bool TryGetDistanceFromEnd(GeoPoint point, out Double distance)
		{
			bool		success = false;

			distance = 0;
			GeoLineList list = new GeoLineList(this);
			list.Reverse();

			for(int index = 0; index < list.Count; index++)
			{
				GeoLine		line = list[index];
				GeoPoint	closest;

				Double		thisDistance;
				closest = line.ClosestPointTo(point, out thisDistance);

				/** less than this distance, he is on the line */
				if(thisDistance > .1)
				{
					distance += line.Length;
				}
				else
				{
					distance += EarthGeo.GetDistance(line.P2 as GeoPoint, closest);
					success = true;
					break;
				}
			}
			return success;
		}

		public bool TryGetDistanceBetweenTwoPointsOnPath(GeoPoint p1, GeoPoint p2, out Double distance)
		{
			bool	success = false;

			distance = 0;

			/** get the line each point lies on */
			GeoLine l1, l2;
			Double	d1, d2;
			if(TryGetLineForPoint(p1, .1, out l1) && TryGetLineForPoint(p2, .1, out l2))
			{
				if(l1.Equals(l2))
				{
					distance = EarthGeo.GetDistance(p1, p2);
					success = true;
				}
				else if(TryGetDistanceFromEnd(p1, out d1) && TryGetDistanceFromEnd(p2, out d2))
				{
					if(d1 > d2)
					{
						distance = GetDistanceForTwoOrderedPoints(l1, p1, l2, p2);
					}
					else
					{
						distance = GetDistanceForTwoOrderedPoints(l2, p2, l1, p1);
					}
					success = true;
				}
			}
			return success;
		}

		public bool TryFindPointForwardOnPath(GeoPoint origin, Double distance, out GeoPoint final)
		{
			final = null;
			GeoLine line;
			return TryGetLineForPoint(origin, .1, out line) && TryMoveForwardOnPath(line, origin, distance, out final);
		}

		public bool TryFindPointBackwardOnPath(GeoPoint origin, Double distance, out GeoPoint final)
		{
			final = null;
			GeoLine line;
			return TryGetLineForPoint(origin, .1, out line) && TryMoveBackwardOnPath(line, origin, distance, out final);
		}

		Double GetDistanceForTwoOrderedPoints(GeoLine l1, GeoPoint p1, GeoLine l2, GeoPoint p2)
		{
			Double distance = 0;

			if(l1.Equals(l2))
			{
				distance = EarthGeo.GetDistance(p1, p2);
			}
			else
			{
				int index;

				for(index = 0; index < Count && this[index].Equals(l1) == false; index++) ;
				if(index < Count)
				{
					distance += EarthGeo.GetDistance(p1 as GeoPoint, l1.P2 as GeoPoint);

					for(; this[index].Equals(l2) == false; index++)
					{
						distance += this[index].Length;
					}
					if(index < Count)
					{
						distance += EarthGeo.GetDistance(p2 as GeoPoint, l2.P1 as GeoPoint);
					}
				}
			}

			return distance;
		}

		bool TryMoveForwardOnPath(GeoLine originLine, GeoPoint origin, Double distance, out GeoPoint final)
		{
			final = null;
			Double distanceMoved = EarthGeo.GetDistance(origin as GeoPoint, originLine.P2 as GeoPoint);
			if(distanceMoved <= distance)
			{
				for(int index = IndexOf(originLine) + 1; index < Count; index++)
				{
					if(distanceMoved + this[index].Length >= distance)
					{
						final = EarthGeo.GetPoint(this[index].P1 as GeoPoint, this[index].Bearing, distance - distanceMoved);
						break;
					}
					else
					{
						distanceMoved += this[index].Length;
					}
				}
			}
			else
			{
				/** the destination is on the first segment */
				final = EarthGeo.GetPoint(origin, originLine.Bearing, distance);
			}
			return final != null;
		}

		bool TryMoveBackwardOnPath(GeoLine originLine, GeoPoint origin, Double distance, out GeoPoint final)
		{
			final = null;
			Double distanceMoved = EarthGeo.GetDistance(origin as GeoPoint, originLine.P1 as GeoPoint);
			if(distanceMoved <= distance)
			{
				for(int index = IndexOf(originLine) - 1; index >= 0; index--)
				{
					if(distanceMoved + this[index].Length >= distance)
					{
						final = EarthGeo.GetPoint(this[index].P2 as GeoPoint, Angle.Reverse(this[index].Bearing), distance - distanceMoved);
						break;
					}
					else
					{
						distanceMoved += this[index].Length;
					}
				}
			}
			else
			{
				/** the destination is on the first segment */
				final = EarthGeo.GetPoint(origin, Angle.Reverse(originLine.Bearing), distance);
			}
			return final != null;
		}

		public bool TryGetLineForPoint(GeoPoint point, Double precision, out GeoLine line)
		{
			line = null;
			Double shortest = Double.MaxValue;

			foreach(GeoLine l in this)
			{
				Double		distance;
				GeoPoint	closest = l.ClosestPointTo(point, out distance);
				if(distance <= precision)
				{
					if(distance < shortest)
					{
						line = l;
						shortest = distance;
					}
				}
			}
			return line != null;
		}
#if zero
		public Double GetDistanceBetweenTwoPoints(GeoPoint p1, GeoPoint p2)
		{
			GeoSpanningTree spanningTree = new GeoSpanningTree(this);
			spanningTree.Origin = p1;
			spanningTree.Destination = p2;
			GeoPath			path = spanningTree.ComputePath();
			return path.Length;
		}
#endif
#endregion

#region Utility

		public Unescaped ToUnescapedSqlString()
		{
			return Unescaped.String(string.Format("GeomFromText('LINESTRING({0})')", String.Join(",", Array.ConvertAll<GeoPoint, String>(this.GeoPoints.ToArray(), p => p.X + " " + p.Y))));
		}

#endregion
	}
}
