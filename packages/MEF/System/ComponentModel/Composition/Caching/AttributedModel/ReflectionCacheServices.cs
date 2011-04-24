// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Internal;
using System.ComponentModel.Composition.ReflectionModel;
using System.Threading;

namespace System.ComponentModel.Composition.Caching.AttributedModel
{
    internal static class ReflectionCacheServices
    {
        public static void WriteLazyAccessors(this IDictionary<string, object> cache, MemberInfo[] accessors, LazyInit<Type> defaultType)
        {
            cache.Write((c, a) => WriteAccessorsCore(c, a, defaultType.Value), AttributedCacheServices.CacheKeys.Accessors, accessors);
        }

        public static void WriteLazyParameter(this IDictionary<string, object> cache, LazyInit<ParameterInfo> parameter, LazyInit<Type> defaultType)
        {
            cache.WriteParameter(parameter.Value, defaultType.Value);
        }

        public static void WriteLazyTypeForPart(this IDictionary<string, object> cache, LazyInit<Type> type)
        {
            cache.WriteMember(AttributedCacheServices.CacheKeys.PartType, type.Value, null);
        }

        private static void WriteParameter(this IDictionary<string, object> cache, ParameterInfo parameter, Type defaultType)
        {
            cache.Write((c, p) => WriteParameterCore(c, p, defaultType), AttributedCacheServices.CacheKeys.Parameter, parameter);
        }
        
        private static void WriteMember(this IDictionary<string, object> cache, string key, MemberInfo member, Type defaultType)
        {
            if (member != null)
            {
                // only write the member as a dictionary if we write the module and assembly as well
                if (
                    (defaultType != null) &&
                    (defaultType.Assembly == member.Module.Assembly) &&
                    (member.Module == member.Module.Assembly.ManifestModule))
                {
                    cache.WriteValue(key, member.MetadataToken);
                }
                else
                {
                    cache.Write((c, m) => WriteMemberCore(c, m, defaultType), key, member);
                }
            }
            else
            {
                cache.WriteValue<MemberInfo>(key, null, typeof(object)); // we want nulls written, so we need to pass the default member such that we would never match it
            }
        }

        private static void WriteModule(this IDictionary<string, object> cache, Module module)
        {
            // We only store the module in the case where it is not the default
            cache.Write(WriteModuleCore, AttributedCacheServices.CacheKeys.Module, module, module.Assembly.ManifestModule);
        }

        public static void WriteAssembly(this IDictionary<string, object> cache, Assembly assembly)
        {
            cache.WriteAssembly(assembly, (Assembly)null);
        }

        public static void WriteAssembly(this IDictionary<string, object> cache, Assembly assembly, Assembly defaultAssembly)
        {
            cache.Write(WriteAssemblyCore, AttributedCacheServices.CacheKeys.Assembly, assembly, defaultAssembly);
        }
        
        private static ParameterInfo ReadParameter(this IDictionary<string, object> cache, LazyInit<Type> defaultType)
        {
            return cache.Read((c) => ReadParameterCore(c, defaultType), AttributedCacheServices.CacheKeys.Parameter);
        }

        public static LazyInit<Type> ReadLazyTypeForPart(this IDictionary<string, object> cache)
        {
             return LazyServices.MakeLazy(()=> cache.ReadMember<Type>(AttributedCacheServices.CacheKeys.PartType, (Type)null));
        }

        public static LazyInit<ParameterInfo> ReadLazyParameter(this IDictionary<string, object> cache, LazyInit<Type> defaultType)
        {
            Assumes.NotNull(cache);
            return LazyServices.MakeLazy(() => cache.ReadParameter(defaultType));
        }

        public static Func<MemberInfo[]> ReadLazyAccessors(this IDictionary<string, object> cache, LazyInit<Type> defaultType)
        {
            return cache.Read((c) => ReadAccessorsCore(c, defaultType), AttributedCacheServices.CacheKeys.Accessors);
        }

        private static Func<MemberInfo[]> ReadAccessorsCore(this IDictionary<string, object> cache, LazyInit<Type> defaultType)
        {
            Assumes.NotNull(cache);
            return () =>
                {
                    MemberInfo[] accessors = new MemberInfo[cache.Count];
                    for (int i = 0; i < cache.Count; i++)
                    {
                        string key = i.ToString(CultureInfo.InvariantCulture);
                        accessors[i] = cache.ReadMember<MemberInfo>(key, defaultType.Value);
                    }

                    return accessors;
                };
        }

        private static T ReadMember<T>(this IDictionary<string, object> cache, string key, Type defaultType) where T : MemberInfo
        {
            // first check if this an abbreviated version of member
            object cacheValue = cache.ReadValue<object>(key);
            IDictionary<string, object> cacheValueAsDictionary = cacheValue as IDictionary<string, object>;
            if (cacheValueAsDictionary != null)
            {
                return ReadMemberCore<T>(cacheValueAsDictionary, defaultType);
            }
            else if ((cacheValue != null) && (defaultType != null))
            {
                int metadataToken = (int)cacheValue;
                return (T)ReflectionResolver.ResolveMember(defaultType.Assembly.ManifestModule, metadataToken);
            }
            else
            {
                return null;
            }
        }

        private static Module ReadModule(this IDictionary<string, object> cache, Assembly assembly)
        {
            return cache.Read(c => ReadModuleCore(c, assembly), AttributedCacheServices.CacheKeys.Module, assembly.ManifestModule);
        }

        public static Assembly ReadAssembly(this IDictionary<string, object> cache)
        {
            return cache.ReadAssembly((Assembly)null);
        }

        public static Assembly ReadAssembly(this IDictionary<string, object> cache, Assembly defaultAssembly)
        {
            return cache.ReadAssembly(AttributedCacheServices.CacheKeys.Assembly, defaultAssembly);
        }

        private static Assembly ReadAssembly(this IDictionary<string, object> cache, string key, Assembly defaultAssembly)
        {
            return cache.Read<Assembly>(ReadAssemblyCore, key, defaultAssembly);
        }

        private static void WriteParameterCore(IDictionary<string, object> cache, ParameterInfo parameter, Type defaultType)
        {
            Assumes.NotNull(cache, parameter);
            cache.WriteValue(AttributedCacheServices.CacheKeys.ParameterPosition, parameter.Position, -1);
            cache.WriteMember(AttributedCacheServices.CacheKeys.ParameterConstructor, parameter.Member, defaultType);
        }

        public static void WriteContractName(this IDictionary<string, object> cache, string contractName)
        {
            Assumes.NotNull(cache);
            cache.WriteValue(AttributedCacheServices.CacheKeys.ContractName, contractName);
        }

        public static void WriteMetadata(this IDictionary<string, object> cache, IDictionary<string, object> metadata)
        {
            Assumes.NotNull(cache, metadata);
            cache.WriteDictionary(AttributedCacheServices.CacheKeys.Metadata, metadata);
        }

        private static void WriteAccessorsCore(IDictionary<string, object> cache, MemberInfo[] accessors, Type defaultType)
        {
            Assumes.NotNull(cache, accessors);
            int i = 0;
            foreach (MemberInfo accessor in accessors)
            {
                cache.WriteMember((i++).ToString(CultureInfo.InvariantCulture), accessor, defaultType);
            }
        }

        private static void WriteMemberCore(IDictionary<string, object> cache, MemberInfo member, Type defaultType)
        {
            Assumes.NotNull(cache, member);

            cache.WriteValue(AttributedCacheServices.CacheKeys.MetadataToken, member.MetadataToken);
            cache.WriteModule(member.Module);
            cache.WriteAssembly(member.Module.Assembly, defaultType == null ? null : defaultType.Assembly);
        }        

        private static void WriteModuleCore(IDictionary<string, object> cache, Module module)
        {
            Assumes.NotNull(module, cache);

            cache.WriteValue<int>(AttributedCacheServices.CacheKeys.MetadataToken, module.MetadataToken);
        }

        private static void WriteAssemblyCore(IDictionary<string, object> cache, Assembly assembly)
        {
            Assumes.NotNull(assembly);
            Assumes.NotNull(cache);

#if !SILVERLIGHT
            // NOTE : we need to store the path to the assembly. We can't use Location as in some cases - such as shadow copying - 
            // a new location will be created each time the application run. The Codebase is significantly more resilient, but 
            // it has the Uri format, and we will need to extract the path. We will also check that the assembly has been loaded
            // from file, as we don't support anything else at this point.
            // To do that we check that 
            // 1. Uri is a file
            // 2. Location is not empty (it being empty indicates that the assembly has been loaded from a byte stream or in some other funky way.
            Uri assemblyCodeBaseUri = new Uri(assembly.CodeBase);
            if (!assemblyCodeBaseUri.IsFile || string.IsNullOrEmpty(assembly.Location))
            {
                throw new InvalidOperationException(string.Format( CultureInfo.InvariantCulture, Strings.AssemblyInformationCannotBeCached, assembly.FullName));
            }
            // Local path properly will give full UNC path
            cache.WriteValue(AttributedCacheServices.CacheKeys.AssemblyLocation, assemblyCodeBaseUri.LocalPath);
            cache.WriteValue(AttributedCacheServices.CacheKeys.AssemblyTimeStamp, File.GetLastWriteTimeUtc(assemblyCodeBaseUri.LocalPath).Ticks);
#endif

            cache.WriteValue(AttributedCacheServices.CacheKeys.AssemblyFullName, assembly.FullName);
        }

        public static string GetAssemblyName(this IDictionary<string, object> cache)
        {
            cache = cache.ReadDictionary<object>(AttributedCacheServices.CacheKeys.Assembly);

            return cache.ReadValue<string>(AttributedCacheServices.CacheKeys.AssemblyFullName);
        }

        public static bool IsAssemblyCacheUpToDate(this IDictionary<string, object> cache)
        {
            Assumes.NotNull(cache);

            cache = cache.ReadDictionary<object>(AttributedCacheServices.CacheKeys.Assembly);

#if !SILVERLIGHT
            string assemblyLocation = cache.ReadValue<string>(AttributedCacheServices.CacheKeys.AssemblyLocation);
            long assemblyTimeStamp = cache.ReadValue<long>(AttributedCacheServices.CacheKeys.AssemblyTimeStamp);
            if (!File.Exists(assemblyLocation))
            {
                return false;
            }
            if (File.GetLastWriteTimeUtc(assemblyLocation).Ticks != assemblyTimeStamp)
            {
                return false;
            }
#endif
            return true;
        }

        private static Assembly ReadAssemblyCore(this IDictionary<string, object> cache)
        {
            Assumes.NotNull(cache);

            // NOTE : we load assembly with the full name AND codebase
            // This will force the loader to look for the default probing path - local directories, GAC, etc - and only after that fallback onto the path
            // if we want the reverse behavior, we will need to try loading from codebase first, and of that fails load from the defualt context
            string assemblyName = cache.ReadValue<string>(AttributedCacheServices.CacheKeys.AssemblyFullName);
            string codeBase = cache.ReadValue<string>(AttributedCacheServices.CacheKeys.AssemblyLocation);

            return ReflectionResolver.ResolveAssembly(assemblyName, codeBase);
        }

        public static string ReadContractName(this IDictionary<string, object> cache)
        {
            Assumes.NotNull(cache);
            return cache.ReadValue<string>(AttributedCacheServices.CacheKeys.ContractName);
        }

        public static LazyInit<IDictionary<string, object>> ReadLazyMetadata(this IDictionary<string, object> cache)
        {
            Assumes.NotNull(cache);
            return LazyServices.MakeLazy(()=> cache.ReadDictionary<object>(AttributedCacheServices.CacheKeys.Metadata));
        }

        private static Module ReadModuleCore(this IDictionary<string, object> cache, Assembly assembly)
        {
            Assumes.NotNull(cache, assembly);

            int metadataToken = cache.ReadValue<int>(AttributedCacheServices.CacheKeys.MetadataToken);

            return ReflectionResolver.ResolveModule(assembly, metadataToken);
        }

        private static T ReadMemberCore<T>(IDictionary<string, object> cache, Type defaultType) where T : MemberInfo
        {
            Assumes.NotNull(cache);

            int metadataToken = cache.ReadValue<int>(AttributedCacheServices.CacheKeys.MetadataToken);
            Assembly assembly = cache.ReadAssembly(defaultType == null ? null : defaultType.Assembly);
            Module module = cache.ReadModule(assembly);

            return (T)ReflectionResolver.ResolveMember(module, metadataToken);
        }

        private static ParameterInfo ReadParameterCore(IDictionary<string, object> cache, LazyInit<Type> defaultType)
        {   
            int parameterPosition = cache.ReadValue<int>(AttributedCacheServices.CacheKeys.ParameterPosition);
            ConstructorInfo constructor = cache.ReadMember<ConstructorInfo>(AttributedCacheServices.CacheKeys.ParameterConstructor, defaultType.Value);

            Assumes.IsTrue(parameterPosition >= 0);

            ParameterInfo[] parameters = constructor.GetParameters();

            Assumes.IsTrue(parameterPosition < parameters.Length);

            return parameters[parameterPosition];
        }

        private static void Write<T>(this IDictionary<string, object> cache, Action<IDictionary<string, object>, T> writer, string key, T value)
        {
            Write<T>(cache, writer, key, value, default(T));
        }

        private static void Write<T>(this IDictionary<string, object> cache, Action<IDictionary<string, object>, T> writer, string key, T value, T defaultValue)
        {
            if (object.Equals(value, defaultValue))
            {   // Only write out the value if it is not the default
                return;
            }

            var valueCache = new Dictionary<string, object>();
            writer(valueCache, value);

            cache.WriteDictionary(key, valueCache);
        }

        private static T Read<T>(this IDictionary<string, object> cache, Func<IDictionary<string, object>, T> loader, string key)
        {
            return Read(cache, loader, key, default(T));
        }

        private static T Read<T>(this IDictionary<string, object> cache, Func<IDictionary<string, object>, T> loader, string key, T defaultValue)
        {
            var valueCache = cache.ReadDictionary<object>(key);
            if (valueCache.Count != 0)
            {
                return loader(valueCache);
            }

            return defaultValue;
        }
    }
}