using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KanoopCommon.Reflection;

namespace KanoopCommon.Serialization
{
	public class KVPSerializer
	{
		public static String Serialize(Object thing)
		{
			StringBuilder sb = new StringBuilder();

			PropertyInfo[] properties = thing.GetType().GetProperties();
			foreach(PropertyInfo property in properties)
			{
				sb.AppendFormat("{0}={1}\n", property.Name, property.GetValue(thing).ToString());
			}
			return sb.ToString();
		}

		public static T Deserialize<T>(String from)
		{
			T thing = Activator.CreateInstance<T>();
			using(StringReader sr = new StringReader(from))
			{
				String line;
				while((line = sr.ReadLine()) != null)
				{
					int index = line.IndexOf('=');
					if(index > 0)
					{
						if(index < line.Length)
						{
							String propertyName = line.Substring(0, index);
							String propertyValue = line.Substring(index + 1);

							PropertyInfo property = typeof(T).GetProperty(propertyName);
							if(property != null && property.CanWrite)
							{
								ReflectionUtilities.SetProperty(thing, propertyName, propertyValue);
							}
						}
					}
				}
			}
			return thing;
		}


	}
}
