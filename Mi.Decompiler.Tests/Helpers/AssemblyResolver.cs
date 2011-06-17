using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mi.Assemblies;
using System.Reflection;

namespace Mi.Decompiler.Tests.Helpers
{
    internal sealed class AssemblyResolver : IAssemblyResolver
    {
        readonly Dictionary<string, AssemblyDefinition> resolutionCache = new Dictionary<string, AssemblyDefinition>();

        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            return Resolve(name, null);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            lock (resolutionCache)
            {
                string key = name.FullName;

                AssemblyDefinition result;
                if (resolutionCache.TryGetValue(key, out result))
                    return result;
            }

            if (!name.Name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                && !name.Name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase))
                return null;

            var asm = Assembly.Load(name.ToString());
            var asmDef = AssemblyDefinition.ReadAssembly(
                asm.Location,
                parameters ??
                new ReaderParameters(ReadingMode.Deferred) { AssemblyResolver = this });

            lock (resolutionCache)
            {
                string key = name.FullName;

                AssemblyDefinition result;
                if (resolutionCache.TryGetValue(key, out result))
                    return result;

                resolutionCache[key] = asmDef;
            }

            return asmDef;
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return Resolve(AssemblyNameReference.Parse(fullName));
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            return Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }
    }
}
