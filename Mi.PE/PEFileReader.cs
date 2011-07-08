using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mi.PE
{
    using Mi.PE.Internal;
    using Mi.PE.PEFormat;

    public sealed class PEFileReader
    {
        public PEFile Read(Stream stream)
        {
            var dosHeader = DosHeaderReader.Read(stream);

            uint fillingByteCount = dosHeader.lfanew - (2 + DosHeaderReader.DosHeaderSize + 4);
            byte[] fillingBytes = fillingByteCount > 0 ? new byte[fillingByteCount] : null;
            stream.CheckedReadBytes(fillingBytes, "seeking to PE header");

            stream.CheckedExpect(
                (byte)'P', (byte)'E',
                0, 0,
                "reading PE singature");

            var peHeader = PEHeaderReader.Read(stream);

            var optionalHeader = OptionalHeaderReader.Read(
                stream,
                peHeader.SizeOfOptionalHeader);

            Section[] sections;
            if (peHeader.NumberOfSections > 0)
            {
                sections = new Section[peHeader.NumberOfSections];

                for (int i = 0; i < sections.Length; i++)
                {
                    sections[i] = SectionHeaderReader.Read(stream);
                }
            }
            else
            {
                sections = null;
            }

            return new PEFile
            {
                DosHeader = dosHeader,
                Header = peHeader,
                OptionalHeader = optionalHeader,
                Sections = sections
            };
        }
    }
}