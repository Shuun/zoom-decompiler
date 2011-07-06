using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    public sealed class OptionalHeader
    {
        public Magic PEMagic { get; set; }
        public byte MajorLinkerVersion { get; set; }
        public byte MinorLinkerVersion { get; set; }

        /// <summary> The size of the code section, in bytes, or the sum of all such sections if there are multiple code sections. </summary>
        public int SizeOfCode { get; set; }

        /// <summary> The size of the initialized data section, in bytes, or the sum of all such sections if there are multiple initialized data sections. </summary>
        public int SizeOfInitializedData { get; set; }

        /// <summary> The size of the uninitialized data section, in bytes, or the sum of all such sections if there are multiple uninitialized data sections. </summary>
        public int SizeOfUninitializedData { get; set; }

        /// <summary>
        /// A pointer to the entry point function, relative to the image base address.
        /// For executable files, this is the starting address.
        /// For device drivers, this is the address of the initialization function.
        /// The entry point function is optional for DLLs.
        /// When no entry point is present, this member is zero.
        /// </summary>
        public int AddressOfEntryPoint { get; set; }

        /// <summary> A pointer to the beginning of the code section, relative to the image base. </summary>
        public int BaseOfCode { get; set; }

        /// <summary> A pointer to the beginning of the data section, relative to the image base. </summary>
        public int BaseOfData { get; set; }

        /// <summary>
        /// The preferred address of the first byte of the image when it is loaded in memory.
        /// This value is a multiple of 64K bytes.
        /// The default value for DLLs is 0x10000000.
        /// The default value for applications is 0x00400000,
        /// except on Windows CE where it is 0x00010000.
        /// </summary>
        public ulong ImageBase { get; set; }

        /// <summary>
        /// The alignment of sections loaded in memory, in bytes.
        /// This value must be greater than or equal to the FileAlignment member.
        /// The default value is the page size for the system.
        /// </summary>
        public int SectionAlignment { get; set; }

        /// <summary>
        /// The alignment of the raw data of sections in the image file, in bytes.
        /// The value should be a power of 2 between 512 and 64K (inclusive).
        /// The default is 512.
        /// If the <see cref="SectionAlignment"/> member is less than the system page size,
        /// this member must be the same as <see cref="SectionAlignment"/>.
        /// </summary>
        public uint FileAlignment { get; set; }

        /// <summary>
        /// The major version number of the required operating system.
        /// </summary>
        public ushort MajorOperatingSystemVersion { get; set; }

        /// <summary>
        /// The minor version number of the required operating system.
        /// </summary>
        public ushort MinorOperatingSystemVersion { get; set; }

        /// <summary>
        /// The major version number of the image.
        /// </summary>
        public ushort MajorImageVersion { get; set; }

        /// <summary>
        /// The minor version number of the image.
        /// </summary>
        public ushort MinorImageVersion { get; set; }

        /// <summary>
        /// The major version number of the subsystem.
        /// </summary>
        public ushort MajorSubsystemVersion { get; set; }

        /// <summary>
        /// The minor version number of the subsystem.
        /// </summary>
        public ushort MinorSubsystemVersion { get; set; }

        /// <summary>
        /// This member is reserved and must be 0.
        /// </summary>
        public uint Win32VersionValue { get; set; }

        /// <summary>
        /// The size of the image, in bytes, including all headers. Must be a multiple of <see cref="SectionAlignment"/>.
        /// </summary>
        public int SizeOfImage { get; set; }

        /// <summary>
        /// The combined size of the MS-DOS stub, the PE header, and the section headers,
        /// rounded to a multiple of the value specified in the FileAlignment member.
        /// </summary>
        public int SizeOfHeaders { get; set; }

        /// <summary>
        /// The image file checksum.
        /// The following files are validated at load time:
        /// all drivers,
        /// any DLL loaded at boot time,
        /// and any DLL loaded into a critical system process.
        /// </summary>
        public uint CheckSum { get; set; }

        /// <summary>
        /// The subsystem required to run this image.
        /// </summary>
        public Subsystem Subsystem { get; set; }

        /// <summary>
        /// The DLL characteristics of the image.
        /// </summary>
        public DllCharacteristics DllCharacteristics { get; set; }

        /// <summary>
        /// The number of bytes to reserve for the stack.
        /// Only the memory specified by the <see cref="SizeOfStackCommit"/> member is committed at load time;
        /// the rest is made available one page at a time until this reserve size is reached.
        /// </summary>
        public ulong SizeOfStackReserve { get; set; }

        /// <summary>
        /// The number of bytes to commit for the stack.
        /// </summary>
        public ulong SizeOfStackCommit { get; set; }

        /// <summary>
        /// The number of bytes to reserve for the local heap.
        /// Only the memory specified by the <see cref="SizeOfHeapCommit"/> member is committed at load time;
        /// the rest is made available one page at a time until this reserve size is reached.
        /// </summary>
        public ulong SizeOfHeapReserve { get; set; }

        /// <summary>
        /// The number of bytes to commit for the local heap.
        /// </summary>
        public ulong SizeOfHeapCommit { get; set; }

        /// <summary>
        /// This member is obsolete.
        /// </summary>
        public uint LoaderFlags { get; set; }

        /// <summary>
        /// The number of directory entries in the remainder of the optional header.
        /// Each entry describes a location and size.
        /// </summary>
        public int NumberOfRvaAndSizes { get; set; }

        public DataDirectory[] DataDirectories { get; set; }
    }
}