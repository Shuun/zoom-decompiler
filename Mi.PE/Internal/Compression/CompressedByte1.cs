using System;
using System.Collections;
using System.Linq;

namespace Mi.PE.Internal.Compression
{
    public sealed class CompressedByte1 : CompressedByteChunk
    {
        readonly byte b;

        public CompressedByte1(byte b, CompressedByteChunk lead)
            : base(1, lead)
        {
            this.b = b;
        }

        protected override void DecompressChunkTo(byte[] buffer, int offset)
        {
            buffer[offset] = b;
        }

        public override CompressedByteChunk CompressNext(byte b)
        {
            return new CompressedByte2(this.b, b, this.Lead);
        }
    }
}