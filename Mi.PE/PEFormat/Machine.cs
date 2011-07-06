using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    /// <summary>
    /// Target CPU types.
    /// </summary>
    public enum Machine : ushort
    {
        /// <summary>
        /// The target CPU is unknown or not specified.
        /// </summary>
        Unknown = 0x0000,
        /// <summary>
        /// Intel 386.
        /// </summary>
        I386 = 0x014C,
        /// <summary>
        /// MIPS little-endian
        /// </summary>
        R3000 = 0x0162,
        /// <summary>
        /// MIPS little-endian
        /// </summary>
        R4000 = 0x0166,
        /// <summary>
        /// MIPS little-endian
        /// </summary>
        R10000 = 0x0168,
        /// <summary>
        /// MIPS little-endian WCE v2
        /// </summary>
        WCEMIPSV2 = 0x0169,
        /// <summary>
        /// Alpha_AXP
        /// </summary>
        Alpha = 0x0184,
        /// <summary>
        /// SH3 little-endian
        /// </summary>
        SH3 = 0x01a2,
        /// <summary>
        /// SH3 little-endian. DSP.
        /// </summary>
        SH3DSP = 0x01a3,
        /// <summary>
        /// SH3E little-endian.
        /// </summary>
        SH3E = 0x01a4,
        /// <summary>
        /// SH4 little-endian.
        /// </summary>
        SH4 = 0x01a6,
        /// <summary>
        /// SH5.
        /// </summary>
        SH5 = 0x01a8,
        /// <summary>
        /// ARM Little-Endian
        /// </summary>
        ARM = 0x01c0,
        /// <summary>
        /// Thumb.
        /// </summary>
        Thumb = 0x01c2,
        /// <summary>
        /// AM33
        /// </summary>
        AM33 = 0x01d3,
        /// <summary>
        /// IBM PowerPC Little-Endian
        /// </summary>
        PowerPC = 0x01F0,
        /// <summary>
        /// PowerPCFP
        /// </summary>
        PowerPCFP = 0x01f1,
        /// <summary>
        /// Intel 64
        /// </summary>
        IA64 = 0x0200,
        /// <summary>
        /// MIPS
        /// </summary>
        MIPS16 = 0x0266,
        /// <summary>
        /// ALPHA64
        /// </summary>
        Alpha64 = 0x0284,
        /// <summary>
        /// MIPS
        /// </summary>
        MIPSFPU = 0x0366,
        /// <summary>
        /// MIPS
        /// </summary>
        MIPSFPU16 = 0x0466,
        /// <summary>
        /// AXP64
        /// </summary>
        AXP64 = Alpha64,
        /// <summary>
        /// Infineon
        /// </summary>
        Tricore = 0x0520,
        /// <summary>
        /// CEF
        /// </summary>
        CEF = 0x0CEF,
        /// <summary>
        /// EFI Byte Code
        /// </summary>
        EBC = 0x0EBC,
        /// <summary>
        /// AMD64 (K8)
        /// </summary>
        AMD64 = 0x8664,
        /// <summary>
        /// M32R little-endian
        /// </summary>
        M32R = 0x9041,
        /// <summary>
        /// CEE
        /// </summary>
        CEE = 0xC0EE,
    }
}
