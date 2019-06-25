using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Conversions
{
    public interface ICopyable<T>
    {
        void CopyFrom(T obj);
    }
}
