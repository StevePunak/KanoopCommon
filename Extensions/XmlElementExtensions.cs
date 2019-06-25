using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using KanoopCommon.Addresses;
using KanoopCommon.CommonObjects;
using KanoopCommon.Types;

namespace KanoopCommon.Extensions
{
	public static class XmlElementExtensions
	{
		#region Add Sub-Node

		public static XmlElement AddSubNode(this XmlElement element, String name, String value)
		{
			XmlElement subNode = element.OwnerDocument.CreateElement(name);
			subNode.InnerText = value;
			element.AppendChild(subNode);

			return subNode;
		}

		public static XmlElement AddSubNode(this XmlElement element, String name)
		{
			XmlElement subNode = element.OwnerDocument.CreateElement(name);
			element.AppendChild(subNode);

			return subNode;
		}

		public static XmlElement AddSubNode(this XmlElement element, String name, UUID value)
		{
			return AddSubNode(element, name, value.ToString());
		}

		public static XmlElement AddSubNode(this XmlElement element, String name, int value)
		{
			return AddSubNode(element, name, value.ToString());
		}

		public static XmlElement AddSubNode(this XmlElement element, String name, uint value)
		{
			return AddSubNode(element, name, value.ToString());
		}

		#endregion

		#region Conversions

		public static String ToString(this XmlElement element, String name)
		{
			return element[name].InnerText;
		}

		public static UInt32 ToUInt32(this XmlElement element, String name)
		{
			return UInt32.Parse(element[name].InnerText);
		}
	
		public static Int32 ToInt32(this XmlElement element, String name)
		{
			return Int32.Parse(element[name].InnerText);
		}

		#endregion
	}
}
