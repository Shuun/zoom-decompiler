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
    internal static class SampleInputLoader
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

        static readonly AssemblyDefinition Mscorlib = AssemblyDefinition.ReadAssembly(
            new MemoryStream(SampleInputFiles.mscorlib));
        static readonly AssemblyDefinition SystemCore = AssemblyDefinition.ReadAssembly(
            new MemoryStream(SampleInputFiles.System_Core));
        static readonly AssemblyDefinition System = AssemblyDefinition.ReadAssembly(
            new MemoryStream(SampleInputFiles.system));

        public static AssemblyDefinition LoadAssembly(string assembly)
        {
            var bytes = (byte[])SampleInputFiles.ResourceManager.GetObject(assembly);

            if (bytes == null)
                throw new FileNotFoundException("Assembly '"+assembly+"' was not found in resources.");

            var dllStream = new MemoryStream(bytes);
            MemoryStream pdbStream;
            try
            {
                pdbStream = new MemoryStream((byte[])SampleInputFiles.ResourceManager.GetObject(assembly + "_pdb"));
            }
            catch
            {
                pdbStream = null;
            }

            var para = new ReaderParameters(ReadingMode.Immediate)
            {
                AssemblyResolver = new Resolver((asmRef, _rp) =>
                    asmRef.Name == Mscorlib.Name.Name ? Mscorlib :
                    asmRef.Name == SystemCore.Name.Name ? SystemCore :
                    asmRef.Name == System.Name.Name ? System :
                    null),
                SymbolReaderProvider = pdbStream == null ? null : new Mi.Assemblies.Pdb.PdbReaderProvider(),
                SymbolStream = pdbStream,
                ReadSymbols = true
            };

            var result = AssemblyDefinition.ReadAssembly(
                dllStream,
                para);

            return result;
        }
    }
}
