using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Mi.PE
{
    public sealed class PEFile
    {
        public PEFormat.DosHeader DosHeader { get; set; }
        public PEFormat.PEHeader Header { get; set; }
        public PEFormat.OptionalHeader OptionalHeader { get; set; }
        public PEFormat.Section[] Sections { get; set; }
    }
}