// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Internal;
using System.Globalization;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Caching
{
    internal class AssemblyCacheGenerator
    {
        private string _catalogIdentifier;
        private IDictionary<string, object> _catalogMetadata;
        private ModuleBuilder _moduleBuilder;
        private ICachedComposablePartCatalogSite _cachedCatalogSite;
        private TypeBuilder _stubBuilder;
        private MethodBuilder _createImportDefinitionMethod;
        private MethodBuilder _createExportDefinitionMethod;
        private MethodBuilder _createPartDefinitionMethod;
        private MethodBuilder _createContractNameToPartDefinitionMappingMethod;
        private MethodBuilder _getCatalogMetadata;
        private TypeBuilder _partsDefinitionBuilder;
        private TypeBuilder _exportsDefinitionBuilder;
        private TypeBuilder _importsDefinitionBuilder;
        private Dictionary<string, List<GeneratedDelegate<Func<ComposablePartDefinition>>>> _contractNameToPartFactoryMapping;
        private bool _isGenerationStarted = false;
        private bool _isGenerationCompleted = false;
        private int _partsCounter = 0;

        private static ConstructorInfo _importsFactoryDelegateConstructor = typeof(Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
        private static ConstructorInfo _exportsFactoryDelegateConstructor = typeof(Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

        public AssemblyCacheGenerator(ModuleBuilder moduleBuilder, ICachedComposablePartCatalogSite cachedCatalogSite, string catalogIdentifier)
        {
            Assumes.NotNull(moduleBuilder);
            Assumes.NotNull(cachedCatalogSite);
            Assumes.NotNull(catalogIdentifier);

            this._moduleBuilder = moduleBuilder;
            this._catalogIdentifier = catalogIdentifier ?? string.Empty;
            this._catalogMetadata = new Dictionary<string, object>();
            this._cachedCatalogSite = cachedCatalogSite;
            this._contractNameToPartFactoryMapping = new Dictionary<string, List<GeneratedDelegate<Func<ComposablePartDefinition>>>>();
        }

        internal ModuleBuilder ModuleBuilder
        {
            get
            {
                return this._moduleBuilder;
            }
        }

        internal ICachedComposablePartCatalogSite CatalogSite
        {
            get
            {
                return this._cachedCatalogSite;
            }
        }

        internal string CatalogIdentifier
        {
            get
            {
                return this._catalogIdentifier;
            }
        }

        public void BeginGeneration()
        {
            Assumes.IsFalse(this._isGenerationStarted);
            Assumes.IsFalse(this._isGenerationCompleted);
            this._isGenerationStarted = true;

            this.GenerateCachingStub();
            this.GenerateTables();

            Assumes.NotNull(this._stubBuilder);
            Assumes.NotNull(this._createPartDefinitionMethod);
            Assumes.NotNull(this._createExportDefinitionMethod);
            Assumes.NotNull(this._createImportDefinitionMethod);
            Assumes.NotNull(this._createContractNameToPartDefinitionMappingMethod);

            Assumes.NotNull(this._partsDefinitionBuilder);
            Assumes.NotNull(this._exportsDefinitionBuilder);
            Assumes.NotNull(this._importsDefinitionBuilder);
        }

        public CompositionResult<Type> EndGeneration()
        {
            Assumes.IsTrue(this._isGenerationStarted);
            Assumes.IsFalse(this._isGenerationCompleted);
            CompositionResult result = CompositionResult.SucceededResult;

            result = result.MergeResult(this.GenerateContractNameToPartDefinitionMapping());
            result = result.MergeResult(this.GenerateGetCatalogMetadata());
            Type stubType = this._stubBuilder.CreateType();
            this._partsDefinitionBuilder.CreateType();
            this._exportsDefinitionBuilder.CreateType();
            this._importsDefinitionBuilder.CreateType();

            this._isGenerationCompleted = true;
            return result.ToResult<Type>(stubType);
        }

        private void GenerateTables()
        {
            this._partsDefinitionBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.PartDefinitionTableNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            this._exportsDefinitionBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.ExportsDefinitionTableNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract);

            this._importsDefinitionBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.ImportsDefinitionTableNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract);

        }

        private void GenerateCachingStub()
        {
            //
            // This stub is injected into all caches. The creation methods call into it to create actual instances of parts, exports and imports
            // The cacher reade injects the delegates into its fields.
            //
            //public class CachingStub
            //{
            //    // creates the export from the dictionary
            //    public static Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition> ExportDefinitionFactory;
            //    // creates the import from the dictionary
            //    public static Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition> ImportDefinitionFactory;
            //    // creates the part definition from the dictionary. 
            //    public static Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition> PartDefinitionFactory;
            //
            //    internal static ExportDefinition CreateExportDefinition(ComposablePartDefinition owner, IDictionary<string, object> cache)
            //    {
            //        return CachingStub.ExportDefinitionFactory.Invoke(owner, cache);
            //    }
            //
            //    internal static ImportDefinition CreateImportDefinition(ComposablePartDefinition owner, IDictionary<string, object> cache)
            //    {
            //        return CachingStub.ImportDefinitionFactory.Invoke(owner, cache);
            //    }
            //
            //    internal static ComposablePartDefinition CreatePartDefinition(IDictionary<string, object> cache, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> importsFactory, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>> exportsFactory)
            //    {
            //        return CachingStub.PartDefinitionFactory.Invoke(cache, importsFactory, exportsFactory);
            //    }
            //    
            //    public static IDictionary<string, IEnumerable<Func<ComposablePartDefinition>>> CreateContractNameToPartDefinitionMapping()
            //    {
            //        Dictionary<string, IEnumerable<Func<ComposablePartDefinition>>> dictionary = new Dictionary<string, IEnumerable<Func<ComposablePartDefinition>>>();
            //        dictionary.Add(<contractName>, new Func<ComposablePartDefinition>[] {method1, method2...};
            //        dictionary.Add(<contractName>, new Func<ComposablePartDefinition>[] {method1, method2...};
            //        ...
            //        return dictionary;
            //    }
            //   
            //    public static IDictionary<string, object> GetCatalogMetadata()
            //    {
            //       return <metadata>;
            //    }
            //
            //    public static string GetCatalogIdentifier()
            //    {
            //       return <id>;
            //    }
            //}

            // define type
            this._stubBuilder = this._moduleBuilder.DefineType(
                string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.CachingStubTypeNamePrefix, this._catalogIdentifier),
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            // define fields
            FieldBuilder exportDefinitionFactoryFieldBuilder = this._stubBuilder.DefineField(
                CacheStructureConstants.CachingStubExportDefinitionFactoryFieldName,
                typeof(Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition>),
                FieldAttributes.Static | FieldAttributes.Public
                );

            FieldBuilder importDefinitionFactoryFieldBuilder = this._stubBuilder.DefineField(
                CacheStructureConstants.CachingStubImportDefinitionFactoryFieldName,
                typeof(Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition>),
                FieldAttributes.Static | FieldAttributes.Public
                );

            FieldBuilder partDefinitionFactoryFieldBuilder = this._stubBuilder.DefineField(
                CacheStructureConstants.CachingStubPartDefinitionFactoryFieldName,
                typeof(Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition>),
                FieldAttributes.Static | FieldAttributes.Public
                );


            //
            // define calling method per field
            //
            ILGenerator ilGenerator = null;

            // static ExportDefinition CreateExportDefinition(ComposablePartDefinition owner, IDictionary<string, object> arg0)
            this._createExportDefinitionMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreateExportDefinitionMethodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                typeof(ExportDefinition),
                new Type[] { typeof(ComposablePartDefinition), typeof(IDictionary<string, object>) });
            ilGenerator = this._createExportDefinitionMethod.GetILGenerator();

            // CachingStub.ExportDefinitionFactory.Invoke(owner, cache)
            ilGenerator.Emit(OpCodes.Ldsfld, exportDefinitionFactoryFieldBuilder); // load the field
            ilGenerator.Emit(OpCodes.Ldarg_0); // load the part
            ilGenerator.Emit(OpCodes.Ldarg_1); // load the dictionary
            ilGenerator.EmitCall(OpCodes.Callvirt, typeof(Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition>).GetMethod("Invoke"), null);
            ilGenerator.Emit(OpCodes.Ret);


            // static ImportDefinition CreateImportDefinition(ComposablePartDefinition owner, IDictionary<string, object> arg0)
            this._createImportDefinitionMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreateImportDefinitionMethodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                typeof(ImportDefinition),
                new Type[] { typeof(ComposablePartDefinition), typeof(IDictionary<string, object>) });
            ilGenerator = this._createImportDefinitionMethod.GetILGenerator();

            // CachingStub.ImportDefinitionFactory.Invoke(owner, cache)
            ilGenerator.Emit(OpCodes.Ldsfld, importDefinitionFactoryFieldBuilder); // load the field
            ilGenerator.Emit(OpCodes.Ldarg_0); // load the part
            ilGenerator.Emit(OpCodes.Ldarg_1); // load the dictionary
            ilGenerator.EmitCall(OpCodes.Callvirt, typeof(Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition>).GetMethod("Invoke"), null);
            ilGenerator.Emit(OpCodes.Ret);

            // ComposablePartDefinition CreatePartDefinition(IDictionary<string, object> arg0, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>> arg1, Func<IEnumerable<ExportDefinition>> arg2)
            this._createPartDefinitionMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreatePartDefinitionMethodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                typeof(ComposablePartDefinition),
                new Type[] { typeof(IDictionary<string, object>), typeof(Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>), typeof(Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>) });
            ilGenerator = this._createPartDefinitionMethod.GetILGenerator();

            // CachingStub.PartDefinitionFactory.Invoke(cache, exportsFactory, importsFactory)
            ilGenerator.Emit(OpCodes.Ldsfld, partDefinitionFactoryFieldBuilder); // load the field
            ilGenerator.Emit(OpCodes.Ldarg_0); // load the dictionary
            ilGenerator.Emit(OpCodes.Ldarg_1); // load the import factory
            ilGenerator.Emit(OpCodes.Ldarg_2); // load the export factory
            ilGenerator.EmitCall(OpCodes.Callvirt, typeof(Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition>).GetMethod("Invoke"), null);
            ilGenerator.Emit(OpCodes.Ret);

            //
            // Define the mapping method. The body will be generated on completion
            //
            this._createContractNameToPartDefinitionMappingMethod = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubCreateContractNameToPartDefinitionMappingMethodName,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(IDictionary<string, IEnumerable<Func<ComposablePartDefinition>>>),
                Type.EmptyTypes);

            //
            // Define the "GetCatalogMetadata" method. The body will be generated on completion
            // 
            this._getCatalogMetadata = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubGetCatalogMetadata,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(IDictionary<string, object>),
                Type.EmptyTypes);

            // 
            // Define the "GetCatalogIdentifier
            //
            MethodBuilder getCatalogIdentifier = this._stubBuilder.DefineMethod(
                CacheStructureConstants.CachingStubGetCatalogIdentifier,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(string),
                Type.EmptyTypes);
            ilGenerator = getCatalogIdentifier.GetILGenerator();

            ilGenerator.LoadValue(this._catalogIdentifier);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private CompositionResult GenerateContractNameToPartDefinitionMapping()
        {
            Assumes.NotNull(this._createContractNameToPartDefinitionMappingMethod);

            ILGenerator ilGenerator = this._createContractNameToPartDefinitionMappingMethod.GetILGenerator();
            CompositionResult result = ilGenerator.LoadValue(this._contractNameToPartFactoryMapping);
            ilGenerator.Emit(OpCodes.Ret);

            return result;
        }

        private CompositionResult GenerateGetCatalogMetadata()
        {
            Assumes.NotNull(this._getCatalogMetadata);
            Assumes.NotNull(this._catalogMetadata);

            ILGenerator ilGenerator = this._getCatalogMetadata.GetILGenerator();
            CompositionResult result = ilGenerator.LoadValue(this._catalogMetadata);
            ilGenerator.Emit(OpCodes.Ret);

            return result;
        }

        public void CacheCatalogMetadata(IDictionary<string, object> catalogMetadata)
        {
            Assumes.NotNull(catalogMetadata);
            foreach (KeyValuePair<string, object> kvp in catalogMetadata)
            {
                this._catalogMetadata[kvp.Key] = kvp.Value;
            }
        }


        public CompositionResult<MethodInfo> CachePartDefinition(ComposablePartDefinition partDefinition)
        {
            Assumes.NotNull(partDefinition);
            CompositionResult result = CompositionResult.SucceededResult;
            string methodName = string.Format(CultureInfo.InvariantCulture, "{0}", this._partsCounter);


            //
            // public static ComposablePartDefinition<>()
            // {
            //    // load dictionary
            //    return CachingStubX.CreatePartDefinition(<dictinary>, <importsFactory>, <exportsFactory>);
            // }

            // Generate the signature
            MethodBuilder partFactoryBuilder = this._partsDefinitionBuilder.DefineMethod(
                methodName,
                MethodAttributes.Static | MethodAttributes.Public,
                typeof(ComposablePartDefinition),
                Type.EmptyTypes);
            ILGenerator ilGenerator = partFactoryBuilder.GetILGenerator();

            // Generate imports caching
            CompositionResult<MethodInfo> importsFactoryResult = AssemblyCacheGenerator.CachePartImportsOrExports<ImportDefinition>(
                partDefinition.ImportDefinitions,
                this._importsDefinitionBuilder,
                this._createImportDefinitionMethod,
                (import) => this._cachedCatalogSite.CacheImportDefinition(partDefinition, import),
                methodName);
            result = result.MergeErrors(importsFactoryResult.Errors);

            // Generate exports caching
            CompositionResult<MethodInfo> exportsFactoryResult = AssemblyCacheGenerator.CachePartImportsOrExports<ExportDefinition>(
                partDefinition.ExportDefinitions,
                this._exportsDefinitionBuilder,
                this._createExportDefinitionMethod,
                (export) => this._cachedCatalogSite.CacheExportDefinition(partDefinition, export),
                methodName);
            result = result.MergeErrors(exportsFactoryResult.Errors);

            // get the actual cache for the part definition
            IDictionary<string, object> cache = this._cachedCatalogSite.CachePartDefinition(partDefinition);

            //
            // now write the method
            //

            // load the cache dictionary on stack
            result = result.MergeResult(ilGenerator.LoadValue(cache));

            // load the imports delegate on stack
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Ldftn, importsFactoryResult.Value);
            ilGenerator.Emit(OpCodes.Newobj, AssemblyCacheGenerator._importsFactoryDelegateConstructor);

            // load the exports delegate on stack
            ilGenerator.Emit(OpCodes.Ldnull);
            ilGenerator.Emit(OpCodes.Ldftn, exportsFactoryResult.Value);
            ilGenerator.Emit(OpCodes.Newobj, AssemblyCacheGenerator._exportsFactoryDelegateConstructor);

            // and then call into stub.CreatePartDefinition and return 
            ilGenerator.EmitCall(OpCodes.Call, this._createPartDefinitionMethod, null);
            ilGenerator.Emit(OpCodes.Ret);
            this._partsCounter++;

            this.MapContractsForCachedPartDefinition(partDefinition, partFactoryBuilder);
            return result.ToResult<MethodInfo>(partFactoryBuilder);
        }

        private static CompositionResult<MethodInfo> CachePartImportsOrExports<T>(IEnumerable<T> items, TypeBuilder definitionsTable, MethodBuilder stubFactoryMethod, Func<T, IDictionary<string, object>> cacheGenerator, string methodName)
             // in reality this is only ExportDefinition or ImportDefinition
        {
            Assumes.NotNull(items);
            Assumes.NotNull(definitionsTable);
            Assumes.NotNull(stubFactoryMethod);
            Assumes.NotNull(cacheGenerator);
            Assumes.NotNull(methodName);
            CompositionResult result = CompositionResult.SucceededResult;


            //
            // internal static IEnumerable<T> CreateTs(ComposablePartDefinition owner)
            // {
            //    T[] items = new ImportDefinition[<count>];
            //    
            //    IDictionary<string, object> dictionary0 = new Dictionary<string, object>();
            //    <populate the dictionary with the cache values>
            //    items[0] = CachingStubX.CreateTDefinition(dictinary0);
            //    ...
            //    IDictionary<string, object> dictionary<count-1> = new Dictionary<string, object>();
            //    <populate the dictionary with the cache values>
            //    items[<count-1>] = CachingStubX.CreateTDefinition(dictinary<count-1>);
            //    ...
            //    return items;
            // }

            // Generate the signature
            MethodBuilder itemsFactoryBuilder = definitionsTable.DefineMethod(
                methodName,
                MethodAttributes.Static | MethodAttributes.Assembly,
                typeof(IEnumerable<T>),
                new Type[] {typeof(ComposablePartDefinition)});
            ILGenerator ilGenerator = itemsFactoryBuilder.GetILGenerator();

            //
            // Generate array creation
            // 
            Type itemType = typeof(T);
            LocalBuilder generatedArrayLocal = ilGenerator.DeclareLocal(itemType.MakeArrayType());
            ilGenerator.LoadValue(items.Count());
            ilGenerator.Emit(OpCodes.Newarr, typeof(T));
            ilGenerator.Emit(OpCodes.Stloc, generatedArrayLocal);

            int index = 0;
            foreach (T item in items)
            {
                // get the cache
                IDictionary<string, object> cache = cacheGenerator(item);

                //
                //items[<index>] = stub.CreateTDefinition(<dictionary>)
                //
                ilGenerator.Emit(OpCodes.Ldloc, generatedArrayLocal);
                result = result.MergeResult(ilGenerator.LoadValue(index));

                ilGenerator.Emit(OpCodes.Ldarg_0); // load the part definition
                result = result.MergeResult(ilGenerator.LoadValue(cache)); // load the dictionary
                ilGenerator.EmitCall(OpCodes.Call, stubFactoryMethod, null);

                ilGenerator.Emit(OpCodes.Stelem, itemType);
                index++;
            }

            // load the value and return
            ilGenerator.Emit(OpCodes.Ldloc, generatedArrayLocal);
            ilGenerator.Emit(OpCodes.Ret);

            return result.ToResult<MethodInfo>(itemsFactoryBuilder);
        }

        private void MapContractsForCachedPartDefinition(ComposablePartDefinition partDefinition, MethodInfo partFactory)
        {
            GeneratedDelegate<Func<ComposablePartDefinition>> partFactoryDelegate = new GeneratedDelegate<Func<ComposablePartDefinition>>() { Method = partFactory };
            foreach (string contractName in partDefinition.ExportDefinitions.Select(export => export.ContractName).Distinct())
            {
                List<GeneratedDelegate<Func<ComposablePartDefinition>>> contractPartFactories = null;
                if (!this._contractNameToPartFactoryMapping.TryGetValue(contractName, out contractPartFactories))
                {
                    contractPartFactories = new List<GeneratedDelegate<Func<ComposablePartDefinition>>>();
                    this._contractNameToPartFactoryMapping.Add(contractName, contractPartFactories);
                }
                contractPartFactories.Add(partFactoryDelegate);
            }
        }

    }

}
