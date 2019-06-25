using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using KanoopCommon.Addresses;

namespace KanoopCommon.Extensions
{
	public static class XmlDocumentExtensions
	{
		#region Add Sub-Node

		public static XmlNode AddSubNode(this XmlDocument doc, String name, String value)
		{
			XmlElement subNode = doc.CreateElement(name);
			subNode.InnerText = value;
			doc.DocumentElement.AppendChild(subNode);

			return subNode;
		}

		public static XmlNode AddSubNode(this XmlDocument doc, String name)
		{
			XmlElement subNode = doc.CreateElement(name);
			doc.DocumentElement.AppendChild(subNode);

			return subNode;
		}

		public static XmlNode AddSubNode(this XmlDocument doc, String name, UUID value)
		{
			return AddSubNode(doc, name, value.ToString());
		}

		public static void AddSubNode(this XmlDocument doc, XmlNode subNode)
		{
			doc.DocumentElement.AppendChild(subNode);
		}

		public static XmlNode AddSubNode(this XmlDocument doc, String name, int value)
		{
			return AddSubNode(doc, name, value.ToString());
		}

		public static XmlNode AddSubNode(this XmlDocument doc, String name, uint value)
		{
			return AddSubNode(doc, name, value.ToString());
		}

		#endregion
	}
}
