using System;
using System.Collections;
using System.Linq;

namespace Mi.PE.Internal.Compression
{
    public abstract class CompressedByteChunk
    {
        public readonly int TotalSize;
        protected readonly CompressedByteChunk Lead;

        protected CompressedByteChunk(int chunkSize, CompressedByteChunk lead)
        {
            this.TotalSize = chunkSize + (lead == null ? 0 : lead.TotalSize);
            this.Lead = lead;
        }

        public byte[] Decompress()
        {
            byte[] result = new byte[TotalSize];
            int offset = result.Length;

            var writeChunk = this;
            do
            {
                offset -= writeChunk.Lead == null ? 0 : this.TotalSize - Lead.TotalSize;

                writeChunk.DecompressChunkTo(result, offset);

                writeChunk = writeChunk.Lead;

            } while (writeChunk != null);

            return result;
        }

        protected abstract void DecompressChunkTo(byte[] buffer, int offset);

        public abstract CompressedByteChunk CompressNext(byte b);
    }
}