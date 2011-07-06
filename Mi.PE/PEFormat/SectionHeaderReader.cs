using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mi.PE.PEFormat
{
    using Mi.PE.Internal;

    public static class SectionHeaderReader
    {
        public static Section Read(Stream stream)
        {
            var section = new Section
            {
                Name = stream.CheckedReadFixedString(8, "reading name of section"),
	            VirtualSize = stream.CheckedReadUInt32("reading virtual size of section"),
	            VirtualAddress = stream.CheckedReadUInt32("reading virtual address of section"),
	            SizeOfRawData = stream.CheckedReadUInt32("reading size of raw data field of section"),
	            PointerToRawData = stream.CheckedReadUInt32("reading pointer to raw data field of section"),
	            PointerToRelocations = stream.CheckedReadUInt32("reading pointer to relocations field of section"),
	            PointerToLinenumbers = stream.CheckedReadUInt32("reading pointer to line numbers field of section"),
	            NumberOfRelocations = stream.CheckedReadUInt16("reading number of relocations field of section"),
	            NumberOfLinenumbers = stream.CheckedReadUInt16("reading number of line numbers field of section"),
	            Characteristics = (SectionCharacteristics)stream.CheckedReadUInt32("reading characteristics field of section")
            };

            return section;
        }
    }
}