using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

using ICSharpCode.Decompiler;

namespace ILSpySL.Model
{
    public sealed class AssemblyIsland
    {
        readonly string m_FullName;
        readonly string m_Name;
        readonly Version m_Version;
        readonly ReadOnlyCollection<AssemblyNamespaceIsland> m_Namespaces;
        readonly Size m_DesiredSize;
        readonly Point m_RelativePosition;

        public AssemblyIsland(
            string fullName, string name, Version version,
            Func<AssemblyIsland, IEnumerable<AssemblyNamespaceIsland>> namespaces,
            Func<AssemblyIsland, Size> getDesiredSize,
            Func<AssemblyIsland, Point> getRelativePosition)
        {
            this.m_FullName = fullName;
            this.m_Name = name;
            this.m_Version = version;

            this.m_Namespaces = namespaces(this).ToReadOnlyCollectionOrNull();

            this.m_DesiredSize = getDesiredSize(this);

            this.m_RelativePosition = getRelativePosition(this);
        }

        public string FullName { get { return this.m_FullName; } }
        public string Name { get { return this.m_Name; } }
        public Version Version { get { return m_Version; } }
        public ReadOnlyCollection<AssemblyNamespaceIsland> Namespaces { get { return this.m_Namespaces ?? Empty.ReadOnlyCollection<AssemblyNamespaceIsland>(); } }
        public Size DesiredSize { get { return this.m_DesiredSize; } }
        public Point RelativePosition { get { return this.m_RelativePosition; } }
    }
}