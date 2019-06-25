using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KanoopCommon.CommonObjects;
using KanoopCommon.Database;

namespace KanoopCommon.Extensions
{
	public static class ListViewExtensions
	{
		public class ListViewRow
		{
			public Object Name { get; private set; }

			public Object FirstColumnText { get; private set; }

			public List<Object> Columns { get; private set; }

			public Object Tag { get; private set; }

			public ListViewRow(Object name, Object firstColumnText, Object tag)
				: this(name, firstColumnText, new List<Object>(), tag) {}

			public ListViewRow(Object name, Object firstColumnText, List<Object> columns, Object tag)
			{
				Name = name;
				FirstColumnText = firstColumnText;
				Columns = columns;
				Tag = tag;
			}
		}

		public static void AddColumnHeader(this ListView listView, String name, int width)
		{
			ColumnHeader header;
			header = new ColumnHeader(name);
			header.Width = width;
			header.Name = header.Text = name;
			if (listView.InvokeRequired)
			{ 
				listView.BeginInvoke(new MethodInvoker(() => listView.Columns.Add(header)));
			}
			else
			{ 
				listView.Columns.Add(header);
			}
		}


		public static void RemoveRow(this ListView listView, Object name)
		{
			listView.Items.RemoveByKey(name.ToString());
		}

		public static ListViewItem AddRow(this ListView listView, Object name, Object firstColText, Object tag)
		{
			return AddRows(listView, new List<ListViewRow>() { new ListViewRow(name, firstColText, tag) })[0];
		}

		public static ListViewItem AddRow(this ListView listView, Object name, Object firstColText, Object tag, bool check)
		{
			ListViewItem item = AddRows(listView, new List<ListViewRow>() { new ListViewRow(name, firstColText, tag) })[0];
			item.Checked = check;
			return item;
		}

		public static List<ListViewItem> AddRows(this ListView listView, List<ListViewRow> rows)
		{
			List<ListViewItem> items = new List<ListViewItem>();

			foreach(ListViewRow row in rows)
			{
				ListViewItem item = new ListViewItem();
				item.Text = row.FirstColumnText.ToString();
				item.Name = row.Name.ToString();
				item.Tag = row.Tag;


				for(int x = 1;x < listView.Columns.Count;x++)
				{
					ColumnHeader header = listView.Columns[x];
					ListViewItem.ListViewSubItem subItem;

					subItem = new ListViewItem.ListViewSubItem(item, row.Columns.Count > x ? row.Columns[x].ToString() : String.Empty);
					subItem.Name = header.Text;
					item.SubItems.Add(subItem);
				}

				items.Add(item);
			}

			listView.Items.AddRange(items.ToArray());

			return items;
		}


		public static bool Contains(this ListView listView, String value)
		{
			bool result = false;
			foreach(ListViewItem item in listView.Items)
			{
				if(item.SubItems[0].Text == value)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static void SetListViewColumn(this ListView listView, ListViewItem item, String column, Object value)
		{
			if(item.SubItems.ContainsKey(column) == true)
			{
				if(item.SubItems[column].Text != value.ToString())
					item.SubItems[column].Text = value.ToString();
			}
			else if(listView.Columns.ContainsKey(column))
			{
				int index = listView.Columns.IndexOfKey(column);
				if(item.SubItems[index].Text != value.ToString())
					item.SubItems[index].Text = value.ToString();
			}
			else
			{
				throw new CommonException("Invalid column name {0}", column);
			}
		}

		public static ListViewItem.ListViewSubItem SetListViewColumn(this ListView listView, ListViewItem item, int index, Object value)
		{
			ListViewItem.ListViewSubItem subItem = item.SubItems[index];
			if(subItem.Text != value.ToString())
				subItem.Text = value.ToString();
			return subItem;
		}

		public static bool TryGetItem(this ListView listView, Object tag, out ListViewItem item)
		{
			item = null;
			foreach(ListViewItem lvi in listView.Items)
			{
				if(lvi.Tag.Equals(tag))
				{
					item = lvi;
				}
			}
			return item != null;
		}

		public static bool TryGetItemWithTagType(this ListView listView, Type tagType, out ListViewItem item)
		{
			item = null;
			foreach(ListViewItem lvi in listView.Items)
			{
				if(lvi.Tag.GetType() == tagType)
				{
					item = lvi;
				}
			}
			return item != null;
		}

		public static bool TryGetItem(this ListView listView, String firstColumnText, out ListViewItem item)
		{
			item = null;
			foreach(ListViewItem lvi in listView.Items)
			{
				if(lvi.SubItems[0].Text.Equals(firstColumnText))
				{
					item = lvi;
					break;
				}
			}
			return item != null;
		}

		public static List<T> GetTagObjects<T>(this ListView listView, bool selectedOnly = false)
		{
			List<T> items = new List<T>();

			foreach(ListViewItem item in listView.Items)
			{
				if(	(item.Tag != null && item.Tag is T) &&
					(selectedOnly == false || item.Selected))
				{
					items.Add((T)item.Tag);
				}
			}
			return items;
		}

		public static Dictionary<String, int> GetColumnWidths(this ListView listView)
		{
			Dictionary<String, int> widths = new Dictionary<String, int>();
			foreach(ColumnHeader header in listView.Columns)
			{
				widths.Add(header.Text, header.Width);
			}
			return widths;
		}

		public static int WidthOf(this ListView listView, Dictionary<String, int> widths, String name)
		{
			int width;
			if(widths.TryGetValue(name, out width) == false)
			{
				width = 50;
			}
			return width;
		}

		public static String ColumnName(this ListView listView, int index)
		{
			return listView.Columns[index].Name;
		}

		public static int ColumnIndex(this ListView listView, String name)
		{
			return listView.Columns[name].Index;
		}

		public static String ToTabbedString(this ListView listView)
		{
			StringBuilder sb = new StringBuilder();

			foreach(ColumnHeader header in listView.Columns)
			{
				sb.AppendFormat("{0}\t", header.Text);
			}
			sb = new StringBuilder(sb.ToString().TrimEnd() + "\n");

			foreach(ListViewItem item in listView.Items)
			{
				foreach(ListViewItem.ListViewSubItem subItem in item.SubItems)
				{
					sb.AppendFormat("{0}\t", subItem.Text);
				}
				sb = new StringBuilder(sb.ToString().TrimEnd() + "\n");
			}
			return sb.ToString();
		}

		public static void ToCSVFile(this ListView listView, String fileName)
		{
			CSVFile file = new CSVFile(fileName);

			List<String> columnNames = new List<String>();

			foreach(ColumnHeader header in listView.Columns)
			{
				columnNames.Add(header.Text);
			}
			file.Columns = columnNames;

			foreach(ListViewItem item in listView.Items)
			{
				List<String> items = new List<String>();
				foreach(ListViewItem.ListViewSubItem subItem in item.SubItems)
				{
					items.Add(subItem.Text);
				}
				file.AddLine(items);
			}

			file.SaveOutput();
		}

		public static void SetColor(this ListViewItem item, ColorPair color)
		{
			item.ForeColor = color.ForeColor;
			item.BackColor = color.BackColor;
		}

		public static ColorPair GetColor(this ListViewItem item)
		{
			return new ColorPair(item.ForeColor, item.BackColor);
		}

	}

	public class ListViewItemComparer : IComparer
	{

		private int col;
		private SortOrder order;
		public ListViewItemComparer()
		{
			col = 0;
			order = SortOrder.Ascending;
		}
		public ListViewItemComparer(int column, SortOrder order)
		{
			col = column;
			this.order = order;
		}
		public int Compare(object x, object y)
		{
			int returnVal= -1;
			returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
							((ListViewItem)y).SubItems[col].Text);
			// Determine whether the sort order is descending.
			if(order == SortOrder.Descending)
				// Invert the value returned by String.Compare.
				returnVal *= -1;
			return returnVal;
		}


	}
}
