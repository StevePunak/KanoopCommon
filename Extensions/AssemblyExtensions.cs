using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace KanoopCommon.Extensions
{
	public static class AssemblyExtensions
	{
		#region Private Member Variables

		static Dictionary<String, Dictionary<String, Type>> _AssemblyTypeCache;

		static AssemblyExtensions()
		{
			_AssemblyTypeCache = new Dictionary<String, Dictionary<String, Type>>();
		}

		#endregion

		#region Public Extension Methods

		public static Type GetFirstNonNamespacedType(this Assembly assembly, String name)
		{
			Type returnType = null;

			lock(_AssemblyTypeCache)
			{
				/** try to get cached list for the given assembly, and if not extant create a new one and add it */
				Dictionary<String, Type> assemblyTypes;
				if(_AssemblyTypeCache.TryGetValue(assembly.GetName().Name, out assemblyTypes) == false)
				{
					assemblyTypes = new Dictionary<String, Type>();
					_AssemblyTypeCache.Add(assembly.GetName().Name, assemblyTypes);
				}

				/** try to get the cached type value */
				if(assemblyTypes.TryGetValue(name, out returnType) == false)
				{
					/** if not there, try to find it */
					returnType = null;
					foreach(Type type in assembly.GetTypes())
					{
						if(type.Name.EndsWith(name))
						{
							/** add it to cache and return */
							returnType = type;
							assemblyTypes.Add(name, returnType);
							break;
						}
					}
				}
			}

			return returnType;
		}

		#endregion
	}
}
