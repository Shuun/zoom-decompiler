using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mi.Scope.Model
{
    public sealed class TypeIsland
    {
        readonly AssemblyNamespaceIsland m_Namespace;
        readonly string m_Name;
        readonly ReadOnlyCollection<CodeLine> m_Lines;
        readonly Point m_RelativePosition;

        public TypeIsland(
            AssemblyNamespaceIsland @namespace, string name, IEnumerable<CodeLine> lines,
            Func<TypeIsland, Point> getRelativePosition)
        {
            this.m_Namespace = @namespace;
            this.m_Name = name;
            this.m_Lines = lines.ToReadOnlyCollectionOrNull();
            this.m_RelativePosition = getRelativePosition(this);
        }

        public AssemblyNamespaceIsland Namespace { get { return this.m_Namespace; } }
        public string Name { get { return this.m_Name; } }
        public ReadOnlyCollection<CodeLine> Lines { get { return this.m_Lines ?? Empty.ReadOnlyCollection<CodeLine>(); } }
        public Point RelativePosition { get { return this.m_RelativePosition; } }
    }
}