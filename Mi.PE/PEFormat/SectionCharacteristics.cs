using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    [Flags]
    public enum SectionCharacteristics
    {
        Reserved_0h = 0x00000000,
        Reserved_1h = 0x00000001,
        Reserved_2h = 0x00000002,
        Reserved_4h = 0x00000004,

        /// <summary>
        /// The section should not be padded to the next boundary.
        /// This flag is obsolete and is replaced by <see cref="Align1Bytes"/>.
        /// </summary>
        NoPadding = 0x00000008,

        Reserved_10h = 0x00000010,

        /// <summary>
        /// The section contains executable code.
        /// </summary>
        ContainsCode = 0x00000020,

        /// <summary>
        /// The section contains initialized data.
        /// </summary>
        ContainsInitializedData = 0x00000040,

        /// <summary>
        /// The section contains uninitialized data.
        /// </summary>
        ContainsUninitializedData = 0x00000080,

        /// <summary>
        /// Reserved.
        /// </summary>
        LinkerOther = 0x00000100,

        /// <summary>
        /// The section contains comments or other information.
        /// This is valid only for object files.
        /// </summary>
        LinkerInfo = 0x00000200,

        Reserved_400h = 0x00000400,

        /// <summary>
        /// The section will not become part of the image.
        /// This is valid only for object files.
        /// </summary>
        LinkerRemove = 0x00000800,

        /// <summary>
        /// The section contains COMDAT data.
        /// This is valid only for object files.
        /// </summary>
        LinkerCOMDAT = 0x00001000,

        Reserved_2000h = 0x00002000,

        /// <summary>
        /// Reset speculative exceptions handling bits in the TLB entries for this section.
        /// </summary>
        NoDeferredSpeculativeExecution = 0x00004000,

        /// <summary>
        /// The section contains data referenced through the global pointer.
        /// </summary>
        GlobalPointerRelative = 0x00008000,

        Reserved_10000h = 0x00010000,

        /// <summary>
        /// Reserved.
        /// </summary>
        MemoryPurgeable = 0x00020000,

        /// <summary>
        /// Reserved.
        /// </summary>
        MemoryLocked = 0x00040000,

        /// <summary>
        /// Reserved.
        /// </summary>
        MemoryPreload = 0x00080000,

        /// <summary>
        /// Align data on a 1-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align1Bytes = 0x00100000,

        /// <summary>
        /// Align data on a 2-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align2Bytes = 0x00200000,

        /// <summary>
        /// Align data on a 4-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align4Bytes = 0x00300000,

        /// <summary>
        /// Align data on a 8-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align8Bytes = 0x00400000,

        /// <summary>
        /// Align data on a 16-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align16Bytes = 0x00500000,

        /// <summary>
        /// Align data on a 32-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align32Bytes = 0x00600000,

        /// <summary>
        /// Align data on a 64-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align64Bytes = 0x00700000,

        /// <summary>
        /// Align data on a 128-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align128Bytes = 0x00800000,

        /// <summary>
        /// Align data on a 256-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align256Bytes = 0x00900000,

        /// <summary>
        /// Align data on a 512-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align512Bytes = 0x00A00000,

        /// <summary>
        /// Align data on a 1024-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align1024Bytes = 0x00B00000,

        /// <summary>
        /// Align data on a 2048-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align2048Bytes = 0x00C00000,

        /// <summary>
        /// Align data on a 4096-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align4096Bytes = 0x00D00000,

        /// <summary>
        /// Align data on a 8192-byte boundary.
        /// This is valid only for object files.
        /// </summary>
        Align8192Bytes = 0x00E00000,

        /// <summary>
        /// The section contains extended relocations.
        /// The count of relocations for the section exceeds the 16 bits that is reserved for it in the section header.
        /// If the NumberOfRelocations field in the section header is 0xffff,
        /// the actual relocation count is stored in the VirtualAddress field of the first relocation.
        /// It is an error if <see cref="LinkerRelocationOverflow"/> is set and there are fewer than 0xffff relocations in the section.
        /// </summary>
        LinkerRelocationOverflow = 0x01000000,

        /// <summary>
        /// The section can be discarded as needed.
        /// </summary>
        MemoryDiscardable = 0x02000000,

        /// <summary>
        /// The section cannot be cached.
        /// </summary>
        MemoryNotCached = 0x04000000,

        /// <summary>
        /// The section cannot be paged.
        /// </summary>
        MemoryNotPaged = 0x08000000,

        /// <summary>
        /// The section can be shared in memory.
        /// </summary>
        MemoryShared = 0x10000000,

        /// <summary>
        /// The section can be executed as code.
        /// </summary>
        MemoryExecute = 0x20000000,

        /// <summary>
        /// The section can be read.
        /// </summary>
        MemoryRead = 0x40000000,

        /// <summary>
        /// The section can be written to.
        /// </summary>
        MemoryWrite = unchecked((int)0x80000000)
    }
}