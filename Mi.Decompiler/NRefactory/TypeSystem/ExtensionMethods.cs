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
		#region GetAllBaseTypes
		/// <summary>
		/// Gets all base types.
		/// </summary>
		/// <remarks>This is the reflexive and transitive closure of <see cref="IType.GetBaseTypes"/>.
		/// Note that this method does not return all supertypes - doing so is impossible due to contravariance
		/// (and undesirable for covariance as the list could become very large).
		/// </remarks>
		public static IEnumerable<IType> GetAllBaseTypes(this IType type, ITypeResolveContext context)
		{
			List<IType> output = new List<IType>();
			Stack<TypeDefinition> activeTypeDefinitions = new Stack<TypeDefinition>();
			CollectAllBaseTypes(type, context, activeTypeDefinitions, output);
			return output;
		}
		
		static void CollectAllBaseTypes(IType type, ITypeResolveContext context, Stack<TypeDefinition> activeTypeDefinitions, List<IType> output)
		{
			TypeDefinition def = type.GetDefinition();
			if (def != null) {
				// Maintain a stack of currently active type definitions, and avoid having one definition
				// multiple times on that stack.
				// This is necessary to ensure the output is finite in the presence of cyclic inheritance:
				// class C<X> : C<C<X>> {} would not be caught by the 'no duplicate output' check, yet would
				// produce infinite output.
				if (activeTypeDefinitions.Contains(def))
					return;
				activeTypeDefinitions.Push(def);
			}
			// Avoid outputting a type more than once - necessary for "diamond" multiple inheritance
			// (e.g. C implements I1 and I2, and both interfaces derive from Object)
			if (!output.Contains(type)) {
				output.Add(type);
				foreach (IType baseType in type.GetBaseTypes(context)) {
					CollectAllBaseTypes(baseType, context, activeTypeDefinitions, output);
				}
			}
			if (def != null)
				activeTypeDefinitions.Pop();
		}
		#endregion
		
		#region GetAllBaseTypeDefinitions
		/// <summary>
		/// Gets all base type definitions.
		/// </summary>
		/// <remarks>
		/// This is equivalent to type.GetAllBaseTypes().Select(t => t.GetDefinition()).Where(d => d != null).Distinct().
		/// </remarks>
		public static IEnumerable<TypeDefinition> GetAllBaseTypeDefinitions(this IType type, ITypeResolveContext context)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (context == null)
				throw new ArgumentNullException("context");
			
			HashSet<TypeDefinition> typeDefinitions = new HashSet<TypeDefinition>();
			Func<TypeDefinition, IEnumerable<TypeDefinition>> recursion =
				t => t.GetBaseTypes(context).Select(b => b.GetDefinition()).Where(d => d != null && typeDefinitions.Add(d));
			
			TypeDefinition typeDef = type as TypeDefinition;
			if (typeDef != null) {
				typeDefinitions.Add(typeDef);
				return TreeTraversal.PreOrder(typeDef, recursion);
			} else {
				return TreeTraversal.PreOrder(
					type.GetBaseTypes(context).Select(b => b.GetDefinition()).Where(d => d != null && typeDefinitions.Add(d)),
					recursion);
			}
		}
		
		/// <summary>
		/// Gets whether this type definition is derived from the base type defintiion.
		/// </summary>
		public static bool IsDerivedFrom(this TypeDefinition type, TypeDefinition baseType, ITypeResolveContext context)
		{
			return GetAllBaseTypeDefinitions(type, context).Contains(baseType);
		}
		#endregion
		
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
		
		/// <summary>
		/// Gets the invoke method for a delegate type.
		/// </summary>
		/// <remarks>
		/// Returns null if the type is not a delegate type; or if the invoke method could not be found.
		/// </remarks>
		public static Method GetDelegateInvokeMethod(this IType type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			TypeDefinition def = type.GetDefinition();
			if (def != null && def.ClassType == ClassType.Delegate) {
				foreach (Method method in def.Methods) {
					if (method.Name == "Invoke")
						return method;
				}
			}
			return null;
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
