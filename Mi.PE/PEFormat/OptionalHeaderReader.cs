using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mi.PE.PEFormat
{
    using Mi.PE.Internal;

    public static class OptionalHeaderReader
    {
        static class ReadHeaderSizes
        {
            public const uint OptionalHeader32 = 96;
            public const uint OptionalHeader64 = 112;
        }

        public static OptionalHeader Read(Stream stream, ushort optionalHeaderSize)
        {
            var optionalHeader = new OptionalHeader();
            optionalHeader.PEMagic = ReadPEMagic(stream);
            
            optionalHeader.MajorLinkerVersion = stream.CheckedReadByte("reading major linker version in optional PE header");
            optionalHeader.MinorLinkerVersion = stream.CheckedReadByte("reading minor linker version in optional PE header");
            optionalHeader.SizeOfCode = stream.CheckedReadInt32("reading size of code field in optional PE header");
            optionalHeader.SizeOfInitializedData = stream.CheckedReadInt32("reading size of initialized data field in optional PE header");
            optionalHeader.SizeOfUninitializedData = stream.CheckedReadInt32("reading size of uninitalized data field in optional PE header");
            optionalHeader.AddressOfEntryPoint = stream.CheckedReadInt32("reading RVA of entry point in optional PE header");
            optionalHeader.BaseOfCode = stream.CheckedReadInt32("reading base of code field in optional PE header");

            if (optionalHeader.PEMagic == Magic.NT32)
            {
                optionalHeader.BaseOfData = stream.CheckedReadInt32("reading base of data field in optional PE header");
                optionalHeader.ImageBase = stream.CheckedReadUInt32("reading (32-bit) image base field in optional PE header");
            }
            else
            {
                optionalHeader.ImageBase = stream.CheckedReadUInt64("reading (64-bit) image base field in optional PE header");
            }

            optionalHeader.SectionAlignment = stream.CheckedReadInt32("reading section alignment field in optional PE header");
            optionalHeader.FileAlignment = stream.CheckedReadUInt32("reading file alignment field in optional PE header");
            optionalHeader.MajorOperatingSystemVersion = stream.CheckedReadUInt16("reading major operting system version field in optional PE header");
            optionalHeader.MinorOperatingSystemVersion = stream.CheckedReadUInt16("reading minor operating system version field in optional PE header");
            optionalHeader.MajorImageVersion = stream.CheckedReadUInt16("reading major image version field in optional PE header");
            optionalHeader.MinorImageVersion = stream.CheckedReadUInt16("reading minor image version field in optional PE header");
            optionalHeader.MajorSubsystemVersion = stream.CheckedReadUInt16("reading major subsystem version field in optional PE header");
            optionalHeader.MinorSubsystemVersion = stream.CheckedReadUInt16("reading minor subsystem version field in optional PE header");
            optionalHeader.Win32VersionValue = stream.CheckedReadUInt32("reading Win32Version (reserved) field in optional PE header");
            optionalHeader.SizeOfImage = stream.CheckedReadInt32("reading size of image field in optional PE header");
            optionalHeader.SizeOfHeaders = stream.CheckedReadInt32("reading size of headers field in optional PE header");
            optionalHeader.CheckSum = stream.CheckedReadUInt32("reading checksum field in optional PE header");
            optionalHeader.Subsystem = (Subsystem)stream.CheckedReadUInt16("reading subsystem field in optional PE header");
            optionalHeader.DllCharacteristics = (DllCharacteristics)stream.CheckedReadUInt16("reading DLL characteristics field in optional PE header");

            if (optionalHeader.PEMagic == Magic.NT32)
            {
                optionalHeader.SizeOfStackReserve = stream.CheckedReadUInt32("reading size of stack reserve field in optional PE header");
                optionalHeader.SizeOfStackCommit = stream.CheckedReadUInt32("reading size of stack commit field in optional PE header");
                optionalHeader.SizeOfHeapReserve = stream.CheckedReadUInt32("reading size of heap reserve field in optional PE header");
                optionalHeader.SizeOfHeapCommit = stream.CheckedReadUInt32("reading size of heap commit field in optional PE header");
            }
            else
            {
                optionalHeader.SizeOfStackReserve = stream.CheckedReadUInt64("reading size of stack reserve field in optional PE header");
                optionalHeader.SizeOfStackCommit = stream.CheckedReadUInt64("reading size of stack commit field in optional PE header");
                optionalHeader.SizeOfHeapReserve = stream.CheckedReadUInt64("reading size of heap reserve field in optional PE header");
                optionalHeader.SizeOfHeapCommit = stream.CheckedReadUInt64("reading size of heap commit field in optional PE header");
            }

            optionalHeader.LoaderFlags = stream.CheckedReadUInt32("reading loader flags field in optional PE header");
            optionalHeader.NumberOfRvaAndSizes = stream.CheckedReadInt32("reading NumberOfRvaAndSizes field in optional PE header");

            uint readHeaderSize = optionalHeader.PEMagic == Magic.NT32 ?
                ReadHeaderSizes.OptionalHeader32 : 
                ReadHeaderSizes.OptionalHeader64;

            if (optionalHeader.NumberOfRvaAndSizes > 0)
            {
                var directories = new DataDirectory[optionalHeader.NumberOfRvaAndSizes];
                for (int i = 0; i < directories.Length; i++)
                {
                    directories[i] = new DataDirectory
                    {
                        VirtualAddress = stream.CheckedReadUInt32("reading virtual address field of PE data directory structure"),
                        Size = stream.CheckedReadUInt32("reading size field of PE data directory structure")
                    };
                }
                optionalHeader.DataDirectories = directories;
            }

            return optionalHeader;
        }

        static Magic ReadPEMagic(Stream stream)
        {
            var peMagic = (Magic)stream.CheckedReadInt16("reading PE or PE+ magic");

            switch (peMagic)
	        {
		        case Magic.NT32:
                case Magic.NT64:
                    return peMagic;

                case Magic.ROM:
                    throw new BadImageFormatException("Unsupported PE magic value 'ROM' " + ((ushort)peMagic).ToString("X") + "h.");

                default:
                    throw new BadImageFormatException("Invalid PE magic value " + ((ushort)peMagic).ToString("X")+"h.");
	        }
        }
    }
}