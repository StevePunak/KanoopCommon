using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.SourceCode
{
	public abstract class SourceGenerator
	{
		public class Set : Dictionary<String, Object> { }

		public abstract bool GenerateEnum(String name, Set values);

		private int _indentLevel;

		protected int IndentLevel 
		{
			get { return _indentLevel; }
			set { if(_indentLevel >= 0) _indentLevel = value; }
		}

		public int IndentSize { get; set; }
		protected StringWriter Writer { get; set; }

		protected SourceGenerator()
		{
			Writer = new StringWriter();
			IndentLevel = 0;
			IndentSize = 4;
		}

		protected void IndentWrite(String value)
		{
			for(int i = 0;i < IndentLevel * IndentSize;i++)
				Writer.Write(' ');
			Writer.Write(value);
		}

		protected void NewLine()
		{
			Writer.Write("\n");
		}

		protected void OpenCurlyBrace()
		{
			IndentWrite("{");
			NewLine();
			IndentLevel++;
		}

		protected void CloseCurlyBrace()
		{
			IndentLevel--;
			IndentWrite("};");
			NewLine();
		}

		public override string ToString()
		{
			return Writer.ToString();
		}
	}
}
