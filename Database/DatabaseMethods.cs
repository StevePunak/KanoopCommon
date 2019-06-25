using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace KanoopCommon.Database
{
	internal class DatabaseMethods
	{
		static List<String> m_SkipMethods;

		static DatabaseMethods()
		{
			/**
			 * put methods to skip in the stack trace here
			 */
			m_SkipMethods = new List<string>()
			{
				"Update",
				"Insert",
				"Query"
			};
		}

		public static MethodBase Method
		{
			get
			{
				MethodBase method = null;
				StackTrace st = new StackTrace(2);
				foreach(StackFrame frame in st.GetFrames())
				{
					method = frame.GetMethod();
					if(m_SkipMethods.Contains(method.Name) == false)
					{
						break;
					}
				}
				return method;
			}
		}

	}
}
