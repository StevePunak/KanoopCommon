using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

		List<Dictionary<String, String>>		m_Table;
		public List<Dictionary<String, String>> Table 
		{ 
			get 
			{ 
				if(m_Table == null)
				{
					m_Table = new List<Dictionary<String, String>>();
				}
				return m_Table; 
			} 
		}

		List<String>							m_Columns;
		public List<String> Columns 
		{ 
			get 
			{
				if(m_Columns == null)
				{
					m_Columns = new List<String>();
				}
				return m_Columns; 
			} 
			set { m_Columns = value; }
		}

		String									filename;
		public String Filename { get { return filename; }  set { filename = value; } }

		public Dictionary<String, String> EmptyRow
		{
			get
			{
				Dictionary<String,String> row = new Dictionary<String,String>();
				foreach(String column in Columns)
				{
					row.Add(column, String.Empty);
				}
				return row;
			}
		}

		#endregion

		#region Private Member Variables

		List<String>							m_Output;

		#endregion

		#region Constructor(s)

		public CSVFile()
			: this("") {}

		public CSVFile(String fileName)
		{
			filename = fileName;
			m_Output = new List<String>	();
		}

		#endregion

		#region Public Access Methods

		public void SetColumnNames(params String[] names)
		{
			Columns = new List<String>(names);
		}

		public void SetColumnNames(List<String> names)
		{
			Columns = new List<String>(names);
		}

		public void AddLine(params object[] parms)
		{
			List<String> parts =  new List<String>();
			foreach(Object item in parms)
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
			if(parts.Count != Columns.Count)
			{
				throw new Exception("Input must match column count");
			}

			Dictionary<String,String> tableLine = new Dictionary<String, String>();

			StringBuilder strOut = new StringBuilder();
			for(int x = 0;x < parts.Count;x++)
			{
				strOut.AppendFormat("\x22{0}\x22", EscapedString(parts[x]));
				if(x != parts.Count - 1)
				{
					strOut.Append(',');
				}
				tableLine.Add(m_Columns[x], parts[x]);
			}
			Table.Add(tableLine);
			m_Output.Add(strOut.ToString());
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

			if(m_Table != null)
			{
				foreach(Dictionary<String, String> row in m_Table)
				{
					int nCol = 0;
					foreach(String col in row.Values)
					{
						String field = EscapedString(col);
						output.AppendFormat("\x22{0}\x22", field);
						if(++nCol < row.Values.Count)
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
			if(!File.Exists(filename))
			{
				throw new Exception(String.Format("File {0} does not exist", filename));
			}

			TextReader tr = new StreamReader(filename);
			ImportFromTextReader(tr);
			tr.Close();
		}

		public void Import(String str)
		{
			TextReader tr = new StringReader(str);
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
			foreach(char inchar in strIn)
			{
				if(m_EscapeList.Contains(inchar))
					sb.Append(ESC_CHAR);
				sb.Append(inchar);
			}
			return sb.ToString();
		}

		String UnescapedString(String strIn)
		{
			StringBuilder sb = new StringBuilder(strIn.Length);
			for(int x = 0;x < strIn.Length;x++)
			{
				if(strIn[x] == ESC_CHAR)
					++x;
				sb.Append(strIn[x]);
			}
			return sb.ToString();
		}

		String[] SplitParts(String strLine)
		{
			List<String> parts = new List<String>();
			bool bDelimited = false;
			Char delimiter = (char)0;

			SplitStates state = SplitStates.Data;

			if(strLine[0] == 0x22 || strLine[0] == 0x27)
			{
				bDelimited = true;
				delimiter = strLine[0];
				
			}
	

			int inOffset = 0;
			while(parts.Count < this.Columns.Count && inOffset < strLine.Length)
			{
				/** do a word */
				StringBuilder sb = new StringBuilder();
				state = bDelimited ? SplitStates.OpenDelim : SplitStates.Data;

				for(;inOffset < strLine.Length && state != SplitStates.CloseDelim;inOffset++)
				{
					switch(state)
					{
						case SplitStates.OpenDelim:
							if(strLine[inOffset] != delimiter)
							{
								throw new Exception("No open delimiter found");
							}
							state = SplitStates.Data;
							break;

						case SplitStates.Data:
							if(strLine[inOffset] == ESC_CHAR)
							{
								state = SplitStates.Escape;
							}
							else if(strLine[inOffset] == delimiter)
							{
								state = SplitStates.CloseDelim;
							}
							else if(bDelimited == false && strLine[inOffset] == ',')
							{
								state = SplitStates.CloseDelim;
								inOffset--;		// decrement because it will be re-incremented in the loop
							}
							else
							{
								sb.Append(strLine[inOffset]);
							}
							break;

						case SplitStates.Escape:
							sb.Append(strLine[inOffset]);
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
			m_Columns = new List<string>();

			String[] parts = strHeader.Split(',');
			foreach(String part in parts)
			{
				m_Columns.Add(part.Trim());
			}
		}

		#endregion

		#region Importation

		void ImportFromTextReader(TextReader tr)
		{
			String strHeaderLine;
			if((strHeaderLine = tr.ReadLine()) == null)
			{
				throw new Exception(String.Format("No header found in {0}", filename));
			}
			/** unless coluns have been specifically added, parse the first line as ID */
			if(Columns.Count == 0)
			{
				ParseHeader(strHeaderLine);
			}

			m_Table = new List<Dictionary<String, String>>();
			String strLine;
			while((strLine = tr.ReadLine()) != null)
			{
				String[] parts = SplitParts(strLine);
				Dictionary<String, String> row = new Dictionary<String, String>();
				for(int x = 0;x < parts.Length;x++)
				{
					String strValue = UnescapedString(parts[x]);

					row.Add(m_Columns[x], strValue);
				}
				m_Table.Add(row);
			}
		}

		#endregion

	}
}
