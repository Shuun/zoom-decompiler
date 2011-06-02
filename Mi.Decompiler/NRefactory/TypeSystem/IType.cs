﻿// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Mi.NRefactory.TypeSystem
{
	#if WITH_CONTRACTS
	[ContractClass(typeof(ITypeContract))]
	#endif
	public interface IType : ITypeReference, INamedElement, IEquatable<IType>
	{
		/// <summary>
		/// Gets whether the type is a reference type or value type.
		/// </summary>
		/// <returns>
		/// true, if the type is a reference type.
		/// false, if the type is a value type.
		/// null, if the type is not known (e.g. unconstrained generic type parameter or type not found)
		/// </returns>
		bool? IsReferenceType { get; }
		
		/// <summary>
		/// Gets the underlying type definition.
		/// Can return null for types which do not have a type definition (for example arrays, pointers, type parameters)
		/// </summary>
		TypeDefinition GetDefinition();
		
		/// <summary>
		/// Gets the parent type, if this is a nested type.
		/// Returns null for top-level types.
		/// </summary>
		IType DeclaringType { get; }
		
		/// <summary>
		/// Calls ITypeVisitor.Visit for this type.
		/// </summary>
		/// <returns>The return value of the ITypeVisitor.Visit call</returns>
		IType AcceptVisitor(TypeVisitor visitor);
		
		/// <summary>
		/// Calls ITypeVisitor.Visit for all children of this type, and reconstructs this type with the children based
		/// on the return values of the visit calls.
		/// </summary>
		/// <returns>A copy of this type, with all children replaced by the return value of the corresponding visitor call.
		/// If the visitor returned the original types for all children (or if there are no children), returns <c>this</c>.
		/// </returns>
		IType VisitChildren(TypeVisitor visitor);
		
		/// <summary>
		/// Gets the direct base types.
		/// </summary>
		IEnumerable<IType> GetBaseTypes(ITypeResolveContext context);
		
		/// <summary>
		/// Gets inner classes (including inherited inner classes).
		/// </summary>
		IEnumerable<IType> GetNestedTypes(ITypeResolveContext context, Predicate<TypeDefinition> filter = null);
	}
	
	#if WITH_CONTRACTS
	[ContractClassFor(typeof(IType))]
	abstract class ITypeContract : ITypeReferenceContract, IType
	{
		Nullable<bool> IType.IsReferenceType {
			get { return null; }
		}
		
		int IType.TypeParameterCount {
			get {
				Contract.Ensures(Contract.Result<int>() >= 0);
				return 0;
			}
		}
		
		IType IType.DeclaringType {
			get { return null; }
		}
		
		IEnumerable<IType> IType.GetBaseTypes(ITypeResolveContext context)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IEnumerable<IType>>() != null);
			return null;
		}
		
		IEnumerable<IType> IType.GetNestedTypes(ITypeResolveContext context, Predicate<ITypeDefinition> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IList<IType>>() != null);
			return null;
		}

		IEnumerable<IMethod> IType.GetMethods(ITypeResolveContext context, Predicate<IMethod> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IList<IMethod>>() != null);
			return null;
		}
		
		IEnumerable<IMethod> IType.GetConstructors(ITypeResolveContext context, Predicate<IMethod> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IList<IMethod>>() != null);
			return null;
		}
		
		IEnumerable<IProperty> IType.GetProperties(ITypeResolveContext context, Predicate<IProperty> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IList<IProperty>>() != null);
			return null;
		}
		
		IEnumerable<IField> IType.GetFields(ITypeResolveContext context, Predicate<IField> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IList<IField>>() != null);
			return null;
		}
		
		IEnumerable<IEvent> IType.GetEvents(ITypeResolveContext context, Predicate<IEvent> filter)
		{
			Contract.Requires(context != null);
			Contract.Ensures(Contract.Result<IList<IEvent>>() != null);
			return null;
		}
		
		string INamedElement.FullName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		string INamedElement.Name {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		string INamedElement.Namespace {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		string INamedElement.ReflectionName {
			get {
				Contract.Ensures(Contract.Result<string>() != null);
				return null;
			}
		}
		
		ITypeDefinition IType.GetDefinition()
		{
			return null;
		}
		
		bool IEquatable<IType>.Equals(IType other)
		{
			return false;
		}
		
		IType IType.AcceptVisitor(TypeVisitor visitor)
		{
			Contract.Requires(visitor != null);
			Contract.Ensures(Contract.Result<IType>() != null);
			return this;
		}
		
		IType IType.VisitChildren(TypeVisitor visitor)
		{
			Contract.Requires(visitor != null);
			Contract.Ensures(Contract.Result<IType>() != null);
			return this;
		}
	}
	#endif
}
