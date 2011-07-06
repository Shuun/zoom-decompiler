using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.PE.PEFormat
{
    public sealed class DataDirectory
    {
        /// <summary> The relative virtual address of the table. </summary>
        public uint VirtualAddress { get; set; }

        /// <summary> The size of the table, in bytes. </summary>
        public uint Size { get; set; }
    }
}
