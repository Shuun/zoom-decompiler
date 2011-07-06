using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    [Flags]
    public enum ImageCharacteristics : ushort
    {
        /// <summary>
        /// Relocation information was stripped from the file.
        /// The file must be loaded at its preferred base address.
        /// If the base address is not available, the loader reports an error.
        /// </summary>
        RelocsStripped = 0x0001,

        /// <summary>
        /// The file is executable (there are no unresolved external references).
        /// </summary>
        ExecutableImage = 0x0002,

        /// <summary>
        /// COFF line numbers were stripped from the file.
        /// </summary>
        LineNumsStripped = 0x0004,

        /// <summary>
        /// COFF symbol table entries were stripped from file.
        /// </summary>
        LocalSymsStripped = 0x0008,

        /// <summary>
        /// Aggressively trim the working set.
        /// This value is obsolete as of Windows 2000.
        /// </summary>
        AggressiveWsTrim = 0x0010,

        /// <summary>
        /// The application can handle addresses larger than 2 GB.
        /// </summary>
        LargeAddressAware = 0x0020,

        /// <summary>
        /// The bytes of the word are reversed. This flag is obsolete.
        /// </summary>
        BytesReversedLo = 0x0080,

        /// <summary>
        /// The computer supports 32-bit words.
        /// </summary>
        Bit32Machine = 0x0100,

        /// <summary>
        /// Debugging information was removed and stored separately in another file.
        /// </summary>
        DebugStripped = 0x0200,

        /// <summary>
        /// If the image is on removable media, copy it to and run it from the swap file.
        /// </summary>
        RemovableRunFromSwap = 0x0400,

        /// <summary>
        /// If the image is on the network, copy it to and run it from the swap file.
        /// </summary>
        NetRunFromSwap = 0x0800,

        /// <summary>
        /// The image is a system file.
        /// </summary>
        System = 0x1000,

        /// <summary>
        /// The image is a DLL file.
        /// While it is an executable file, it cannot be run directly.
        /// </summary>
        Dll = 0x2000,

        /// <summary>
        /// The file should be run only on a uniprocessor computer.
        /// </summary>
        UpSystemOnly = 0x4000,

        /// <summary>
        /// The bytes of the word are reversed. This flag is obsolete.
        /// </summary>
        BytesReversedHi = 0x8000
    }
}