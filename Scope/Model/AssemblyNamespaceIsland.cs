using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

using Mi;

namespace Mi.Zoom.Model
{
    using Mi.Decompiler;

    public sealed class AssemblyNamespaceIsland
    {
        readonly AssemblyIsland m_Assembly;
        readonly AssemblyNamespaceIsland m_ParentNamespace;
        readonly string m_Name;
        readonly ReadOnlyCollection<TypeIsland> m_Types;
        readonly Size m_DesiredSize;
        readonly Point m_RelativePosition;

        public AssemblyNamespaceIsland(
            AssemblyIsland assembly, AssemblyNamespaceIsland parentNamespace, string name,
            Func<AssemblyNamespaceIsland, IEnumerable<TypeIsland>> getTypes,
            Func<AssemblyNamespaceIsland, Size> getDesiredSize,
            Func<AssemblyNamespaceIsland, Point> getRelativePosition)
        {
            this.m_Assembly = assembly;
            this.m_ParentNamespace = parentNamespace;
            this.m_Name = name;

            this.m_Types = getTypes(this).ToReadOnlyCollectionOrNull();

            this.m_DesiredSize = getDesiredSize(this);

            this.m_RelativePosition = getRelativePosition(this);
        }

        public AssemblyIsland Assembly { get { return this.m_Assembly; } }
        public AssemblyNamespaceIsland ParentNamespace { get { return this.m_ParentNamespace; } }
        public string Name { get { return this.m_Name; } }
        public ReadOnlyCollection<TypeIsland> Types { get { return this.m_Types ?? Empty.ReadOnlyCollection<TypeIsland>(); } }
        public Size DesiredSize { get { return this.m_DesiredSize; } }
        public Point RelativePosition { get { return this.m_RelativePosition; } }
    }
}