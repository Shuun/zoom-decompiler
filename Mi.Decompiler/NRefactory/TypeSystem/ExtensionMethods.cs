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
		
		#region IsOpen / IsUnbound
		sealed class TypeClassificationVisitor : TypeVisitor
		{
			internal bool isOpen;
			
			public override IType VisitTypeParameter(TypeParameter type)
			{
				isOpen = true;
				return base.VisitTypeParameter(type);
			}
		}
		
		/// <summary>
		/// Gets whether the type is an open type (contains type parameters).
		/// </summary>
		public static bool IsOpen(this IType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			TypeClassificationVisitor v = new TypeClassificationVisitor();
			type.AcceptVisitor(v);
			return v.isOpen;
		}
		
		/// <summary>
		/// Gets whether the type is unbound.
		/// </summary>
		public static bool IsUnbound(this IType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			return type is TypeDefinition && type.TypeParameterCount > 0;
		}
		#endregion
		
		#region IsEnum / IsDelegate
		/// <summary>
		/// Gets whether the type is an enumeration type.
		/// </summary>
		public static bool IsEnum(this IType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			TypeDefinition def = type.GetDefinition();
			return def != null && def.ClassType == ClassType.Enum;
		}
		
		/// <summary>
		/// Gets the underlying type for this enum type.
		/// </summary>
		public static IType GetEnumUnderlyingType(this IType enumType, ITypeResolveContext context)
		{
			if (enumType == null)
				throw new ArgumentNullException("enumType");
			if (context == null)
				throw new ArgumentNullException("context");
			TypeDefinition def = enumType.GetDefinition();
			if (def != null && def.ClassType == ClassType.Enum) {
				if (def.BaseTypes.Count == 1)
					return def.BaseTypes[0].Resolve(context);
				else
					return KnownTypeReference.Int32.Resolve(context);
			} else {
				throw new ArgumentException("enumType must be an enum");
			}
		}
		
		/// <summary>
		/// Gets whether the type is an delegate type.
		/// </summary>
		/// <remarks>This method returns <c>false</c> for System.Delegate itself</remarks>
		public static bool IsDelegate(this IType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			TypeDefinition def = type.GetDefinition();
			return def != null && def.ClassType == ClassType.Delegate;
		}
		#endregion
		
		#region InternalsVisibleTo
		/// <summary>
		/// Gets whether the internals of this project are visible to the other project
		/// </summary>
		public static bool InternalsVisibleTo(this ITypeResolveContext projectContent, ITypeResolveContext other, ITypeResolveContext context)
		{
			if (projectContent == other)
				return true;
			// TODO: implement support for [InternalsVisibleToAttribute]
			// Make sure implementation doesn't hurt performance, e.g. don't resolve all assembly attributes whenever
			// this method is called - it'll be called once per internal member during lookup operations
			return false;
		}
		#endregion
		
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
