using System;
using System.Collections;
using System.Linq;

namespace Mi.PE.Internal.Compression
{
    public sealed class CompressedByte4 : CompressedByteChunk
    {
        readonly byte b1;
        readonly byte b2;
        readonly byte b3;
        readonly byte b4;

        public CompressedByte4(byte b1, byte b2, byte b3, byte b4, CompressedByteChunk lead)
            : base(4, lead)
        {
            this.b1 = b1;
            this.b2 = b2;
            this.b3 = b3;
            this.b4 = b4;
        }

        protected override void DecompressChunkTo(byte[] buffer, int offset)
        {
            buffer[offset - 3] = b1;
            buffer[offset - 2] = b2;
            buffer[offset - 1] = b3;
            buffer[offset] = b4;
        }

        public override CompressedByteChunk CompressNext(byte b)
        {
            throw new NotImplementedException();
        }
    }
}