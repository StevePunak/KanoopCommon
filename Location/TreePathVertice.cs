using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KanoopCommon.Geometry;

namespace KanoopCommon.LocationAlgorithms
{
	public class TreePathVertice
	{
		public enum VerticeType
		{
			Standard,
			AdHocOrigin,
			AdHocDestination,
			AdHocOriginProximal,
			AdHocDestinationProximal,
		}

		public enum VerticeState
		{
			Unvisited,
			Visited
		}

		PointD		m_Position;
		public PointD Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		Double		m_nDistance;
		public Double Distance
		{
			get { return m_nDistance; }
			set { m_nDistance = value; }
		}

		VerticeState m_State;
		public VerticeState State
		{
			get { return m_State; }
			set { m_State = value; }
		}

		VerticeType m_Type;
		public VerticeType Type
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		TreePathVertice		m_Source;
		public TreePathVertice Source
		{
			get { return m_Source; }
			set { m_Source = value; }
		}

		Dictionary<String, TreePathVertice>		m_Neighbors;
		public Dictionary<String, TreePathVertice> Neighbors
		{
			get { return m_Neighbors; }
		}

		public String Name { get; set; }

		public String HashName { get { return m_Position.HashName; } }

		public TreePathVertice(PointD position, VerticeType type)
		{
			m_Position = position;
			m_nDistance = Double.MaxValue;
			m_Neighbors = new Dictionary<string,TreePathVertice>();
			Name = String.IsNullOrEmpty(position.Name) ? m_Position.ToString() : position.HashName;
			m_Type = type;
		}

		public void TryAddNeighbor(TreePathVertice neighbor)
		{
			if(m_Neighbors.ContainsKey(neighbor.HashName) == false)
			{
				m_Neighbors.Add(neighbor.HashName, neighbor);
			}
		}

		public void TryReplaceNeighbor(TreePathVertice oldNeighbor, TreePathVertice newNeighbor)
		{
			if(m_Neighbors.ContainsKey(oldNeighbor.HashName))
			{
				m_Neighbors.Remove(oldNeighbor.HashName);
			}
			TryAddNeighbor(newNeighbor);
		}

		public override string ToString()
		{
			return Name;
		}

	}
}
