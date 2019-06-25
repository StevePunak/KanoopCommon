using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace KanoopCommon.Database
{
	public interface ILoadable
	{
		void LoadFrom(DatabaseDataReader reader);
	}
}
