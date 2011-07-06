using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Mi.PE.PEFormat
{
    using Mi.PE.Internal;

    public static class DosHeaderReader
    {
        public const int DosHeaderSize = 58;

        public static DosHeader Read(Stream stream)
        {
            byte[] headerBuf = new byte[DosHeaderSize];
            stream.CheckedReadBytes(headerBuf, "reading DOS header");

            // shallow copy -- we just need a new array of the same byte-arrays
            byte[][] matchingVariants = (byte[][])DosHeader.MostOftenDosHeaderData.Clone();

            int matchedVariantIndex = -1;
            for (int iByte = 0; iByte < headerBuf.Length; iByte++)
            {
                matchedVariantIndex = -1;
                for (int iMatch = 0; iMatch < matchingVariants.Length; iMatch++)
                {
                    if (matchingVariants[iMatch] == null)
                        continue;

                    if (matchingVariants[iMatch][iByte] == headerBuf[iByte])
                    {
                        if(matchedVariantIndex<0)
                            matchedVariantIndex = iMatch;
                    }
                    else
                    {
                        matchingVariants[iMatch] = null;
                    }
                }

                if (matchedVariantIndex < 0)
                    break;
            }

            if (matchedVariantIndex >= 0)
                return new DosHeader(matchedVariantIndex);
            else
            {
                string constStr = "{ " + string.Join(", ", headerBuf) + "}";

                return new DosHeader { Bytes = headerBuf };
            }
        }
    }
}