using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing.Imaging;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Types
{
	public class MimeTypes
	{
		public const String MTSTR_UNKNOWN =									"unknown";
		public const String MTSTR_X_WWW_FORM_URLENCODED = 					"application/x-www-form-urlencoded";				// Form Encoded Data XD
		public const String MTSTR_EDI_X12 = 								"application/EDI-X12"; 								// EDI X12 data; Defined in RFC 1767
		public const String MTSTR_EDIFACT = 								"application/EDIFACT"; 								// EDI EDIFACT data; Defined in RFC 1767
		public const String MTSTR_JAVASCRIPT = 								"application/javascript"; 							// JavaScript; Defined in RFC 4329 but not accepted in IE 8 or earlier
		public const String MTSTR_OCTET_STREAM = 							"application/octet-stream"; 						// Arbitrary binary data[4]. Generally speaking this type identifies files that are not associated with a specific application. Contrary to past assumptions by software packages such as Apache this is not a type that should be applied to unknown files. In such a case, a server or application should not indicate a content type, as it may be incorrect, but rather, should omit the type in order to allow the recipient to guess the type.[5]
		public const String MTSTR_OGG = 									"application/ogg"; 									// Ogg, a multimedia bitstream container format; Defined in RFC 5334
		public const String MTSTR_PDF = 									"application/pdf"; 									// Portable Document Format, PDF has been in use for document exchange on the Internet since 1993; Defined in RFC 3778
		public const String MTSTR_XHTML_XML = 								"application/xhtml+xml"; 							// XHTML; Defined by RFC 3236
		public const String MTSTR_XML_DTD = 								"application/xml-dtd"; 								// DTD files; Defined by RFC 3023
		public const String MTSTR_JSON = 									"application/json"; 								// JavaScript Object Notation JSON; Defined in RFC 4627
		public const String MTSTR_ZIP = 									"application/zip"; 									// ZIP archive files; Registered[6]
		public const String MTSTR_BASIC = 									"audio/basic"; 										// mulaw audio at 8KHz, 1 channel; Defined in RFC 2046
		public const String MTSTR_MP4 = 									"audio/mp4"; 										// MP4 audio
        public const String MTSTR_MP3 =                                     "audio/mp3"; 										// MP3 audio
        public const String MTSTR_MPEG =                                    "audio/mpeg"; 										// MP3 or other MPEG audio; Defined in RFC 3003
		public const String MTSTR_VORBIS = 									"audio/vorbis"; 									// Vorbis encoded audio; Defined in RFC 5215
		public const String MTSTR_X_MS_WMA = 								"audio/x-ms-wma"; 									// Windows Media Audio; Documented in Microsoft KB 288102
		public const String MTSTR_VND_RN_REALAUDIO = 						"audio/vnd.rn-realaudio"; 							// RealAudio; Documented in RealPlayer Customer Support Answer 2559
		public const String MTSTR_VND_WAVE = 								"audio/vnd.wave"; 									// WAV audio; Defined in RFC 2361
        public const String MTSTR_BMP =                                     "image/bmp"; 										// BMP image; 
        public const String MTSTR_GIF =                                     "image/gif"; 										// GIF image; Defined in RFC 2045 and RFC 2046
		public const String MTSTR_JPEG = 									"image/jpeg"; 										// JPEG JFIF image; Defined in RFC 2045 and RFC 2046
		public const String MTSTR_PNG = 									"image/png"; 										// Portable Network Graphics; Registered[7], Defined in RFC 2083
		public const String MTSTR_SVG_XML = 								"image/svg+xml"; 									// SVG vector image; Defined in RFC 3023
		public const String MTSTR_TIFF = 									"image/tiff"; 										// Tag Image File Format; Defined in RFC 3302
		public const String MTSTR_VND_MICROSOFT_ICON = 						"image/vnd.microsoft.icon"; 						// ICO image; Registered[8]
		public const String MTSTR_HTTP = 									"message/http";										// http
		public const String MTSTR_MIXED = 									"multipart/mixed"; 									// MIME E-mail; Defined in RFC 2045 and RFC 2046
		public const String MTSTR_ALTERNATIVE = 							"multipart/alternative"; 							// MIME E-mail; Defined in RFC 2045 and RFC 2046
		public const String MTSTR_RELATED = 								"multipart/related"; 								// MIME E-mail; Defined in RFC 2387 and used by MHTML (HTML mail)
		public const String MTSTR_FORM_DATA = 								"multipart/form-data"; 								// MIME Webform; Defined in RFC 2388
		public const String MTSTR_SIGNED = 									"multipart/signed"; 								// Defined in RFC 1847
		public const String MTSTR_ENCRYPTED = 								"multipart/encrypted"; 								// Defined in RFC 1847
		public const String MTSTR_CSS = 									"text/css"; 										// Cascading Style Sheets; Defined in RFC 2318
		public const String MTSTR_CSV = 									"text/csv"; 										// Comma-separated values; Defined in RFC 4180
		public const String MTSTR_HTML = 									"text/html"; 										// HTML; Defined in RFC 2854
		public const String MTSTR_PLAIN = 									"text/plain"; 										// Textual data; Defined in RFC 2046 and RFC 3676
		public const String MTSTR_XML = 									"text/xml"; 										// Extensible Markup Language; Defined in RFC 3023
		public const String MTSTR_QUICKTIME = 								"video/quicktime"; 									// QuickTime video; Registered[9]
		public const String MTSTR_X_MS_WMV = 								"video/x-ms-wmv"; 									// Windows Media Video; Documented in Microsoft KB 288102
		public const String MTSTR_X_MS_VIDEO =								"video/x-msvideo"; 								
		public const String MTSTR_VND_OASIS_OPENDOCUMENT_TEXT = 			"application/vnd.oasis.opendocument.text"; 			// OpenDocument Text; Registered [11]
		public const String MTSTR_VND_OASIS_OPENDOCUMENT_SPREADSHEET = 		"application/vnd.oasis.opendocument.spreadsheet"; 	// OpenDocument Spreadsheet; Registered [12]
		public const String MTSTR_VND_OASIS_OPENDOCUMENT_PRESENTATION = 	"application/vnd.oasis.opendocument.presentation"; 	// OpenDocument Presentation; Registered [13]
		public const String MTSTR_VND_OASIS_OPENDOCUMENT_GRAPHICS = 		"application/vnd.oasis.opendocument.graphics"; 		// OpenDocument Graphics; Registered [14]
		public const String MTSTR_VND_MS_EXCEL = 							"application/vnd.ms-excel"; 						// Microsoft Excel files
		public const String MTSTR_VND_MS_POWERPOINT = 						"application/vnd.ms-powerpoint"; 					// Microsoft Powerpoint files
		public const String MTSTR_MSWORD = 									"application/msword"; 								// Microsoft Word files
		public const String MTSTR_VND_MOZILLA_XUL_XML = 					"application/vnd.mozilla.xul+xml"; 					// Mozilla XUL files
		public const String MTSTR_X_DVI = 									"application/x-dvi"; 								// Digital Video files in DVI format
		public const String MTSTR_X_HTTPD_PHP = 							"application/x-httpd-php"; 							// PHP files
		public const String MTSTR_X_HTTPD_PHP_SOURCE = 						"application/x-httpd-php-source"; 					// PHP source files
		public const String MTSTR_X_LATEX = 								"application/x-latex"; 								// LaTeX files
		public const String MTSTR_X_SHOCKWAVE_FLASH = 						"application/x-shockwave-flash"; 					// Adobe Flash files; Documented in Adobe TechNote tn_4151 and Adobe TechNote tn_16509
		public const String MTSTR_X_STUFFIT = 								"application/x-stuffit"; 							// StuffIt archive files
		public const String MTSTR_X_RAR_COMPRESSED = 						"application/x-rar-compressed"; 					// RAR archive files
		public const String MTSTR_X_TAR = 									"application/x-tar"; 								// Tarball files
        public const String MTSTR_NMEA = 									"text/nmea"; 										// Extensible Markup Language; Defined in RFC 3023

        public const String MTEXT_UNKNOWN =                                 ".*"; 								                // Unknown
        public const String MTEXT_BMP =                                     ".bmp";                                             // BMP image; 
        public const String MTEXT_GIF =                                     ".gif";                                             // GIF image; Defined in RFC 2045 and RFC 2046
        public const String MTEXT_JPG =                                     ".jpg";                                             // JPEG JFIF image; Defined in RFC 2045 and RFC 2046
        public const String MTEXT_JPEG =                                    ".jpeg";                                            // JPEG JFIF image; Defined in RFC 2045 and RFC 2046
		public const String MTEXT_PNG =                                     ".png";                                             // Portable Network Graphics; Registered[7], Defined in RFC 2083
        public const String MTEXT_TIFF =                                    ".tif";                                             // Tag Image File Format; Defined in RFC 3302
        public const String MTEXT_HTM =                                     ".htm";                                             // HTML; Defined in RFC 2854
        public const String MTEXT_HTML =                                    ".html";                                            // HTML; Defined in RFC 2854
        public const String MTEXT_PLAIN =                                   ".txt";                                             // Textual data; Defined in RFC 2046 and RFC 3676
        public const String MTEXT_MP3 =                                     ".mp3";                                             // MP3 audio

		private static StringToEnum 			m_StringToEnum = new StringToEnum();
		private static EnumToString				m_EnumToString = new EnumToString();

        private static ExtensionToEnum          m_ExtToEnum = new ExtensionToEnum();
        private static EnumToExtension          m_EnumToExt = new EnumToExtension();


		static List<MimeType> m_GraphicTypes;
		static List<MimeType> m_TextTypes;

		static MimeTypes()
		{
			InitializeDictionary(typeof(MimeType));
			m_GraphicTypes = new List<MimeType>()
			{
				MimeType.BMP,
				MimeType.GIF,
				MimeType.JPEG,
				MimeType.PNG,
				MimeType.TIFF
			};

			m_TextTypes = new List<MimeType>()
			{
				MimeType.JSON,
				MimeType.NMEA,
				MimeType.PLAIN,
				MimeType.XML
			};
		}

		public static void GetEnumString(MimeType value, out String ret)
		{
			if(m_EnumToString.TryGetValue(value, out ret) == false)
			{
				throw new Exception("Can't match enum to String");
			}
		}

		public static bool TryParse(String value, out MimeType ret)
		{
			ret = MimeType.UNKNOWN;
			Enum tmp;
			String s = (value.IndexOf(';') > 0) 
				? value.Substring(0, value.IndexOf(';')) 
				: value;
			if(m_StringToEnum.TryGetValue(s, out tmp) == true)
			{
				ret = (MimeType)tmp;
			}
			return ret != MimeType.UNKNOWN;
		}

		public static bool TryGetInternetMediaType(MimeType value, out String ret)
		{
			return m_EnumToString.TryGetValue(value, out ret);
		}

		/// <summary>
		/// Version of TryGetEnumString which will always return a string
		/// </summary>
		/// <param name="value">MimeType</param>
		/// <returns>Internet Media Type of give enumeration value, or String.Empty on failure</returns>
		public static String InternetMediaType(MimeType value)
		{
			String ret;
			if(TryGetInternetMediaType(value, out ret) == false)
				ret = String.Empty;
			return ret;
		}

		public static MimeType FromInternetMediaType(String value)
		{
			MimeType ret;
			if(TryParse(value, out ret) == false)
			{
				ret = MimeType.UNKNOWN;
			}
			return ret;
		}

        public static void GetEnumExtension(MimeType value, out String ret)
        {
            if (m_EnumToExt.TryGetValue(value, out ret) == false)
            {
                throw new Exception("Can't match enum to Extension");
            }
        }
        
		public static void GetExtensionEnum(String value, out MimeType ret)
        {
            Enum tmp;
            if (m_ExtToEnum.TryGetValue(value, out tmp) == false)
            {
                throw new Exception("Can't match Extension to enum");
            }
            ret = (MimeType)tmp;
        }

		public static ImageFormat GetImageFormat(MimeType mimeType)
		{
			ImageFormat value = null;
			switch(mimeType)
			{
				case MimeType.PNG:
					value = ImageFormat.Png;
					break;

				case MimeType.JPEG:
					value = ImageFormat.Jpeg;
					break;

				case MimeType.TIFF:
					value = ImageFormat.Tiff;
					break;

				case MimeType.GIF:
					value = ImageFormat.Gif;
					break;

				case MimeType.BMP:
					value = ImageFormat.Bmp;
					break;

				default:
					throw new CommonException("Could not convert image format");
			}
			return value;
		}

		public static MimeType GetMimeType(ImageFormat format)
		{
			MimeType type = MimeType.UNKNOWN;
			if(format == ImageFormat.Jpeg)
				type = MimeType.JPEG;
			else if(format == ImageFormat.Gif)
				type = MimeType.GIF;
			else if(format == ImageFormat.Bmp)
				type = MimeType.BMP;
			else if(format == ImageFormat.Gif)
				type = MimeType.GIF;
			else if(format == ImageFormat.Tiff)
				type = MimeType.TIFF;
			else if(format == ImageFormat.Png)
				type = MimeType.PNG;
			
			return type;
		}

		private static void InitializeDictionary(Type t)
		{
			try
			{
				foreach(FieldInfo field in t.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public))
				{
					object value = field.GetValue(null);
					Type type = value.GetType();
					FieldInfo info = type.GetField(value.ToString());
					
					// Add String Attributes
					EnumStringAttribute[] attribs = (EnumStringAttribute[])info.GetCustomAttributes(typeof(EnumStringAttribute), false);
					if(attribs.Length > 0)
					{
						try
						{
							m_StringToEnum.Add(attribs[0].Value, (Enum)value);
							m_EnumToString.Add((Enum)value, attribs[0].Value);
						}
						catch(Exception)
						{
						}
					}
//					String strVal = (attribs.Length > 0) ? attribs[0].Value : null;

					// Add Extension Attributes
					EnumExtensionAttribute[] attribs2 = (EnumExtensionAttribute[])info.GetCustomAttributes(typeof(EnumExtensionAttribute), false);
					if (attribs2.Length > 0)
					{
						try
						{
							m_ExtToEnum.Add(attribs2[0].Value, (Enum)value);
							m_EnumToExt.Add((Enum)value, attribs2[0].Value);
						}
						catch(Exception)
						{
						}
					}
//					String strVal2 = (attribs2.Length > 0) ? attribs2[0].Value : null;
				}
			}
			catch(Exception)
			{
			}
		}

		public static bool IsGraphic(MimeType type)
		{
			return m_GraphicTypes.Contains(type);
		}

		public static bool IsText(MimeType type)
		{
			return m_TextTypes.Contains(type);
		}

		public class EnumStringAttribute : Attribute
		{
			private readonly String m_strValue;
			public String Value { get { return m_strValue; }  }
			public EnumStringAttribute(String strValue) { this.m_strValue = strValue; }
		}

        public class EnumExtensionAttribute : Attribute
        {
            private readonly String m_strValue;
            public String Value { get { return m_strValue; } }
            public EnumExtensionAttribute(String strValue) { this.m_strValue = strValue; }
        }

		class StringToEnum : Dictionary<String, Enum> {}
		class EnumToString : Dictionary<Enum, String> {}

        class ExtensionToEnum : Dictionary<String, Enum> { }
        class EnumToExtension : Dictionary<Enum, String> { }

	}

	public enum MimeType
	{
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_UNKNOWN)]
		[MimeTypes.EnumString(MimeTypes.MTSTR_UNKNOWN)]
		UNKNOWN = 0,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_WWW_FORM_URLENCODED)]
        X_WWW_FORM_URLENCODED = 1,
        [MimeTypes.EnumString(MimeTypes.MTSTR_EDI_X12)]
        EDI_X12 = 2,
        [MimeTypes.EnumString(MimeTypes.MTSTR_EDIFACT)]
        EDIFACT = 3,
        [MimeTypes.EnumString(MimeTypes.MTSTR_JAVASCRIPT)]
        JAVASCRIPT = 4,
        [MimeTypes.EnumString(MimeTypes.MTSTR_OCTET_STREAM)]
        OCTET_STREAM = 5,
        [MimeTypes.EnumString(MimeTypes.MTSTR_PDF)]
        PDF = 6,
        [MimeTypes.EnumString(MimeTypes.MTSTR_XHTML_XML)]
        XHTML_XML = 7,
        [MimeTypes.EnumString(MimeTypes.MTSTR_XML_DTD)]
        XML_DTD = 8,
        [MimeTypes.EnumString(MimeTypes.MTSTR_JSON)]
        JSON = 9,
        [MimeTypes.EnumString(MimeTypes.MTSTR_ZIP)]
        ZIP = 10,
        [MimeTypes.EnumString(MimeTypes.MTSTR_BASIC)]
        BASIC = 11,
        [MimeTypes.EnumString(MimeTypes.MTSTR_MP4)]
        MP4 = 12,
        [MimeTypes.EnumString(MimeTypes.MTSTR_MPEG)]
        MPEG = 13,
        [MimeTypes.EnumString(MimeTypes.MTSTR_OGG)]
        OGG = 14,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VORBIS)]
        VORBIS = 15,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_MS_WMA)]
        X_MS_WMA = 16,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_RN_REALAUDIO)]
        VND_RN_REALAUDIO = 17,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_WAVE)]
        VND_WAVE = 9001,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_BMP)]
        [MimeTypes.EnumString(MimeTypes.MTSTR_BMP)]
        BMP = 18,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_GIF)]
		[MimeTypes.EnumString(MimeTypes.MTSTR_GIF)]
		GIF = 19,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_JPG)]
		[MimeTypes.EnumString(MimeTypes.MTSTR_JPEG)]
		JPEG = 20,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_PNG)]
		[MimeTypes.EnumString(MimeTypes.MTSTR_PNG)]
		PNG = 21,
        [MimeTypes.EnumString(MimeTypes.MTSTR_SVG_XML)]
        SVG_XML = 22,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_TIFF)]
		[MimeTypes.EnumString(MimeTypes.MTSTR_TIFF)]
		TIFF = 23,
		[MimeTypes.EnumString(MimeTypes.MTSTR_VND_MICROSOFT_ICON)]
		VND_MICROSOFT_ICON = 24,
        [MimeTypes.EnumString(MimeTypes.MTSTR_HTTP)]
        HTTP = 25,
        [MimeTypes.EnumString(MimeTypes.MTSTR_MIXED)]
        MIXED = 26,
        [MimeTypes.EnumString(MimeTypes.MTSTR_ALTERNATIVE)]
        ALTERNATIVE = 27,
        [MimeTypes.EnumString(MimeTypes.MTSTR_RELATED)]
        RELATED = 28,
        [MimeTypes.EnumString(MimeTypes.MTSTR_FORM_DATA)]
        FORM_DATA = 29,
        [MimeTypes.EnumString(MimeTypes.MTSTR_SIGNED)]
        SIGNED = 30,
        [MimeTypes.EnumString(MimeTypes.MTSTR_ENCRYPTED)]
        ENCRYPTED = 31,
        [MimeTypes.EnumString(MimeTypes.MTSTR_CSS)]
        CSS = 32,
        [MimeTypes.EnumString(MimeTypes.MTSTR_CSV)]
        CSV = 33,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_HTML)]
		[MimeTypes.EnumString(MimeTypes.MTSTR_HTML)]
		HTML = 34,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_PLAIN)]
        [MimeTypes.EnumString(MimeTypes.MTSTR_PLAIN)]
        PLAIN = 35,
        [MimeTypes.EnumString(MimeTypes.MTSTR_XML)]
        XML = 36,
        [MimeTypes.EnumString(MimeTypes.MTSTR_QUICKTIME)]
        QUICKTIME = 37,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_MS_WMV)]
        X_MS_WMV = 38,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_OASIS_OPENDOCUMENT_TEXT)]
        VND_OASIS_OPENDOCUMENT_TEXT = 39,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_OASIS_OPENDOCUMENT_SPREADSHEET)]
        VND_OASIS_OPENDOCUMENT_SPREADSHEET = 40,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_OASIS_OPENDOCUMENT_PRESENTATION)]
        VND_OASIS_OPENDOCUMENT_PRESENTATION = 41,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_OASIS_OPENDOCUMENT_GRAPHICS)]
        VND_OASIS_OPENDOCUMENT_GRAPHICS = 42,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_MS_EXCEL)]
        VND_MS_EXCEL = 43,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_MS_POWERPOINT)]
        VND_MS_POWERPOINT = 44,
        [MimeTypes.EnumString(MimeTypes.MTSTR_MSWORD)]
        MSWORD = 45,
        [MimeTypes.EnumString(MimeTypes.MTSTR_VND_MOZILLA_XUL_XML)]
        VND_MOZILLA_XUL_XML = 46,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_DVI)]
        X_DVI = 47,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_HTTPD_PHP)]
        X_HTTPD_PHP = 48,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_HTTPD_PHP_SOURCE)]
        X_HTTPD_PHP_SOURCE = 49,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_LATEX)]
        X_LATEX = 50,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_SHOCKWAVE_FLASH)]
        X_SHOCKWAVE_FLASH = 51,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_STUFFIT)]
        X_STUFFIT = 52,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_RAR_COMPRESSED)]
        X_RAR_COMPRESSED = 53,
        [MimeTypes.EnumString(MimeTypes.MTSTR_X_TAR)]
        X_TAR = 54,
        [MimeTypes.EnumExtension(MimeTypes.MTEXT_MP3)]
        [MimeTypes.EnumString(MimeTypes.MTSTR_MP3)]
        MP3 = 55,
        
        [MimeTypes.EnumString(MimeTypes.MTSTR_NMEA)]
        NMEA = 1001,

	}


}
