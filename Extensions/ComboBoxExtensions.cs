using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KanoopCommon.Extensions
{
	public static class ComboBoxExtensions
	{
		public static bool TryFindByText(this ComboBox comboBox, String text, out Object item)
		{
			item = null;

			foreach(Object i in comboBox.Items)
			{
				if(i.ToString() == text)
				{
					item = i;
					break;
				}
			}

			return item != null;
		}

		public static void AddEnums<T>(this ComboBox comboBox) where T : Enum
		{
			foreach(Enum e in Enum.GetValues(typeof(T)))
			{
				comboBox.Items.Add(e);
			}
			
		}
	}
}
