using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    [Flags]
    public enum DllCharacteristics : ushort
    {
        /// <summary> Reserved. </summary>
        ProcessInit = 0x0001,

        /// <summary> Reserved. </summary>
        ProcessTerm = 0x0002,

        /// <summary> Reserved. </summary>
        ThreadInit = 0x0004,

        /// <summary> Reserved. </summary>
        ThreadTerm = 0x0008,

        /// <summary> The DLL can be relocated at load time. </summary>
        DynamicBase = 0x0040,

        /// <summary>
        /// Code integrity checks are forced.
        /// If you set this flag and a section contains only uninitialized data,
        /// set the PointerToRawData member of IMAGE_SECTION_HEADER
        /// for that section to zero;
        /// otherwise, the image will fail to load because the digital signature cannot be verified.
        /// </summary>
        ForceIntegrity = 0x0040,

        /// <summary> The image is compatible with data execution prevention (DEP). </summary>
        NxCompatible = 0x0100,

        /// <summary> The image is isolation aware, but should not be isolated. </summary>
        NoIsolation = 0x0200,

        /// <summary> The image does not use structured exception handling (SEH). No SE handler may reside in this image. </summary>
        NoSEH = 0x0400,

        /// <summary> Do not bind this image. </summary>
        NoBind = 0x0800,

        /// <summary> The image must run inside an AppContainer. </summary>
        AppContainer = 0x1000,

        /// <summary> WDM (Windows Driver Model) driver. </summary>
        WdmDriver = 0x2000,

        /// <summary> Reserved (no specific name). </summary>
        Reserved = 0x4000,

        /// <summary> The image is terminal server aware. </summary>
        TerminalServerAware = 0x8000,
    }
}