using System;

namespace Mi.PE.PEFormat
{
    public enum PEMagic : ushort
    {
        NT32 = 0x010B,
        NT64 = 0x020B,
        ROM = 0x107
    }
}
