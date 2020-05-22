using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace KanoopCommon.Database
{
	public class CSVFile
	{
		#region Constants

		const char ESC_CHAR = '\\';

		static readonly List<Char> m_EscapeList = new List<Char>()
		{
			'\x22',
			'\x60',
			'\x27',
			'\x5c',
		};

		const char QUOTE = '\x22';

		#endregion

		#region Enumerations

		enum SplitStates
		{
			OpenDelim,
			Escape,
			Data,
			CloseDelim
		};

		#endregion

		#region Public Properties

		List<Dictionary<String, String>> _table;
		public List<Dictionary<String, String>> Table
		{
			get
			{
				if (_table == null)
				{
					_table = new List<Dictionary<String, String>>();
				}
				return _table;
			}
		}

		List<String> _columns;
		public List<String> Columns
		{
			get
			{
				if (_columns == null)
				{
					_columns = new List<String>();
				}
				return _columns;
			}
			set { _columns = value; }
		}

		String filename;
		public String Filename { get { return filename; } set { filename = value; } }

		public Dictionary<String, String> EmptyRow
		{
			get
			{
				Dictionary<String, String> row = new Dictionary<String, String>();
				foreach (String column in Columns)
				{
					row.Add(column, String.Empty);
				}
				return row;
			}
		}

		#endregion

		#region Private Member Variables

		List<String> _output;

		#endregion

		#region Constructor(s)

		public CSVFile()
			: this("") { }

		public CSVFile(String fileName)
		{
			filename = fileName;
			_output = new List<String>();
		}

		#endregion

		#region Public Access Methods

		public void SetColumnNames<T>()
		{
			List<String> names = new List<String>();

			foreach (PropertyInfo property in typeof(T).GetProperties())
			{
				ColumnNameAttribute attr = property.GetCustomAttribute<ColumnNameAttribute>();
				if (attr != null)
				{
					names.Add(attr.ActualName);
				}
			}

			SetColumnNames(names);
		}

		public void SetColumnNames(params String[] names)
		{
			Columns = new List<String>(names);
		}

		public void SetColumnNames(List<String> names)
		{
			Columns = new List<String>(names);
		}

		public void AddLine<T>(T item)
		{
			Dictionary<String, String> row = new Dictionary<String, String>();

			foreach (PropertyInfo property in typeof(T).GetProperties())
			{
				ColumnNameAttribute attr = property.GetCustomAttribute<ColumnNameAttribute>();
				if (attr != null)
				{
					Object thing = property.GetValue(item);
					if (thing == null)
					{
						thing = "";
					}
					row.Add(attr.ActualName, thing.ToString());
				}
			}

			AddLine(row);
		}

		public void AddLine(params object[] parms)
		{
			List<String> parts = new List<String>();
			foreach (Object item in parms)
			{
				parts.Add(item.ToString());
			}
			AddLine(parts);
		}

		public void AddLine(Dictionary<String, String> row)
		{
			AddLine(new List<String>(row.Values));
		}

		public void AddLine(List<String> parts)
		{
			if (parts.Count != Columns.Count)
			{
				throw new Exception("Input must match column count");
			}

			Dictionary<String, String> tableLine = new Dictionary<String, String>();

			StringBuilder output = new StringBuilder();
			for (int x = 0; x < parts.Count; x++)
			{
				output.AppendFormat("\x22{0}\x22", EscapedString(parts[x]));
				if (x != parts.Count - 1)
				{
					output.Append(',');
				}
				tableLine.Add(_columns[x], parts[x]);
			}
			Table.Add(tableLine);
			_output.Add(output.ToString());
		}

		public void SaveOutput()
		{
			TextWriter tw = new StreamWriter(filename);
			tw.Write(ToString());
			tw.Close();
		}

		public override String ToString()
		{
			StringBuilder output = new StringBuilder();
			for (int x = 0; x < Columns.Count; x++)
			{
				output.AppendFormat("{0}", Columns[x]);
				if (x < Columns.Count - 1)
				{
					output.Append(',');
				}
			}
			output.Append("\r\n");

			if (_table != null)
			{
				foreach (Dictionary<String, String> row in _table)
				{
					int nCol = 0;
					foreach (String col in row.Values)
					{
						String field = EscapedString(col);
						output.AppendFormat("\x22{0}\x22", field);
						if (++nCol < row.Values.Count)
						{
							output.Append(',');
						}
					}
					output.Append("\r\n");
				}
			}
			output.Remove(output.Length - 2, 2);

			return output.ToString();
		}

		public void Import()
		{
			if (!File.Exists(filename))
			{
				throw new Exception(String.Format("File {0} does not exist", filename));
			}

			TextReader tr = new StreamReader(filename);
			ImportFromTextReader(tr);
			tr.Close();
		}

		public void Import(String stringContainingCSVText)
		{
			TextReader tr = new StringReader(stringContainingCSVText);
			ImportFromTextReader(tr);
		}

		public void Import(TextReader tr)
		{
			ImportFromTextReader(tr);
		}

		#endregion

		#region String Parsing

		String EscapedString(String strIn)
		{
			StringBuilder sb = new StringBuilder(strIn.Length);
			foreach (char inchar in strIn)
			{
				if (m_EscapeList.Contains(inchar))
					sb.Append(ESC_CHAR);
				sb.Append(inchar);
			}
			return sb.ToString();
		}

		String UnescapedString(String strIn)
		{
			StringBuilder sb = new StringBuilder(strIn.Length);
			for (int x = 0; x < strIn.Length; x++)
			{
				if (strIn[x] == ESC_CHAR)
					++x;
				sb.Append(strIn[x]);
			}
			return sb.ToString();
		}

		String[] SplitParts(String origLine)
		{
			const char QUOTE = '\"';

			String line = origLine;

			List<Char> splitChars = new List<Char>() { ',' };
			List<String> parts = new List<String>();

			line = line.Trim();

			bool inQuotedText = false;
			bool lastWasMatch = false;
			int partStartIndex = 0;
			for (int x = 0; x < line.Length; x++)
			{
				if (line[x] == QUOTE)
				{
					if (inQuotedText)
					{
						parts.Add(line.Substring(partStartIndex, x - partStartIndex));
						x++;
						partStartIndex = x + 1;
						inQuotedText = false;
						lastWasMatch = true;
					}
					else
					{
						inQuotedText = true;
						partStartIndex = x + 1;
					}
				}
				else if (splitChars.Contains(line[x]) && inQuotedText == false)
				{
					//if(lastWasMatch == false)
					{
						parts.Add(line.Substring(partStartIndex, x - partStartIndex));
						partStartIndex = x + 1;
						lastWasMatch = true;
					}
				}
				else
				{
					lastWasMatch = false;
				}
			}

			if (lastWasMatch == false && partStartIndex < line.Length)
			{
				parts.Add(line.Substring(partStartIndex));
			}

			return parts.ToArray();
		}

		String[] OldSplitParts(String line)
		{
			List<String> parts = new List<String>();
			bool delimited = false;
			Char delimiter = (char)0;

			SplitStates state = SplitStates.Data;

			if (line[0] == 0x22 || line[0] == 0x27)
			{
				delimited = true;
				delimiter = line[0];

			}


			int inOffset = 0;
			while (parts.Count < this.Columns.Count && inOffset < line.Length)
			{
				/** do a word */
				StringBuilder sb = new StringBuilder();
				state = delimited ? SplitStates.OpenDelim : SplitStates.Data;

				for (; inOffset < line.Length && state != SplitStates.CloseDelim; inOffset++)
				{
					switch (state)
					{
						case SplitStates.OpenDelim:
							if (line[inOffset] != delimiter)
							{
								throw new Exception("No open delimiter found");
							}
							state = SplitStates.Data;
							break;

						case SplitStates.Data:
							if (line[inOffset] == ESC_CHAR)
							{
								state = SplitStates.Escape;
							}
							else if (line[inOffset] == delimiter)
							{
								state = SplitStates.CloseDelim;
							}
							else if (delimited == false && line[inOffset] == ',')
							{
								state = SplitStates.CloseDelim;
								inOffset--;     // decrement because it will be re-incremented in the loop
							}
							else
							{
								sb.Append(line[inOffset]);
							}
							break;

						case SplitStates.Escape:
							sb.Append(line[inOffset]);
							state = SplitStates.Data;
							break;

						case SplitStates.CloseDelim:
							break;

					}
				}

				parts.Add(sb.ToString());

				/** we should be on a comma, or at the end */
				inOffset++;

			}
			return parts.ToArray();
		}

		void ParseHeader(String strHeader)
		{
			_columns = new List<string>();

			String[] parts = strHeader.Split(',');
			foreach (String part in parts)
			{
				_columns.Add(part.Trim(new char[] { '\"', '\n', '\r' }).Trim());
			}
		}

		#endregion

		#region Importation

		void ImportFromTextReader(TextReader tr)
		{
			String headerLine;
			if ((headerLine = tr.ReadLine()) == null)
			{
				throw new Exception(String.Format("No header found in {0}", filename));
			}
			/** unless coluns have been specifically added, parse the first line as ID */
			if (Columns.Count == 0)
			{
				ParseHeader(headerLine);
			}

			_table = new List<Dictionary<String, String>>();
			String line;
			while ((line = tr.ReadLine()) != null)
			{
				String[] parts = SplitParts(line);
				Dictionary<String, String> row = new Dictionary<String, String>();
				for (int x = 0; x < parts.Length; x++)
				{
					String strValue = UnescapedString(parts[x]);

					row.Add(_columns[x], strValue);
				}
				_table.Add(row);
			}
		}

		private static bool TryGetUnquotedString(String input, int offset, out String outputString, out int newIndex)
		{
			outputString = String.Empty;
			int index = newIndex = offset;

			StringBuilder sb = new StringBuilder();
			while (index < input.Length && input[newIndex] != ',' && input[newIndex] != '\n')
			{
				sb.Append(input[newIndex++]);
			}
			outputString = sb.ToString();

			return true;
		}

		private static bool TryGetQuotedString(String input, int offset, out String outputString, out int newIndex)
		{
			outputString = String.Empty;
			int index = newIndex = offset;

			if (input[index] != QUOTE)
				return false;

			if (++index >= input.Length)
				return false;

			StringBuilder sb = new StringBuilder();
			char inChar;
			do
			{
				inChar = input[index];
				if (inChar != QUOTE && inChar >= 0x20)
					sb.Append(inChar);
			} while (inChar != QUOTE && ++index < input.Length);

			if (inChar == QUOTE)
			{
				newIndex = index;
				outputString = sb.ToString();
			}
			return inChar == QUOTE;
		}

		private static bool TryGetNextString(String input, int offset, out String outputString, out int newIndex)
		{
			if (input[offset] != QUOTE)
				return TryGetUnquotedString(input, offset, out outputString, out newIndex);
			else
				return TryGetQuotedString(input, offset, out outputString, out newIndex);
		}

		private static bool TryGetLine(String input, int start, out int newIndex, out List<String> columns)
		{
			columns = new List<string>();
			newIndex = 0;

			if (start >= input.Length - 1)
				return false;

			int index = start;
			bool done = false;
			String item;
			while (done == false && TryGetNextString(input, index, out item, out newIndex))
			{
				columns.Add(item);
				index = newIndex;
				// scan to next field
				for (++index; index < input.Length; index++)
				{
					if (input[index] == '\"')
					{
						break;
					}
					else if (input[index] == '\n')
					{
						newIndex = ++index;
						done = true;
					}
					else if (input[index] == ',')
					{

					}
					else
					{
						break;
					}
				}
			}

			return done && columns.Count > 0;
		}

		public static bool TryTransformMultiline(String inputFile, String outputFile)
		{
			String inputText = File.ReadAllText(inputFile);
			int index = 0;
			List<String> columns;
			CSVFile newcsv = null;

			if (TryGetLine(inputText, index, out index, out columns))
			{
				newcsv = new CSVFile(outputFile);
				newcsv.Columns = columns;

				int newIndex;
				while (TryGetLine(inputText, index, out newIndex, out columns))
				{
					Dictionary<String, String> row = new Dictionary<String, String>();
					for (int x = 0; x < newcsv.Columns.Count; x++)
					{
						row.Add(newcsv.Columns[x], columns.Count > x ? columns[x] : String.Empty);
					}
					newcsv.AddLine(row);

					index = newIndex;
					if (index >= inputText.Length)
						break;
				}

				newcsv.SaveOutput();
			}

			return newcsv != null;
		}

		#endregion

	}
}
