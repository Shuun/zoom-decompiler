using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Mi.PE.PEFormat
{
    public sealed class PEHeader
    {
        /// <summary>
        /// The architecture type of the computer.
        /// An image file can only be run on the specified computer or a system that emulates the specified computer.
        /// </summary>
        public Machine Machine { get; set; }

        /// <summary>
        ///  Indicates the size of the section table, which immediately follows the headers.
        ///  Note that the Windows loader limits the number of sections to 96.
        /// </summary>
        public int NumberOfSections { get; set; }

        /// <summary>
        /// The low 32 bits of the time stamp of the image.
        /// This represents the date and time the image was created by the linker.
        /// The value is represented in the number of seconds elapsed since
        /// midnight (00:00:00), January 1, 1970, Universal Coordinated Time,
        /// according to the system clock.
        /// </summary>
        public ImageTimestamp Timestamp { get; set; }

        /// <summary>
        /// The offset of the symbol table, in bytes, or zero if no COFF symbol table exists.
        /// </summary>
        public int PointerToSymbolTable { get; set; }

        /// <summary>
        /// The number of symbols in the symbol table.
        /// </summary>
        public int NumberOfSymbols { get; set; }

        /// <summary>
        /// The size of the optional header, in bytes. This value should be 0 for object files.
        /// </summary>
        public ushort SizeOfOptionalHeader { get; set; }

        /// <summary>
        /// The characteristics of the image.
        /// </summary>
        public ImageCharacteristics Characteristics { get; set; }
    }
}