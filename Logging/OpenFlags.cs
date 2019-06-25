using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace KanoopCommon.Logging
{
	public class OpenFlags
	{
		public const uint	OUTPUT_TO_FILE = 0x00000001;						// cause output to file
		public const uint	OUTPUT_TO_NETWORK =	0x00000002;						// cause output to network
		public const uint	OUTPUT_TO_CONSOLE =	0x00000004;						// cause output to stdout
		public const uint	OUTPUT_TO_ASYNCH = 0x00000008;						// generate output on separate task
		public const uint	OUTPUT_TO_DEBUG = 0x00000010;						// generate output to system debug
		public const uint	OUTPUT_TO_MEMORY = 0x00000020;						// generate output to memory log

		/**
		 * Content Flags (masked w/0x0000FF00)
		 */
		public const uint	CONTENT_NOTHING = 0;
		public const uint	CONTENT_NOHEX =	0x00000100;							// disallow hex trace
		public const uint	CONTENT_LINE_NUMBERS = 0x00000200;					// print line numbers
		public const uint	CONTENT_TIMESTAMP =	0x00000400;						// print timestamp
		public const uint	CONTENT_PRINT_LEVEL = 0x00000800;					// print log level
		public const uint	CONTENT_DATESTAMP =	0x00001000;						// print date
		public const uint	CONTENT_THREAD_NAME = 0x00002000;                   // log thread ID

		public const uint	COMBO_VERBOSE = ( CONTENT_TIMESTAMP | CONTENT_PRINT_LEVEL | CONTENT_DATESTAMP);
	}
}
