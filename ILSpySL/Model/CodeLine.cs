using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

using ICSharpCode.Decompiler;

namespace ILSpySL.Model
{
    public sealed class CodeLine
    {
        readonly ReadOnlyCollection<string> m_Parts;
        readonly int m_Length;

        public CodeLine(IEnumerable<string> parts, int length)
        {
            this.m_Parts = parts.ToReadOnlyCollectionOrNull();
            this.m_Length = length;
        }

        public ReadOnlyCollection<string> Parts { get { return this.m_Parts ?? Empty.ReadOnlyCollection<string>(); } }
        public int Length { get { return this.m_Length; } }
    }
}