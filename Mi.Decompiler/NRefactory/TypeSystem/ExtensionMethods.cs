// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using Mi.NRefactory.Utils;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Contains extension methods for the type system.
	/// </summary>
	public static class ExtensionMethods
	{
		#region GetAllClasses
		/// <summary>
		/// Gets all classes, including nested classes.
		/// </summary>
		public static IEnumerable<TypeDefinition> GetAllClasses(this ITypeResolveContext context)
		{
			return TreeTraversal.PreOrder(context.GetClasses(), t => t.InnerClasses);
		}
		#endregion
	}
}
