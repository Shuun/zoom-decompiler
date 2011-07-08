using System;

namespace Mi.PE.PEFormat
{
    public enum PESignature
    {
        MZ = 'M' + ('Z' << 8)
    }
}