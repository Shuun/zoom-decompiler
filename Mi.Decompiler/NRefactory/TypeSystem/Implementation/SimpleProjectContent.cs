// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

using Mi.NRefactory.Utils;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Simple <see cref="ITypeResolveContext"/> implementation that stores the list of classes/namespaces.
	/// Synchronization is implemented using a <see cref="ReaderWriterLockSlim"/>.
	/// </summary>
	/// <remarks>
	/// Compared with <see cref="TypeStorage"/>, this class adds support for the ITypeResolveContext interface,
	/// for partial classes, and for multi-threading.
	/// </remarks>
	public sealed class SimpleProjectContent : ITypeResolveContext
	{
		// This class is sealed by design:
		// the synchronization story doesn't mix well with someone trying to extend this class.
		// If you wanted to derive from this: use delegation, not inheritance.
		
		readonly TypeStorage types = new TypeStorage();
		
		#region AssemblyAttributes
		readonly List<IAttribute> assemblyAttributes = new List<IAttribute>(); // mutable assembly attribute storage
		
		volatile IAttribute[] readOnlyAssemblyAttributes = {}; // volatile field with copy for reading threads
		
		/// <inheritdoc/>
		public IList<IAttribute> AssemblyAttributes {
			get { return readOnlyAssemblyAttributes; }
		}
		
		void AddRemoveAssemblyAttributes(ICollection<IAttribute> addedAttributes, ICollection<IAttribute> removedAttributes)
		{
			// API uses ICollection instead of IEnumerable to discourage users from evaluating
			// the list inside the lock (this method is called inside the write lock)
			bool hasChanges = false;
			if (removedAttributes != null && removedAttributes.Count > 0) {
				if (assemblyAttributes.RemoveAll(removedAttributes.Contains) > 0)
					hasChanges = true;
			}
			if (addedAttributes != null) {
				assemblyAttributes.AddRange(addedAttributes);
				hasChanges = true;
			}
			
			if (hasChanges)
				readOnlyAssemblyAttributes = assemblyAttributes.ToArray();
		}
		#endregion
		
		#region AddType
		void AddType(ITypeDefinition typeDefinition)
		{
			if (typeDefinition == null)
				throw new ArgumentNullException("typeDefinition");
			typeDefinition.Freeze(); // Type definition must be frozen before it can be added to a project content
			if (typeDefinition.ProjectContent != this)
				throw new ArgumentException("Cannot add a type definition that belongs to another project content");
			
			// TODO: handle partial classes
			types.UpdateType(typeDefinition);
		}
		#endregion
		
		#region RemoveType
		void RemoveType(ITypeDefinition typeDefinition)
		{
			throw new NotImplementedException();
		}
		#endregion
		
		#region UpdateProjectContent
		/// <summary>
		/// Removes oldTypes from the project, adds newTypes.
		/// Removes oldAssemblyAttributes, adds newAssemblyAttributes.
		/// </summary>
		/// <remarks>
		/// The update is done inside a write lock; when other threads access this project content
		/// from within a <c>using (Synchronize())</c> block, they will not see intermediate (inconsistent) state.
		/// </remarks>
		public void UpdateProjectContent(ICollection<ITypeDefinition> oldTypes = null,
		                                 ICollection<ITypeDefinition> newTypes = null,
		                                 ICollection<IAttribute> oldAssemblyAttributes = null,
		                                 ICollection<IAttribute> newAssemblyAttributes = null)
		{
				if (oldTypes != null) {
					foreach (var element in oldTypes) {
						RemoveType(element);
					}
				}
				if (newTypes != null) {
					foreach (var element in newTypes) {
						AddType(element);
					}
				}
				AddRemoveAssemblyAttributes(oldAssemblyAttributes, newAssemblyAttributes);
		}
		#endregion
		
		#region ITypeResolveContext implementation
		public ITypeDefinition GetClass(string nameSpace, string name, int typeParameterCount, StringComparer nameComparer)
		{
			return types.GetClass(nameSpace, name, typeParameterCount, nameComparer);
		}
		
		public IEnumerable<ITypeDefinition> GetClasses()
		{
			// make a copy with ToArray() for thread-safe access
			return types.GetClasses().ToArray();
		}
		
		public IEnumerable<ITypeDefinition> GetClasses(string nameSpace, StringComparer nameComparer)
		{
			// make a copy with ToArray() for thread-safe access
			return types.GetClasses(nameSpace, nameComparer).ToArray();
		}
		
		public IEnumerable<string> GetNamespaces()
		{
			// make a copy with ToArray() for thread-safe access
			return types.GetNamespaces().ToArray();
		}
		
		public string GetNamespace(string nameSpace, StringComparer nameComparer)
		{
			return types.GetNamespace(nameSpace, nameComparer);
		}
		#endregion
		
		#region Synchronization
		public CacheManager CacheManager {
			get { return null; }
		}
		#endregion
	}
}
