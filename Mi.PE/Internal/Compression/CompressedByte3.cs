using System;
using System.Collections;
using System.Linq;

namespace Mi.PE.Internal.Compression
{
    public sealed class CompressedByte3 : CompressedByteChunk
    {
        readonly byte b1;
        readonly byte b2;
        readonly byte b3;

        public CompressedByte3(byte b1, byte b2, byte b3, CompressedByteChunk lead)
            : base(3, lead)
        {
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
        }

        protected override void DecompressChunkTo(byte[] buffer, int offset)
        {
            buffer[offset - 2] = b1;
            buffer[offset - 1] = b2;
            buffer[offset] = b3;
        }

        public override CompressedByteChunk CompressNext(byte b)
        {
            return new CompressedByte4(this.b1, this.b2, this.b3, b, this.Lead);
        }
    }
}