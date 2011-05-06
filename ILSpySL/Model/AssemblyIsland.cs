using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace ILSpySL.Model
{
    using ICSharpCode.Decompiler;

    public sealed class AssemblyIsland
    {
        readonly string m_FullName;
        readonly string m_Name;
        readonly Version m_Version;
        readonly ReadOnlyCollection<AssemblyNamespaceIsland> m_Namespaces;

        public AssemblyIsland(
            string fullName, string name, Version version,
            Func<AssemblyIsland, IEnumerable<AssemblyNamespaceIsland>> namespaces)
        {
            this.m_FullName = fullName;
            this.m_Name = name;
            this.m_Version = version;
            this.m_Namespaces = namespaces(this).ToReadOnlyCollectionOrNull();
        }

        public string FullName { get { return this.m_FullName; } }
        public string Name { get { return this.m_Name; } }
        public Version Version { get { return m_Version; } }
        public ReadOnlyCollection<AssemblyNamespaceIsland> Namespaces { get { return this.m_Namespaces ?? Empty.ReadOnlyCollection<AssemblyNamespaceIsland>(); } }
    }
}