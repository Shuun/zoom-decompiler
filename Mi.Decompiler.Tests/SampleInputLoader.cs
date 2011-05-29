using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mi.Decompiler.AstServices;
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

        static readonly List<AssemblyDefinition> assemblyCache = new List<AssemblyDefinition>();

        public static AssemblyDefinition LoadAssembly(string assembly)
        {
            var fromCache = assemblyCache.FirstOrDefault(
                            loadedAsm => string.Equals(
                                assembly.Replace(".", ""),
                                loadedAsm.Name.Name.Replace(".",""),
                                StringComparison.OrdinalIgnoreCase));

            if (fromCache != null)
                return fromCache;

            var bytes = (byte[])SampleInputFiles.ResourceManager.GetObject(assembly.Replace(".", ""));

            if (bytes == null)
                throw new FileNotFoundException("Assembly '"+assembly+"' was not found in resources.");

            var dllStream = new MemoryStream(bytes);
            MemoryStream pdbStream;
            {
                var pdbBytes = (byte[])SampleInputFiles.ResourceManager.GetObject(assembly + "_pdb");
                if(pdbBytes==null)
                    pdbStream = null;
                else
                    pdbStream = new MemoryStream(pdbBytes);
            }

            var para = new ReaderParameters(ReadingMode.Immediate)
            {
                AssemblyResolver = new Resolver((asmRef, _rp) => LoadAssembly(asmRef.Name)),
                SymbolReaderProvider = pdbStream == null ? null : new Mi.Assemblies.Pdb.PdbReaderProvider(),
                SymbolStream = pdbStream,
                ReadSymbols = true
            };

            var result = AssemblyDefinition.ReadAssembly(
                dllStream,
                para);

            assemblyCache.Add(result);

            return result;
        }
    }
}
