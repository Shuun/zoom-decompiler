using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Mi.PE.Internal
{
    public static class StreamExtensions
    {
        static string DiagnosticByteValue(byte b)
        {
            if ((b >= '0' && b <= '9')
                || (b >= 'A' && b <= 'Z')
                || (b >= 'a' && b <= 'z'))
                return "'" + (char)b + "'";
            else if (b == (byte)'\r')
                return "<CR>";
            else if (b == (byte)'\n')
                return "<LF>";
            else
                return b.ToString("X") + "h";
        }

        public static void CheckedExpect(this Stream stream, byte value, string errorMessageState)
        {
            byte b = stream.CheckedReadByte(errorMessageState);
            if (b != value)
                throw new BadImageFormatException(
                    "Incorrect byte "+DiagnosticByteValue(b)+
                    " instead of expected "+DiagnosticByteValue(value)+
                    " when "+errorMessageState+".");
        }

        public static void CheckedExpect(this Stream stream, byte byte1, byte byte2, string errorMessageState)
        {
            short b1 = stream.CheckedReadByte(errorMessageState);
            byte b2 = stream.CheckedReadByte(errorMessageState);
            if (b1 != byte1 || b2 != byte2)
                throw new BadImageFormatException("Incorrect data when " + errorMessageState + ".");
        }

        public static void CheckedExpect(this Stream stream, byte byte1, byte byte2, byte byte3, byte byte4, string errorMessageState)
        {
            byte b1 = stream.CheckedReadByte(errorMessageState);
            byte b2 = stream.CheckedReadByte(errorMessageState);
            byte b3 = stream.CheckedReadByte(errorMessageState);
            byte b4 = stream.CheckedReadByte(errorMessageState);

            if (b1 != byte1 || b2 != byte2)
                throw new BadImageFormatException("Incorrect data when " + errorMessageState + ".");
        }

        public static void CheckedSkipBytes(this Stream stream, long byteCount, string eofErrorMessageState)
        {
            if (stream.CanSeek)
            {
                stream.Seek(byteCount, SeekOrigin.Current);
                return;
            }

            for (int i = 0; i < byteCount; i++)
            {
                stream.CheckedReadByte(eofErrorMessageState);
            }
        }

        public static byte CheckedReadByte(this Stream stream, string eofErrorMessageState)
        {
            int value = stream.ReadByte();
            if (value < 0)
                throw new EndOfStreamException("Unexpected end of file whilst "+eofErrorMessageState+".");

            unchecked
            {
                return (byte)value;
            }
        }

        public static short CheckedReadInt16(this Stream stream, string eofErrorMessageState)
        {
            byte b0 = stream.CheckedReadByte(eofErrorMessageState);
            byte b1 = stream.CheckedReadByte(eofErrorMessageState);

            unchecked
            {
                return (short)(b0 | (b1 << 8));
            }            
        }

        public static ushort CheckedReadUInt16(this Stream stream, string eofErrorMessageState)
        {
            unchecked
	        {
                return (ushort)stream.CheckedReadInt16(eofErrorMessageState);
	        }            
        }

        public static int CheckedReadInt32(this Stream stream, string eofErrorMessageState)
        {
            byte b0 = stream.CheckedReadByte(eofErrorMessageState);
            byte b1 = stream.CheckedReadByte(eofErrorMessageState);
            byte b2 = stream.CheckedReadByte(eofErrorMessageState);
            byte b3 = stream.CheckedReadByte(eofErrorMessageState);

            unchecked
            {
                return
                    b0 |
                    (b1 << 8) |
                    (b2 << 16) |
                    (b3 << 24);
            }
        }

        public static uint CheckedReadUInt32(this Stream stream, string eofErrorMessageState)
        {
            unchecked
            {
                return (uint)CheckedReadInt32(stream, eofErrorMessageState);
            }
        }

        public static string CheckedReadFixedString(this Stream stream, int characterCount, string eofErrorMessageState)
        {
            char[] result = new char[characterCount];
            int size = 0;
            for (int i = 0; i < result.Length; i++)
            {
                char readChar = result[i] = (char)stream.CheckedReadByte(eofErrorMessageState);

                if (readChar != (char)0)
                    size = i + 1;
            }

            return size == 0 ? string.Empty : new string(result, 0, size);
        }

        public static long CheckedReadInt64(this Stream stream, string eofErrorMessageState)
        {
            byte b0 = stream.CheckedReadByte(eofErrorMessageState);
            byte b1 = stream.CheckedReadByte(eofErrorMessageState);
            byte b2 = stream.CheckedReadByte(eofErrorMessageState);
            byte b3 = stream.CheckedReadByte(eofErrorMessageState);
            byte b4 = stream.CheckedReadByte(eofErrorMessageState);
            byte b5 = stream.CheckedReadByte(eofErrorMessageState);
            byte b6 = stream.CheckedReadByte(eofErrorMessageState);
            byte b7 = stream.CheckedReadByte(eofErrorMessageState);

            unchecked
            {
                return
                    (long)b0 |
                    ((long)b1 << 8) |
                    ((long)b2 << 16) |
                    ((long)b3 << 24) |
                    ((long)b4 << 32) |
                    ((long)b5 << 40) |
                    ((long)b6 << 48) |
                    ((long)b7 << 56);
            }
        }

        public static ulong CheckedReadUInt64(this Stream stream, string eofErrorMessageState)
        {
            unchecked
            {
                return (ulong)CheckedReadInt64(stream, eofErrorMessageState);
            }
        }

        public static void CheckedReadBytes(this Stream stream, byte[] buffer, int offset, int count, string eofErrorMessageState)
        {
            int readCount = 0;
            while (readCount < count)
            {
                int chunk = stream.Read(buffer, offset, count - readCount);
                if (chunk <= 0)
                    throw new EndOfStreamException("Unexpected end of file whilst " + eofErrorMessageState + ".");

                readCount += chunk;
            }
        }

        public static void CheckedReadBytes(this Stream stream, byte[] buffer, string eofErrorMessageState)
        {
            stream.CheckedReadBytes(buffer, 0, buffer.Length, eofErrorMessageState);
        }
    }
}