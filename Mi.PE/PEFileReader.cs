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
            stream.CheckedExpect((byte)'M', (byte)'Z', "reading the leading MZ singature of PE file");

            var dosHeader = DosHeaderReader.Read(stream);

            uint peHeaderOffset = stream.CheckedReadUInt32("reading PE header offset");

            stream.CheckedSkipBytes(
                peHeaderOffset - (2 + DosHeaderReader.DosHeaderSize + 4),
                "seeking to PE header");

            stream.CheckedExpect(
                (byte)'P', (byte)'E',
                0, 0,
                "reading the leading MZ singature of PE file");

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