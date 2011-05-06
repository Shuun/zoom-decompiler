using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

using Mono.Cecil;

using ICSharpCode.Decompiler;

namespace ILSpySL.Services
{
    using ILSpySL.Model;

    public static class PopulateAssemblyService
    {
        public static AssemblyIsland PopulateAssembly(
            AssemblyDefinition assembly,
            Func<string,string> internString)
        {
            //var ctx = new DecompilerContext(ty.Module);
            //var astBui = new AstBuilder(ctx);
            //astBui.AddType(ty);
            //astBui.RunTransformations();
            //var outp = new RichTextOutput();
            //astBui.GenerateCode(outp);

            var grouped =
                from m in assembly.Modules
                from t in m.Types
                group t by t.Namespace into nsTypes
                orderby nsTypes.Key
                select new
                {
                    FullNamespace = nsTypes.Key,
                    Types = nsTypes.ToList()
                };

            return new AssemblyIsland(
                internString(assembly.FullName),
                internString(assembly.Name.Name),
                assembly.Name.Version,
                asmIsland => GetAssemblyNamespaces(assembly),
                CalcAssemblySize,
                asmIsland => default(Point));
        }

        static IEnumerable<AssemblyNamespaceIsland> GetAssemblyNamespaces(AssemblyDefinition assembly)
        {
            throw new NotImplementedException();
        }

        static Size CalcAssemblySize(AssemblyIsland assembly)
        {
            throw new NotImplementedException();
        }
    }
}
