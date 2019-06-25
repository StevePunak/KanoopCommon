using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KanoopCommon.Extensions
{
	public static class TextBoxExtensions
	{
		public static int CountLines(this TextBox textBox)
		{
			/** count lines */
			int lineCount = 0;
			for(int x = 0; ;x++)
			{
				int start = textBox.GetFirstCharIndexFromLine(x);
				if(start < 0)
					break;

				int end = textBox.GetFirstCharIndexFromLine(x + 1);
				if(end == -1 || start == end)
				{
					end = textBox.Text.Length;
				}

				lineCount++;
			}
			return lineCount;
		}
	}
}
