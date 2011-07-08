using System;
using System.Collections.Generic;
using System.Linq;

namespace Mi.PE.PEFormat
{
    public sealed class DosHeader
    {
        public struct Reserved10Bytes
        {
            public byte Byte0 { get; set; }
            public byte Byte1 { get; set; }
            public byte Byte2 { get; set; }
            public byte Byte3 { get; set; }
            public byte Byte4 { get; set; }
            public byte Byte5 { get; set; }
            public byte Byte6 { get; set; }
            public byte Byte7 { get; set; }
            public byte Byte8 { get; set; }
            public byte Byte9 { get; set; }
        }
        
        internal sealed class DataBeforePEOffset
        {
            public PESignature Signature;
            public ushort cblp;
            public ushort cp;
            public ushort crlc;
            public ushort cparhdr;
            public ushort minalloc;
            public ushort maxalloc;
            public ushort ss;
            public ushort sp;
            public ushort csum;
            public ushort ip;
            public ushort cs;
            public ushort lfarlc;
            public ushort ovno;
            public ulong res1;
            public ushort oemid;
            public ushort oeminfo;
            public Reserved10Bytes res2;

            public DataBeforePEOffset Clone()
            {
                return new DataBeforePEOffset
                {
                    Signature = this.Signature,
                    cblp = this.cblp,
                    cp = this.cp,
                    crlc = this.crlc,
                    cparhdr = this.cparhdr,
                    minalloc = this.minalloc,
                    maxalloc = this.maxalloc,
                    ss = this.ss,
                    sp = this.sp,
                    csum = this.csum,
                    ip = this.ip,
                    cs = this.cs,
                    lfarlc = this.lfarlc,
                    ovno = this.ovno,
                    res1 =this.res1,
                    oemid = this.oemid,
                    oeminfo = this.oeminfo,
                    res2 = this.res2
                };
            }
        }

        static readonly DataBeforePEOffset DefaultData = new DataBeforePEOffset
        {
            Signature = PESignature.MZ,
            cblp = 144,
            cp  = 3,
            cparhdr = 4,
            maxalloc = ushort.MaxValue,
            sp = 184,
            lfarlc = 64
        };

        DataBeforePEOffset modifiedData;

        public DosHeader()
        {
        }

        /// <summary>
        /// PE signature, expected to be 'MZ'.
        /// </summary>
        public PESignature Signature
        {
            get { return (modifiedData ?? DefaultData).Signature; }
            set
            {
                if (value == this.Signature)
                    return;

                PrepareForModification();
                modifiedData.Signature = value;
            }
        }

        /// <summary>
        /// Bytes on last page of file.
        /// </summary>
        public ushort cblp
        {
            get { return (modifiedData ?? DefaultData).cblp; }
            set
            {
                if (value == this.cblp)
                    return;

                PrepareForModification();
                modifiedData.cblp = value;
            }
        }

        /// <summary>
        /// Pages in file.
        /// </summary>
        public ushort cp
        {
            get { return (modifiedData ?? DefaultData).cp; }
            set
            {
                if (value == this.cp)
                    return;

                PrepareForModification();
                modifiedData.cp = value;
            }
        }

        /// <summary>
        /// Relocations.
        /// </summary>
        public ushort crlc
        {
            get { return (modifiedData ?? DefaultData).crlc; }
            set
            {
                if (value == this.crlc)
                    return;

                PrepareForModification();
                modifiedData.crlc = value;
            }
        }

        /// <summary>
        /// Size of header in paragraphs.
        /// </summary>
        public ushort cparhdr
        {
            get { return (modifiedData ?? DefaultData).cparhdr; }
            set
            {
                if (value == this.cparhdr)
                    return;

                PrepareForModification();
                modifiedData.cparhdr = value;
            }
        }

        /// <summary>
        /// Minimum extra paragraphs needed.
        /// </summary>
        public ushort minalloc
        {
            get { return (modifiedData ?? DefaultData).minalloc; }
            set
            {
                if (value == this.minalloc)
                    return;

                PrepareForModification();
                modifiedData.minalloc = value;
            }
        }

        /// <summary>
        /// Maximum extra paragraphs needed.
        /// </summary>
        public ushort maxalloc
        {
            get { return (modifiedData ?? DefaultData).maxalloc; }
            set
            {
                if (value == this.maxalloc)
                    return;

                PrepareForModification();
                modifiedData.maxalloc = value;
            }
        }

        /// <summary>
        /// Initial (relative) SS value.
        /// </summary>
        public ushort ss
        {
            get { return (modifiedData ?? DefaultData).ss; }
            set
            {
                if (value == this.ss)
                    return;

                PrepareForModification();
                modifiedData.ss = value;
            }
        }

        /// <summary>
        /// Initial SP value.
        /// </summary>
        public ushort sp
        {
            get { return (modifiedData ?? DefaultData).sp; }
            set
            {
                if (value == this.sp)
                    return;

                PrepareForModification();
                modifiedData.sp = value;
            }
        }

        /// <summary>
        /// Checksum.
        /// </summary>
        public ushort csum
        {
            get { return (modifiedData ?? DefaultData).csum; }
            set
            {
                if (value == this.csum)
                    return;

                PrepareForModification();
                modifiedData.csum = value;
            }
        }

        /// <summary>
        /// Initial IP value.
        /// </summary>
        public ushort ip
        {
            get { return (modifiedData ?? DefaultData).ip; }
            set
            {
                if (value == this.ip)
                    return;

                PrepareForModification();
                modifiedData.ip = value;
            }
        }

        /// <summary>
        /// Initial (relative) CS value.
        /// </summary>
        public ushort cs
        {
            get { return (modifiedData ?? DefaultData).cs; }
            set
            {
                if (value == this.cs)
                    return;

                PrepareForModification();
                modifiedData.cs = value;
            }
        }

        /// <summary>
        /// File address of relocation table.
        /// </summary>
        public ushort lfarlc
        {
            get { return (modifiedData ?? DefaultData).lfarlc; }
            set
            {
                if (value == this.lfarlc)
                    return;

                PrepareForModification();
                modifiedData.lfarlc = value;
            }
        }

        /// <summary>
        /// Overlay number.
        /// </summary>
        public ushort ovno
        {
            get { return (modifiedData ?? DefaultData).ovno; }
            set
            {
                if (value == this.ovno)
                    return;

                PrepareForModification();
                modifiedData.ovno = value;
            }
        }

        /// <summary>
        /// Reserved words.
        /// </summary>
        public ulong res1
        {
            get { return (modifiedData ?? DefaultData).res1; }
            set
            {
                if (value == this.res1)
                    return;

                PrepareForModification();
                modifiedData.res1 = value;
            }
        }

        /// <summary>
        /// OEM identifier (for e_oeminfo).
        /// </summary>
        public ushort oemid
        {
            get { return (modifiedData ?? DefaultData).oemid; }
            set
            {
                if (value == this.oemid)
                    return;

                PrepareForModification();
                modifiedData.oemid = value;
            }
        }

        /// <summary>
        /// OEM information; e_oemid specific.
        /// </summary>
        public ushort oeminfo
        {
            get { return (modifiedData ?? DefaultData).oeminfo; }
            set
            {
                if (value == this.oeminfo)
                    return;

                PrepareForModification();
                modifiedData.oeminfo = value;
            }
        }

        public Reserved10Bytes res2
        {
            get { return (modifiedData ?? DefaultData).res2; }
            set
            {
                if (Equals(value, this.res2))
                    return;

                PrepareForModification();
                modifiedData.res2 = value;
            }
        }

        public uint lfanew { get; set; }


        void PrepareForModification()
        {
            if (modifiedData != null)
                return;

            var copy = DefaultData.Clone();
            System.Threading.Interlocked.CompareExchange(ref modifiedData, copy, null);
        }
    }
}