using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linux
{
	public class LineSplitter
	{
		public static List<LinePart> Split(String original)
		{
			int index = 0;
			List<LinePart> list = new List<LinePart>();

			do
			{
				/** get to first non-whitespace */
				while(index < original.Length && Char.IsWhiteSpace(original[index]))
					index++;

				if(index < original.Length)
				{
					int startIndex = index;

					/** get to next whitespace */
					while(index < original.Length && Char.IsWhiteSpace(original[index]) == false)
						index++;

					String s = original.Substring(startIndex, index - startIndex);
					LinePart part = new LinePart(original, s, startIndex);

					list.Add(part);
				}
			}while(index < original.Length);
	
			return list;
		}

	}

	public class LinePart
	{
		public int Index { get; private set; }

		public String Text { get; private set; }

		public String Original { get; private set; }

		public int Length { get { return Text.Length; } }

		public LinePart(String original, String text, int index)
		{
			Original = original;
			Text = text;
			Index = index;
		}

		public override string ToString()
		{
			return String.Format("'{0}' @ {1}", Text, Index);
		}
	}
}
