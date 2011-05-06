using System;
using System.Collections.Generic;
using System.Linq;

namespace ILSpySL.Model
{
    public sealed class TypeIsland
    {
        readonly AssemblyNamespaceIsland m_Namespace;
        readonly string m_Name;

        public TypeIsland(AssemblyNamespaceIsland @namespace, string name)
        {
            this.m_Namespace = @namespace;
            this.m_Name = name;
        }

        public AssemblyNamespaceIsland Namespace { get { return this.m_Namespace; } }
        public string Name { get { return this.m_Name; } }
    }
}