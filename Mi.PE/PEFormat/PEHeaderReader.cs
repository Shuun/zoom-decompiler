using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mi.PE.PEFormat
{
    using Mi.PE.Internal;

    public static class PEHeaderReader
    {
        public const int PEHeaderSize = 24;

        public static PEHeader Read(Stream stream)
        {
            var machine = (Machine)stream.CheckedReadInt16("reading architecture field of PE file");

            int numberOfSections = stream.CheckedReadUInt16("reading number of sections of PE file");

            var timestamp = new ImageTimestamp(stream.CheckedReadUInt32("reading timestamp field of PE header"));
            int pointerToSymbolTable = stream.CheckedReadInt32("reading pointer to symbol table field of PE header");
            int numberOfSymbols = stream.CheckedReadInt32("reading number of symbols field of PE header");
            ushort optionalHeaderSize = stream.CheckedReadUInt16("reading optional PE header size");    
            var characteristics = (ImageCharacteristics)stream.CheckedReadUInt16("reading characteristics");

            return new PEHeader
            {
                Machine = machine,
                NumberOfSections = numberOfSections,
                Timestamp = timestamp,
                PointerToSymbolTable = pointerToSymbolTable,
                NumberOfSymbols = numberOfSymbols,
                SizeOfOptionalHeader = optionalHeaderSize,
                Characteristics = characteristics
            };
        }
    }
}
