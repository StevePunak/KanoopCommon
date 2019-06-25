using KanoopCommon.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System;
using KanoopCommon.Addresses;
using System.Globalization;
using KanoopCommon.CommonObjects;

namespace KanoopCommon.Conversions
{
	public class AddressConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string strvalue = value as string;
			if (strvalue != null)
			{
				AddressBase addr = AddressBase.Factory(strvalue);
				return addr;
			}
			else
				return new AddressBase();
		}
	}

	public class UUIDConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			UUID uuid = UUID.EmptyUUID;
			try
			{
				if (value != null)
				{
					uuid = new UUID(value as string);
				}
			}
			catch { }

			return uuid;
		}
	}

	public class SimpleEnumConverter<T> : TypeConverter 
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{

			T retVal = default(T);
			try
			{
                if (value != null && value.GetType() != typeof(DBNull))
//					Enum.TryParse<T>(value.ToString(), true,out retVal);
					retVal = (T)Enum.Parse(typeof(T), value.ToString(), true);
//				retVal = (T)value;
			}
			catch
			{ 
			}

			return retVal;
		}
	}

	public class IPv4AddressPortConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			IPv4AddressPort address;
			string			strvalue = value as string;
			if(strvalue == null || IPv4AddressPort.TryParse(strvalue, out address) == false)
			{
				address = new IPv4AddressPort();
			}
			return address;
		}
	
	}

}
