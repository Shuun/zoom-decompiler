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

        public static readonly AssemblyDefinition Mscorlib;
        public static readonly AssemblyDefinition SystemCore;
        public static readonly AssemblyDefinition Assembly;

        static CompiledAssembly()
        {
            Mscorlib = AssemblyDefinition.ReadAssembly(
                new MemoryStream(SampleInputAssemblyFiles.mscorlib));
            SystemCore = AssemblyDefinition.ReadAssembly(
                new MemoryStream(SampleInputAssemblyFiles.System_Core));

            var rp = new ReaderParameters(ReadingMode.Immediate);
            rp.AssemblyResolver = new Resolver((asmRef, _rp) => asmRef.Name == Mscorlib.Name.Name ? Mscorlib : SystemCore);

            Assembly = AssemblyDefinition.ReadAssembly(
                new MemoryStream(SampleInputAssemblyFiles.SampleInputAssembly),
                rp);
        }
    }
}
