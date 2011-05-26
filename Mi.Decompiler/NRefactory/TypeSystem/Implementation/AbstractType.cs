// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Default implementation for IType interface.
	/// </summary>
	public abstract class AbstractType : IType
	{
		public virtual string FullName {
			get {
				string ns = this.Namespace;
				string name = this.Name;
				if (string.IsNullOrEmpty(ns)) {
					return name;
				} else {
					return ns + "." + name;
				}
			}
		}
		
		public abstract string Name { get; }
		
		public virtual string Namespace {
			get { return string.Empty; }
		}
		
		public virtual string ReflectionName {
			get { return this.FullName; }
		}
		
		public abstract bool? IsReferenceType { get; }
		
		public virtual int TypeParameterCount {
			get { return 0; }
		}
		
		public virtual IType DeclaringType {
			get { return null; }
		}
		
		public virtual TypeDefinition GetDefinition()
		{
			return null;
		}
		
		IType ITypeReference.Resolve(ITypeResolveContext context)
		{
			return this;
		}
		
		public virtual IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
            return Empty.ReadOnlyCollection<IType>();
		}
		
		public virtual IEnumerable<IType> GetNestedTypes(ITypeResolveContext context, Predicate<TypeDefinition> filter = null)
		{
            return Empty.ReadOnlyCollection<IType>();
		}
		
		public virtual IEnumerable<Method> GetMethods(ITypeResolveContext context, Predicate<Method> filter = null)
		{
            return Empty.ReadOnlyCollection<Method>();
		}
		
		public virtual IEnumerable<Method> GetConstructors(ITypeResolveContext context, Predicate<Method> filter = null)
		{
            return Empty.ReadOnlyCollection<Method>();
		}
		
		public virtual IEnumerable<Property> GetProperties(ITypeResolveContext context, Predicate<Property> filter = null)
		{
            return Empty.ReadOnlyCollection<Property>();
		}
		
		public virtual IEnumerable<Field> GetFields(ITypeResolveContext context, Predicate<Field> filter = null)
		{
            return Empty.ReadOnlyCollection<Field>();
		}
		
		public virtual IEnumerable<Event> GetEvents(ITypeResolveContext context, Predicate<Event> filter = null)
		{
            return Empty.ReadOnlyCollection<Event>();
		}
		
		public override bool Equals(object obj)
		{
			return Equals(obj as IType);
		}
		
		public abstract override int GetHashCode();
		public abstract bool Equals(IType other);
		
		public override string ToString()
		{
			return this.ReflectionName;
		}
		
		public virtual IType AcceptVisitor(TypeVisitor visitor)
		{
			return visitor.VisitOtherType(this);
		}
		
		public virtual IType VisitChildren(TypeVisitor visitor)
		{
			return this;
		}
	}
}
