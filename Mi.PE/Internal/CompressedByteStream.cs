using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Mi.PE.Internal
{
    public static class CompressedByteStream
    {
        private sealed class CompressedChunk
        {
            public readonly int Size;
            readonly Action<byte[], int> chunkWriteTo;
            readonly CompressedChunk previousChunk;

            public CompressedChunk(int size, Action<byte[], int> writeTo)
            {
                this.Size = size;
                this.chunkWriteTo = writeTo;
            }

            public CompressedChunk(int size, Action<byte[], int> writeTo, CompressedChunk previous)
            {
                this.Size = size + previous.Size;
                this.chunkWriteTo = writeTo;
                this.previousChunk = previous;
            }

            public void WriteTo(byte[] buffer, int offset)
            {
                var writeChunk = this;
                do
                {
                    int leadingSize =writeChunk.previousChunk == null ? 0 : writeChunk.previousChunk.Size;

                    chunkWriteTo(buffer, offset + leadingSize);

                    writeChunk = writeChunk.previousChunk;

                } while (writeChunk != null);
            }
        }

        public static IEnumerable<Func<byte[]>> Compress(Func<byte> getByte)
        {
            throw new NotImplementedException();
        }
    }
}