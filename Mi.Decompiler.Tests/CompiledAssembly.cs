using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mi.Decompiler.Ast;
using Mi.Decompiler.Tests.Helpers;
using Mi.Assemblies;

namespace Mi.Decompiler.Tests
{
    internal static class CompiledAssembly
    {
        private sealed class Resolver : IAssemblyResolver
        {
            readonly Func<AssemblyNameReference, ReaderParameters, AssemblyDefinition> resolve;

            public Resolver(Func<AssemblyNameReference, ReaderParameters, AssemblyDefinition> resolve)
            {
                this.resolve = resolve;
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                return Resolve(name, null);
            }

            public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                return this.resolve(name, parameters);
            }

            public AssemblyDefinition Resolve(string fullName)
            {
                return Resolve(fullName, null);
            }

            public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
            {
                return Resolve(AssemblyNameReference.Parse(fullName), parameters);
            }
        }

        static readonly Lazy<AssemblyDefinition> m_Assembly = new Lazy<AssemblyDefinition>(LoadAssembly);
        public static AssemblyDefinition Assembly { get { return m_Assembly.Value; } }

        static AssemblyDefinition LoadAssembly()
        {
            var mscorlib = AssemblyDefinition.ReadAssembly(
                new MemoryStream(SampleInputAssemblyFiles.mscorlib));
            var systemCore = AssemblyDefinition.ReadAssembly(
                new MemoryStream(SampleInputAssemblyFiles.System_Core));

            var rp = new ReaderParameters(ReadingMode.Immediate);
            rp.AssemblyResolver = new Resolver((asmRef, _rp) => asmRef.Name == mscorlib.Name.Name ? mscorlib : systemCore);
            rp.SymbolReaderProvider = new Mi.Assemblies.Pdb.PdbReaderProvider();
            rp.ReadSymbols = true;
            rp.SymbolStream = new MemoryStream(SampleInputAssemblyFiles.SampleInputAssembly_pdb);

            var result = AssemblyDefinition.ReadAssembly(
                new MemoryStream(SampleInputAssemblyFiles.SampleInputAssembly),
                rp);

            return result;
        }
    }
}
