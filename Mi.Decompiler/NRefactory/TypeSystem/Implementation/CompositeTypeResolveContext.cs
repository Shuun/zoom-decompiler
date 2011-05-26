// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Mi.NRefactory.Utils;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Represents multiple type resolve contexts.
	/// </summary>
	public class CompositeTypeResolveContext : ITypeResolveContext
	{
		/// <summary>
		/// Creates a <see cref="CompositeTypeResolveContext"/> that combines the given resolve contexts.
		/// If one of the input parameters is null, the other input parameter is returned directly.
		/// If both input parameters are null, the function returns null.
		/// </summary>
		public static ITypeResolveContext Combine(ITypeResolveContext a, ITypeResolveContext b)
		{
			if (a == null)
				return b;
			if (b == null)
				return a;
			return new CompositeTypeResolveContext(new [] { a, b });
		}
		
		readonly ITypeResolveContext[] children;
		
		/// <summary>
		/// Creates a new <see cref="CompositeTypeResolveContext"/>
		/// </summary>
		public CompositeTypeResolveContext(IEnumerable<ITypeResolveContext> children)
		{
			if (children == null)
				throw new ArgumentNullException("children");
			this.children = children.ToArray();
			foreach (ITypeResolveContext c in this.children) {
				if (c == null)
					throw new ArgumentException("children enumeration contains nulls");
			}
		}
		
		private CompositeTypeResolveContext(ITypeResolveContext[] children)
		{
			Debug.Assert(children != null);
			this.children = children;
		}
		
		/// <inheritdoc/>
		public TypeDefinition GetClass(string nameSpace, string name, int typeParameterCount, StringComparer nameComparer)
		{
			foreach (ITypeResolveContext context in children) {
				TypeDefinition d = context.GetClass(nameSpace, name, typeParameterCount, nameComparer);
				if (d != null)
					return d;
			}
			return null;
		}
		
		/// <inheritdoc/>
		public IEnumerable<TypeDefinition> GetClasses()
		{
			return children.SelectMany(c => c.GetClasses());
		}
		
		/// <inheritdoc/>
		public IEnumerable<TypeDefinition> GetClasses(string nameSpace, StringComparer nameComparer)
		{
			return children.SelectMany(c => c.GetClasses(nameSpace, nameComparer));
		}
		
		/// <inheritdoc/>
		public IEnumerable<string> GetNamespaces()
		{
			return children.SelectMany(c => c.GetNamespaces()).Distinct();
		}
		
		/// <inheritdoc/>
		public string GetNamespace(string nameSpace, StringComparer nameComparer)
		{
			foreach (ITypeResolveContext context in children) {
				string r = context.GetNamespace(nameSpace, nameComparer);
				if (r != null)
					return r;
			}
			return null;
		}
		
		public virtual CacheManager CacheManager {
			// We don't know if our input contexts are mutable, so, to be on the safe side,
			// we don't implement caching here.
			get { return null; }
		}
	}
}
