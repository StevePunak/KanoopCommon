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
	public static class XmlNodeExtensions
	{
		#region Conversions

		public static bool TryGetAttribute(this XmlNode node, String attributeName, out String value)
		{
			bool result = false;
			value = null;

			if(node.Attributes != null && node.Attributes[attributeName] != null)
			{
				value = node.Attributes[attributeName].Value;
				result = true;
			}
			return result;
		}

		public static void AddAttribute(this XmlNode node, String name, String value)
		{
//			XmlAttribute attribute2 = node.OwnerDocument.CreateAttribute(name);
//			attribute2.InnerText = value;
			((XmlElement)node).SetAttribute(name, value);
		}

		public static void AddSubNode(this XmlNode node, XmlNode subNode)
		{
			node.AppendChild(subNode);
		}

		public static String ToString(this XmlNode element, String name)
		{
			return element[name].InnerText;
		}

		public static UUID ToUUID(this XmlNode element, String name)
		{
			return new UUID(element[name].InnerText);
		}
	
		public static UInt32 ToUInt32(this XmlNode element, String name)
		{
			return UInt32.Parse(element[name].InnerText);
		}
	
		public static Int32 ToInt32(this XmlNode element, String name)
		{
			return Int32.Parse(element[name].InnerText);
		}

		#endregion
	}
}
