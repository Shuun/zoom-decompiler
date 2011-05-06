using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace ILSpySL.Model
{
    using ICSharpCode.Decompiler;

    public sealed class AssemblyNamespaceIsland
    {
        readonly AssemblyIsland m_Assembly;
        readonly AssemblyNamespaceIsland m_ParentNamespace;
        readonly string m_Name;
        readonly ReadOnlyCollection<TypeIsland> m_Types;

        public AssemblyNamespaceIsland(
            AssemblyIsland assembly, AssemblyNamespaceIsland parentNamespace, string name,
            Func<AssemblyNamespaceIsland, IEnumerable<TypeIsland>> types)
        {
            this.m_Assembly = assembly;
            this.m_ParentNamespace = parentNamespace;
            this.m_Name = name;

            this.m_Types = types(this).ToReadOnlyCollectionOrNull();
        }

        public AssemblyIsland Assembly { get { return this.m_Assembly; } }
        public AssemblyNamespaceIsland ParentNamespace { get { return this.m_ParentNamespace; } }
        public string Name { get { return this.m_Name; } }
        public ReadOnlyCollection<TypeIsland> Types { get { return this.m_Types ?? Empty.ReadOnlyCollection<TypeIsland>(); }}
    }
}