using System;
using System.Collections;
using System.Linq;

namespace Mi.PE.Internal.Compression
{
    public sealed class CompressedByte2 : CompressedByteChunk
    {
        readonly byte b1;
        readonly byte b2;

        public CompressedByte2(byte b1, byte b2, CompressedByteChunk lead)
            : base(2, lead)
        {
            this.b1 = b1;
            this.b2 = b2;
        }

        protected override void DecompressChunkTo(byte[] buffer, int offset)
        {
            buffer[offset - 1] = b1;
            buffer[offset] = b2;
        }

        public override CompressedByteChunk CompressNext(byte b)
        {
            return new CompressedByte3(this.b1, this.b2, b, this.Lead);
        }
    }
}