using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    public enum Magic : ushort
    {
        NT32 = 0x010B,
        NT64 = 0x020B,
        ROM = 0x107
    }
}
