using System;
using System.Collections.Generic;
using System.Linq;

namespace Mi.PE.PEFormat
{
    public sealed class DosHeader
    {
        internal static readonly byte[][] MostOftenDosHeaderData = new byte[][]
        {
            new byte [] { 144, 0, 3, 0, 0, 0, 4, 0, 0, 0, 255, 255, 0, 0, 184, 0, 0, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        byte[] m_Bytes;
        object knownVariant;

        public DosHeader()
        {
            this.knownVariant = null;
        }

        internal DosHeader(int knownVariant)
        {
            this.knownVariant = knownVariant;
        }

        public byte[] Bytes
        {
            get
            {
                object knownVariant = this.knownVariant;
                if (knownVariant != null)
                {
                    lock (knownVariant)
                    {
                        if (this.knownVariant == null)
                            return m_Bytes;

                        this.m_Bytes = MostOftenDosHeaderData[(int)this.knownVariant];
                        this.knownVariant = null;
                    }
                }

                return m_Bytes;
            }

            set
            {
                this.knownVariant = null;
                m_Bytes = value;
            }
        }
    }
}