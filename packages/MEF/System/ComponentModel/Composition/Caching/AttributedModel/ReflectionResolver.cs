// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Caching
{
    internal static class ReflectionResolver
    {
        public static Assembly ResolveAssembly(string assemblyName, string codeBase)
        {
            Assumes.NotNullOrEmpty(assemblyName);

#if !SILVERLIGHT
            AssemblyName name = new AssemblyName(assemblyName);
            name.CodeBase = codeBase;
            return Assembly.Load(name);
#else
            // Assembly.Load(AssemblyName) is marked [SecurityCritical] 
            // on Silverlight, so need to call Assembly.Load(String) instead.
            Assumes.Null(codeBase);
            return Assembly.Load(assemblyName);
#endif
        }
        
        public static Module ResolveModule(Assembly assembly, int metadataToken)
        {
            Assumes.NotNull(assembly);
            Assumes.IsTrue(metadataToken != 0);

            // TODO: This likely cause all modules in the assembly to be loaded
            // perhaps we should load via file name (how will that work on SL)?
            foreach (Module module in assembly.GetModules())
            {
                if (module.MetadataToken == metadataToken)
                {
                    return module;
                }
            }

            Assumes.Fail("");
            return null;
        }

#if !SILVERLIGHT
        public static MemberInfo ResolveMember(Module module, int metadataToken)
        {
            Assumes.NotNull(module);
            Assumes.IsTrue(metadataToken != 0);

            return module.ResolveMember(metadataToken);
        }
#else
        public static MemberInfo ResolveMember(Module module, int metadataToken)
        {
            // WORKAROUND: Silverlight is missing the Module.ResolveMember methods 
            // so until they have been added, on that platform we need to determine 
            // what kind of token we are looking at, and then call the associated
            // Module.ResolveXXX methods. Unfortunately, Module.ResolveField also
            // doesn't exist, so when looking at field when we need to walk every 
            // field in the module - not a very ideal solution.

            Assumes.NotNull(module);
            Assumes.IsTrue(metadataToken != 0);

            switch ((MetadataTokenType)(metadataToken) & MetadataTokenType.Mask)
            {
                case MetadataTokenType.MemberRef:
                case MetadataTokenType.MethodDef:
                case MetadataTokenType.MethodSpec:
                    return module.ResolveMethod(metadataToken);
                
                case MetadataTokenType.TypeDef:
                case MetadataTokenType.TypeRef:
                case MetadataTokenType.TypeSpec:
                    return module.ResolveType(metadataToken);

                case MetadataTokenType.FieldDef:
                    return WORKAROUND_ResolveField(module, metadataToken);

                default:
                    Assumes.Fail("");
                    return null;
            }
        }

        private static FieldInfo WORKAROUND_ResolveField(Module module, int metadataToken)
        {
            // TODO: Need to replace this with Module.ResolveMember when it has been added to Silverlight

            foreach (Type type in module.GetTypes())
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static |
                                                           BindingFlags.DeclaredOnly |
                                                           BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (field.MetadataToken == metadataToken)
                    {
                        return field;
                    }
                }
            }

            Assumes.Fail("");
            return null;
        }

        // Taken from ndp\clr\src\BCL\System\Reflection\MdImport.cs
        private enum MetadataTokenType
        {
            Module = 0x00000000,
            TypeRef = 0x01000000,
            TypeDef = 0x02000000,
            FieldDef = 0x04000000,
            MethodDef = 0x06000000,
            ParamDef = 0x08000000,
            InterfaceImpl = 0x09000000,
            MemberRef = 0x0a000000,
            CustomAttribute = 0x0c000000,
            Permission = 0x0e000000,
            Signature = 0x11000000,
            Event = 0x14000000,
            Property = 0x17000000,
            ModuleRef = 0x1a000000,
            TypeSpec = 0x1b000000,
            Assembly = 0x20000000,
            AssemblyRef = 0x23000000,
            File = 0x26000000,
            ExportedType = 0x27000000,
            ManifestResource = 0x28000000,
            GenericPar = 0x2a000000,
            MethodSpec = 0x2b000000,
            String = 0x70000000,
            Name = 0x71000000,
            BaseType = 0x72000000,
            Invalid = 0x7FFFFFFF,
            Mask = unchecked((int)0xFF000000),
        }
#endif

    }
}
