// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using Microsoft.Internal;
using System.IO;
using System.Reflection;
using System.Linq.Expressions;
using System.Globalization;

namespace System.ComponentModel.Composition.Caching
{
    internal partial class ComposablePartCatalogAssemblyCache : ComposablePartCatalogCache
    {
        private class CatalogCache : ComposablePartCatalog
        {
            private Type _stubType;
            private string _catalogIdentifier;
            private ICachedComposablePartCatalogSite _cachedCatalogSite;
            private List<Func<ComposablePartDefinition>> _partDefinitionFactories;
            private Dictionary<Func<ComposablePartDefinition>, ComposablePartDefinition> _createdPartDefinitions;
            private IDictionary<string, IEnumerable<Func<ComposablePartDefinition>>> _contractPartDefinitionMapping;
            private IQueryable<ComposablePartDefinition> _parts;


            public CatalogCache(Type stubType, ICachedComposablePartCatalogSite cachedCatalogSite)
            {
                Assumes.NotNull(stubType);

                cachedCatalogSite = cachedCatalogSite ?? new EmptyCachedComposablePartCatalogSite();
                this._stubType = stubType;
                this._createdPartDefinitions = new Dictionary<Func<ComposablePartDefinition>, ComposablePartDefinition>();
                this._cachedCatalogSite = cachedCatalogSite;

                // Get catalog identifier
                MethodInfo getCatalogIdentifierMethod = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeMethod(this._stubType, CacheStructureConstants.CachingStubGetCatalogIdentifier, BindingFlags.Public | BindingFlags.Static);
                Func<string> getCatalogIdentifier = (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), getCatalogIdentifierMethod);
                this._catalogIdentifier = getCatalogIdentifier.Invoke();

                // get fields
                FieldInfo importDefinitionFactoryField = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeField(this._stubType, CacheStructureConstants.CachingStubImportDefinitionFactoryFieldName, BindingFlags.Public | BindingFlags.Static);
                FieldInfo exportDefinitionFactoryField = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeField(this._stubType, CacheStructureConstants.CachingStubExportDefinitionFactoryFieldName, BindingFlags.Public | BindingFlags.Static);
                FieldInfo partDefinitionFactoryField = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeField(this._stubType, CacheStructureConstants.CachingStubPartDefinitionFactoryFieldName, BindingFlags.Public | BindingFlags.Static);

                // get the mapping method
                MethodInfo createContractPartDefinitionMappingMethod = ComposablePartCatalogAssemblyCacheReader.GetCacheTypeMethod(this._stubType, CacheStructureConstants.CachingStubCreateContractNameToPartDefinitionMappingMethodName, BindingFlags.Public | BindingFlags.Static);

                // get part definition table
                Type partDefinitionTable = ComposablePartCatalogAssemblyCacheReader.GetCacheType(this._stubType.Assembly, string.Format(CultureInfo.InvariantCulture, "{0}{1}", CacheStructureConstants.PartDefinitionTableNamePrefix, this._catalogIdentifier));

                // initialize the cache fields with the factory methods
                importDefinitionFactoryField.SetValue(null, new Func<ComposablePartDefinition, IDictionary<string, object>, ImportDefinition>(this._cachedCatalogSite.CreateImportDefinitionFromCache));
                exportDefinitionFactoryField.SetValue(null, new Func<ComposablePartDefinition, IDictionary<string, object>, ExportDefinition>(this._cachedCatalogSite.CreateExportDefinitionFromCache));
                partDefinitionFactoryField.SetValue(null, new Func<IDictionary<string, object>, Func<ComposablePartDefinition, IEnumerable<ImportDefinition>>, Func<ComposablePartDefinition, IEnumerable<ExportDefinition>>, ComposablePartDefinition>(this._cachedCatalogSite.CreatePartDefinitionFromCache));

                // process the parts table and the contract->part mapping
                this.ProcessPartDefinitionTable(partDefinitionTable);
                this.ProcessContractPartDefinitionMapping(createContractPartDefinitionMappingMethod);
                this._parts = this._partDefinitionFactories.Select(partDefinitionFactory => this.CreatePartDefinition(partDefinitionFactory)).AsQueryable();
            }

            private void ProcessPartDefinitionTable(Type partDefinitionTable)
            {
                Assumes.NotNull(partDefinitionTable);

                // we simply go over every method and wrap it in a delegate
                this._partDefinitionFactories = new List<Func<ComposablePartDefinition>>();
                foreach (MethodInfo partDefinitionFactoryMethod in partDefinitionTable.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    Func<ComposablePartDefinition> partDefinitionFactory = (Func<ComposablePartDefinition>)Delegate.CreateDelegate(typeof(Func<ComposablePartDefinition>), partDefinitionFactoryMethod);
                    this._partDefinitionFactories.Add(partDefinitionFactory);
                }
            }

            private void ProcessContractPartDefinitionMapping(MethodInfo createContractPartDefinitionMappingMethod)
            {
                Assumes.NotNull(createContractPartDefinitionMappingMethod);
                Func<IDictionary<string, IEnumerable<Func<ComposablePartDefinition>>>> createContractPartDefinitionMappingDelegate = (Func<IDictionary<string, IEnumerable<Func<ComposablePartDefinition>>>>)Delegate.CreateDelegate(
                    typeof(Func<IDictionary<string, IEnumerable<Func<ComposablePartDefinition>>>>), createContractPartDefinitionMappingMethod);
                this._contractPartDefinitionMapping = createContractPartDefinitionMappingDelegate.Invoke();
            }

            private ComposablePartDefinition CreatePartDefinition(Func<ComposablePartDefinition> partDefinitionFactory)
            {
                // delegates have proper equality identify, so two delegates created over the same method are considered equal.
                // NOTE : this is very much not thread-safe and will have to be hardened
                ComposablePartDefinition partDefinition = null;
                if (!this._createdPartDefinitions.TryGetValue(partDefinitionFactory, out partDefinition))
                {
                    partDefinition = partDefinitionFactory.Invoke();
                    this._createdPartDefinitions.Add(partDefinitionFactory, partDefinition);
                }
                return partDefinition;
            }

            public override IQueryable<ComposablePartDefinition> Parts
            {
                get
                {
                    return this._parts;
                }
            }

            public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
            {
                Assumes.NotNull(definition, "definition");

                ContractBasedImportDefinition contractDefinition = definition as ContractBasedImportDefinition;

                if (contractDefinition != null)
                {
                    return this.GetExports(contractDefinition);
                }

                return base.GetExports(definition);
            }

            private IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ContractBasedImportDefinition definition)
            {
                IEnumerable<Func<ComposablePartDefinition>> matchingPartFactories = null;
                if (this._contractPartDefinitionMapping.TryGetValue(definition.ContractName, out matchingPartFactories))
                {
                    return matchingPartFactories.
                        Select(partFactory => this.CreatePartDefinition(partFactory)).
                        SelectMany(part => part.ExportDefinitions, (part, export) => new Tuple<ComposablePartDefinition, ExportDefinition>(part, export)).
                        Where(partAndExport => definition.MatchConstraint(partAndExport.Item2));
                }
                else
                {
                    return Enumerable.Empty<Tuple<ComposablePartDefinition, ExportDefinition>>();
                }
            }
        }
    }
}
