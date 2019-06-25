using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Database
{
	public class QueryString
	{
		protected String		_value;
		public String Value { get { return _value; } }

		protected bool			_trusted;
		public bool Trusted { get { return _trusted; } }

		protected QueryString(bool trusted)
			: this(trusted, String.Empty) {}

		public QueryString(String other)
		{
			_trusted = false;
			_value = other;
		}

		protected QueryString(bool trusted, String other)
		{
			_trusted = trusted;
			_value = other;
		}

		protected QueryString(bool trusted, String format, params object[] args)
		{
			_trusted = trusted;
			_value = Format(format, args);
		}

		public static QueryString operator +(QueryString s1, String s2)
		{
			s1.AppendString(s2);
			return s1;
		}
		public static QueryString operator +(QueryString s1, QueryString s2)
		{
			s1.AppendString(s2.ToString());
			return s1;
		}
		
		public String Format(String format, params object[] parms)
		{
			String formatted = null;
			
			if(parms == null || parms.Length == 0)
			{
				/** if there are no parms, just use the format entry */
				formatted = format;
			}
			else if(_trusted)
			{
				/** if this is a trusted query string, use stock format */
				formatted = String.Format(format, parms);
			}
			else
			{
				/** otherwise, escape each string in the parms list */
				List<Object> newParms = new List<Object>();
				foreach(Object element in parms)
				{
					if (element is ISQLUnescapeable)
						newParms.Add(((ISQLUnescapeable)element).ToSQLString());
					else if (element is ISQLUnescaped)
						newParms.Add(((ISQLUnescaped)element).ToSQLString());
					else if(element is String)
						newParms.Add(EscapeString(element as String));
					else
						newParms.Add(element);
				}
				formatted = String.Format(format, newParms.ToArray());
			}
			return formatted;
		}

		protected virtual QueryString Create(bool trusted, String other)
		{
			return new QueryString(trusted, other);
		}

		protected virtual String EscapeString(String unescaped)
		{
			return unescaped;
		}

		protected virtual void AppendString(String value)
		{
			_value += value;
		}

		public bool Contains(String value)
		{
			return _value.Contains(value);
		}

		public QueryString TrimEnd(char ch)
		{
			return Create(Trusted, _value.Trim(ch));
		}

		public QueryString Replace(String oldValue, String newValue)
		{
			return Create(Trusted, _value.Replace(oldValue, newValue));
		}

		public override String ToString()
		{
			return _value;
		}
	}
}
