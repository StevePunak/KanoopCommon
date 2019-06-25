using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace KanoopCommon.Addresses
{

	public class EmptyAddress : AddressBase
	{
		public override int Length
		{
			get { return AddressLengths.NONE; }
		}

		public override String Address
		{
			get { return ""; }
			set { return; }
		}

		public override byte[] AddressAsByteArray
		{
			get { return null; }
			set { return; }
		}

		public override bool Equals(object obj)
        {
            try
            {
                return ((EmptyAddress)obj)._address == this._address;
            }
            catch
            {
                return false;
            }
        }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/**
		 * @b          AddressBase
		 *             Constructor
		 */
		public EmptyAddress(byte[] inArray)
		{
		}

		public EmptyAddress(String address)
		{
		}

		public EmptyAddress()
		{
			_address = "";
			_type = AddressType.NONE;
		}
	}

}
