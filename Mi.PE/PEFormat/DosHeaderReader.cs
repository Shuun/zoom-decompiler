using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Mi.PE.PEFormat
{
    using Mi.PE.Internal;

    public static class DosHeaderReader
    {
        public const int DosHeaderSize = 64;

        public static DosHeader Read(Stream stream)
        {
            stream.CheckedExpect((ushort)PESignature.MZ, "reading MZ signature");

            byte[] headerBuf = new byte[DosHeaderSize];
            stream.CheckedReadBytes(headerBuf, "reading DOS header");

            var result = new DosHeader();

            result.cblp = stream.CheckedReadUInt16("reading cblp field of DOS header");
            result.cp = stream.CheckedReadUInt16("reading cp field of DOS header");
            result.crlc = stream.CheckedReadUInt16("reading crlc field of DOS header");
            result.cparhdr = stream.CheckedReadUInt16("reading cparhdr field of DOS header");
            result.minalloc = stream.CheckedReadUInt16("reading minalloc field of DOS header");
            result.maxalloc = stream.CheckedReadUInt16("reading maxalloc field of DOS header");
            result.ss = stream.CheckedReadUInt16("reading ss field of DOS header");
            result.sp = stream.CheckedReadUInt16("reading sp field of DOS header");
            result.csum = stream.CheckedReadUInt16("reading csum field of DOS header");
            result.ip = stream.CheckedReadUInt16("reading ip field of DOS header");
            result.cs = stream.CheckedReadUInt16("reading cs field of DOS header");
            result.lfarlc = stream.CheckedReadUInt16("reading lfarlc field of DOS header");
            result.ovno = stream.CheckedReadUInt16("reading ovno field of DOS header");

            result.res1 = stream.CheckedReadUInt64("reading res1 field of DOS header");

            result.oemid = stream.CheckedReadUInt16("reading oemid field of DOS header");
            result.oeminfo = stream.CheckedReadUInt16("reading oeminfo field of DOS header");

            var res2 = new DosHeader.Reserved10Bytes();
            res2.Byte0 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte1 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte2 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte3 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte4 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte5 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte6 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte7 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte8 = stream.CheckedReadByte("reading res2 field of DOS header");
            res2.Byte9 = stream.CheckedReadByte("reading res2 field of DOS header");

            result.res2 = res2;

            result.lfanew = stream.CheckedReadUInt32("reading res2 field of DOS header");

            return result;
        }
    }
}