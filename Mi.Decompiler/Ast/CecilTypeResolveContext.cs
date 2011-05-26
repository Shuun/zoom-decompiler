// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using Mi.NRefactory.TypeSystem;
using Mi.Assemblies;

namespace Mi.Decompiler.Ast
{
	/// <summary>
	/// ITypeResolveContext implementation that lazily loads types from Cecil.
	/// </summary>
	public class CecilTypeResolveContext :  ITypeResolveContext
	{
		readonly ModuleDefinition module;
		readonly string[] namespaces;
		readonly CecilLoader loader;
        Dictionary<Mi.Assemblies.TypeDefinition, WeakReference> resolvedCache = new Dictionary<Assemblies.TypeDefinition, WeakReference>();
		int countUntilNextCleanup = 4;
		
		public CecilTypeResolveContext(ModuleDefinition module)
		{
			this.loader = new CecilLoader();
			this.loader.IncludeInternalMembers = true;
			this.module = module;
			this.namespaces = module.Types.Select(t => t.Namespace).Distinct().ToArray();
		}
		
		Mi.NRefactory.TypeSystem.TypeDefinition GetClass(Mi.Assemblies.TypeDefinition cecilType)
		{
			lock (resolvedCache) {
				WeakReference wr;
				Mi.NRefactory.TypeSystem.TypeDefinition type;
				if (resolvedCache.TryGetValue(cecilType, out wr)) {
					type = (Mi.NRefactory.TypeSystem.TypeDefinition)wr.Target;
				} else {
					wr = null;
					type = null;
				}
				if (type == null) {
					type = loader.LoadType(cecilType, this);
				}
				if (wr == null) {
					if (--countUntilNextCleanup <= 0)
						CleanupDict();
					wr = new WeakReference(type);
					resolvedCache.Add(cecilType, wr);
				} else {
					wr.Target = type;
				}
				return type;
			}
		}
		
		void CleanupDict()
		{
			var deletedKeys =
                (from pair in resolvedCache
                where !pair.Value.IsAlive
                select pair.Key).ToList();

            foreach (var key in deletedKeys) {
				resolvedCache.Remove(key);
			}
			countUntilNextCleanup = resolvedCache.Count + 4;
		}
		
		public Mi.NRefactory.TypeSystem.TypeDefinition GetClass(string nameSpace, string name, int typeParameterCount, StringComparer nameComparer)
		{
			if (typeParameterCount > 0)
				name = name + "`" + typeParameterCount.ToString();
			if (nameComparer == StringComparer.Ordinal) {
				var cecilType = module.GetType(nameSpace, name);
				if (cecilType != null)
					return GetClass(cecilType);
				else
					return null;
			}
			foreach (var cecilType in module.Types) {
				if (nameComparer.Equals(name, cecilType.Name)
				    && nameComparer.Equals(nameSpace, cecilType.Namespace)
				    && cecilType.GenericParameters.Count == typeParameterCount)
				{
					return GetClass(cecilType);
				}
			}
			return null;
		}
		
		public IEnumerable<Mi.NRefactory.TypeSystem.TypeDefinition> GetClasses()
		{
			foreach (var cecilType in module.Types) {
				yield return GetClass(cecilType);
			}
		}
		
		public IEnumerable<Mi.NRefactory.TypeSystem.TypeDefinition> GetClasses(string nameSpace, StringComparer nameComparer)
		{
			foreach (var cecilType in module.Types) {
				if (nameComparer.Equals(nameSpace, cecilType.Namespace))
					yield return GetClass(cecilType);
			}
		}
		
		public IEnumerable<string> GetNamespaces()
		{
			return namespaces;
		}
		
		public string GetNamespace(string nameSpace, StringComparer nameComparer)
		{
			foreach (string ns in namespaces) {
				if (nameComparer.Equals(ns, nameSpace))
					return ns;
			}
			return null;
		}
		
		Mi.NRefactory.Utils.CacheManager ITypeResolveContext.CacheManager {
			get {
				// We don't support caching
				return null;
			}
		}
	}
}
