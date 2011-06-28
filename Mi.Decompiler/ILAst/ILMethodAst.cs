using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Mi.Decompiler.ILAst;

namespace Mi.Decompiler.ILAst
{
    public sealed class ILMethodAst
    {
        public static readonly ILMethodAst Empty = new ILMethodAst(null, null);

        readonly ReadOnlyCollection<ILVariable> m_Parameters;
        readonly ReadOnlyCollection<ILNode> m_Nodes;

        public ILMethodAst(IEnumerable<ILVariable> parameters, IEnumerable<ILNode> nodes)
        {
            this.m_Parameters = parameters.ToReadOnlyCollectionOrNull();
            this.m_Nodes = nodes.ToReadOnlyCollectionOrNull();
        }

        public ReadOnlyCollection<ILVariable> Parameters { get { return m_Parameters ?? Mi.Empty.ReadOnlyCollection<ILVariable>(); } }
        public ReadOnlyCollection<ILNode> Nodes { get { return m_Nodes ?? Mi.Empty.ReadOnlyCollection<ILNode>(); } }
    }
}