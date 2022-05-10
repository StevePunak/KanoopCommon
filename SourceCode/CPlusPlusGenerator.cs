using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanoopCommon.SourceCode
{
	public class CPlusPlusGenerator : SourceGenerator
	{
		public override bool GenerateEnum(string name, Set values)
		{
			BeginEnum(name);
			foreach(var value in values)
			{
				if(value.Value != null)
					IndentWrite($"{value.Key} = {value.Value},");
				else
					IndentWrite($"{value.Key},");
				NewLine();
			}
			EndEnum();
			return true;
		}

		private void BeginEnum(String name)
		{
			IndentWrite($"enum {name}");
			NewLine();
			OpenCurlyBrace();
		}

		private void EndEnum()
		{
			CloseCurlyBrace();
			NewLine();
		}
	}
}
