using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KanoopCommon.Extensions
{

	public class TreeNodeList : List<TreeNode>
	{
		public TreeNodeList()
			: base() { }

		public TreeNodeList(TreeNodeCollection nodes)
		{
			foreach(TreeNode node in nodes)
			{
				Add(node);
			}
		}

		public void AddRange(TreeNodeCollection nodes)
		{
			foreach(TreeNode node in nodes)
			{
				Add(node);
			}
		}
	}

	public static class TreeViewExtensions
	{
		public static bool TryFindNextNodeInAllChildren(this TreeNode parent, TreeNode first, out TreeNode next)
		{
			bool foundFirst = false;
			next = null;

			foreach(TreeNode node in parent.Nodes)
			{
				if(foundFirst)
				{
					next = node;
					break;
				}

				if(node == first)
				{
					foundFirst = true;
				}
				else if(node.Nodes.Count > 0 && TryFindNextNodeInAllChildren(node, first, out next))
				{
					break;
				}
			}

			return next != null;

		}

		public static bool TryFindTextInAllChildren(this TreeNode parent, String search, out TreeNode found)
		{
			found = null;

			foreach(TreeNode node in parent.Nodes)
			{
				if(node.Text.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					found = node;
					break;
				}
				else if(node.Nodes.Count > 0 && TryFindTextInAllChildren(node, search, out found))
				{
					break;
				}
			}

			return found != null;

		}

		public static List<T> GetTagObjects<T>(this TreeView treeView, bool recurse = false, bool selectedOnly = false)
		{
			List<T> items = new List<T>();

			foreach(TreeNode node in treeView.Nodes)
			{
				if(selectedOnly == false || node.IsSelected)
				{
					GetTagObjects<T>(node, ref items, recurse);
				}
			}
			return items;
		}

		static void GetTagObjects<T>(TreeNode parentNode, ref List<T> list, bool recurse = false)
		{
			foreach(TreeNode node in parentNode.Nodes)
			{
				if(node.Tag != null && node.Tag is T)
				{
					list.Add((T)node.Tag);

					if(recurse && node.Nodes.Count > 0)
					{
						GetTagObjects<T>(node, ref list, recurse);
					}
				}
			}
		}
		public static List<TreeNode> GetTagNodes<T>(this TreeView treeView, bool recurse = false, bool selectedOnly = false)
		{
			List<TreeNode> items = new List<TreeNode>();

			foreach(TreeNode node in treeView.Nodes)
			{
				if(selectedOnly == false || node.IsSelected)
				{
					GetTagNodes<T>(node, ref items, recurse);
				}
			}
			return items;
		}

		static void GetTagNodes<T>(TreeNode parentNode, ref List<TreeNode> list, bool recurse = false)
		{
			foreach(TreeNode node in parentNode.Nodes)
			{
				Type t = node.Tag.GetType();
				if((node.Tag != null) && (node.Tag is T) || node.Tag.GetType().IsSubclassOf(typeof(T)))
				{
					list.Add(node);
				}

				if(recurse && node.Nodes.Count > 0)
				{
					GetTagNodes<T>(node, ref list, recurse);
				}

			}
		}

		public static List<TreeNode> GetTagNodes(this TreeView treeView, Type type, bool recurse = false, bool selectedOnly = false)
		{
			List<TreeNode> items = new List<TreeNode>();

			foreach(TreeNode node in treeView.Nodes)
			{
				if(selectedOnly == false || node.IsSelected)
				{
					GetTagNodes(node, type, ref items, recurse);
				}
			}
			return items;
		}

		static void GetTagNodes(TreeNode parentNode, Type type, ref List<TreeNode> list, bool recurse = false)
		{
			foreach(TreeNode node in parentNode.Nodes)
			{
				Type t = node.Tag.GetType().UnderlyingSystemType;
				if((node.Tag != null) && (node.Tag is Type) && ((Type)node.Tag == type || ((Type)node.Tag).IsSubclassOf(type)))
				{
					list.Add(node);
				}

				if(recurse && node.Nodes.Count > 0)
				{
					GetTagNodes(node, type, ref list, recurse);
				}

			}
		}
	}
}
