using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Extensions
{
    public static class StringBuilderExtensions
    {
        private readonly static char[] m_CrLf;

		const char CR = '\n';

        static StringBuilderExtensions()
        {
            m_CrLf = "\r\n".ToArray();
        }
        public static void AppendFormatWithCrLf(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(m_CrLf);
        }

        public static void AppendFormatWithCrLf(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args);
            builder.Append(m_CrLf);
        }

        public static void AppendWithCrLf(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(m_CrLf);
        }

        public static void AppendWithCrLf(this StringBuilder builder)
        {
            builder.Append(m_CrLf);
        }

        public static void AppendFormatWithCr(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(CR);
        }

        public static void AppendFormatWithCr(this StringBuilder builder, string format, params object[] args)
        {
			if(args.Length > 0)
			{
				builder.AppendFormat(format, args);
			}
			else
			{
				builder.Append(format);
			}
            builder.Append(CR);
        }

        public static void AppendWithCr(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(CR);
        }

        public static void AppendWithCr(this StringBuilder builder)
        {
            builder.Append(CR);
        }

		public static void AppendLineColumns(this StringBuilder builder, List<Int32> widths, params object[] args)
        {
            if(widths.Count != args.Length)
			{
				throw new ArgumentException("Number of columns must match the number of arguments");
			}
			for(int x = 0;x < widths.Count;x++)
			{
				builder.Append(args[x] != null ? args[x].ToString().PadToLength(widths[x]) : "null");
			}
			builder.Append(m_CrLf);
        }
    }
}
