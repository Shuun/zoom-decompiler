// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// ParameterizedType represents an instance of a generic type.
	/// Example: List&lt;string&gt;
	/// </summary>
	/// <remarks>
	/// When getting the members, this type modifies the lists so that
	/// type parameters in the signatures of the members are replaced with
	/// the type arguments.
	/// </remarks>
	public sealed class ParameterizedType : Immutable, IType
	{
		sealed class Substitution : TypeVisitor
		{
			readonly IType[] typeArguments;
			
			public Substitution(IType[] typeArguments)
			{
				this.typeArguments = typeArguments;
			}
			
			public override IType VisitTypeParameter(TypeParameter type)
			{
				int index = type.Index;
				if (type.OwnerType == EntityType.TypeDefinition) {
					if (index >= 0 && index < typeArguments.Length)
						return typeArguments[index];
					else
						return SharedTypes.UnknownType;
				} else {
					return base.VisitTypeParameter(type);
				}
			}
		}
		
		readonly TypeDefinition genericType;
		readonly IType[] typeArguments;
		
		public ParameterizedType(TypeDefinition genericType, IEnumerable<IType> typeArguments)
		{
            throw new NotSupportedException("Property and SpecializedProperty classes are disabled.");

			if (genericType == null)
				throw new ArgumentNullException("genericType");
			if (typeArguments == null)
				throw new ArgumentNullException("typeArguments");
			this.genericType = genericType;
			this.typeArguments = typeArguments.ToArray(); // copy input array to ensure it isn't modified
			if (this.typeArguments.Length == 0)
				throw new ArgumentException("Cannot use ParameterizedType with 0 type arguments.");
			if (genericType.TypeParameterCount != this.typeArguments.Length)
				throw new ArgumentException("Number of type arguments must match the type definition's number of type parameters");
			for (int i = 0; i < this.typeArguments.Length; i++) {
				if (this.typeArguments[i] == null)
					throw new ArgumentNullException("typeArguments[" + i + "]");
			}
		}
		
		/// <summary>
		/// Fast internal version of the constructor. (no safety checks)
		/// Keeps the array that was passed and assumes it won't be modified.
		/// </summary>
		internal ParameterizedType(TypeDefinition genericType, IType[] typeArguments)
		{
			Debug.Assert(genericType.TypeParameterCount == typeArguments.Length);
			this.genericType = genericType;
			this.typeArguments = typeArguments;
		}
		
		public bool? IsReferenceType {
			get { return genericType.IsReferenceType; }
		}
		
		public IType DeclaringType {
			get {
				TypeDefinition declaringTypeDef = genericType.DeclaringTypeDefinition;
				if (declaringTypeDef != null && declaringTypeDef.TypeParameterCount > 0) {
					IType[] newTypeArgs = new IType[declaringTypeDef.TypeParameterCount];
					Array.Copy(this.typeArguments, 0, newTypeArgs, 0, newTypeArgs.Length);
					return new ParameterizedType(declaringTypeDef, newTypeArgs);
				}
				return declaringTypeDef;
			}
		}
		
		public int TypeParameterCount {
			get { return genericType.TypeParameterCount; }
		}
		
		public string FullName {
			get { return genericType.FullName; }
		}
		
		public string Name {
			get { return genericType.Name; }
		}
		
		public string Namespace {
			get { return genericType.Namespace;}
		}
		
		public string ReflectionName {
			get {
				StringBuilder b = new StringBuilder(genericType.ReflectionName);
				b.Append('[');
				for (int i = 0; i < typeArguments.Length; i++) {
					if (i > 0)
						b.Append(',');
					b.Append('[');
					b.Append(typeArguments[i].ReflectionName);
					b.Append(']');
				}
				b.Append(']');
				return b.ToString();
			}
		}
		
		public override string ToString()
		{
			return ReflectionName;
		}
		
		public ReadOnlyCollection<IType> TypeArguments {
			get {
				return Array.AsReadOnly(typeArguments);
			}
		}
		
		public TypeDefinition GetDefinition()
		{
			return genericType;
		}
		
		public IType Resolve(ITypeResolveContext context)
		{
			return this;
		}
		
		/// <summary>
		/// Substitutes the class type parameters in the <paramref name="type"/> with the
		/// type arguments of this parameterized type.
		/// </summary>
		public IType SubstituteInType(IType type)
		{
			return type.AcceptVisitor(new Substitution(typeArguments));
		}
		
		public IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
			Substitution substitution = new Substitution(typeArguments);
			return genericType.GetBaseTypes(context).Select(t => t.AcceptVisitor(substitution));
		}
		
		public IEnumerable<IType> GetNestedTypes(ITypeResolveContext context, Predicate<TypeDefinition> filter = null)
		{
			/*
			class Base<T> {
				class Nested {}
			}
			class Derived<A, B> : Base<B> {}
			
			Derived<string,int>.GetNestedTypes() = { Base`1+Nested<int> }
			Derived.GetNestedTypes() = { Base`1+Nested<B> }
			Base<B>.GetNestedTypes() = { Base`1+Nested<B> }
			Base.GetNestedTypes() = { Base`1+Nested<T2> } where T2 = copy of T in Base`1+Nested
			 */
			Substitution substitution = new Substitution(typeArguments);
			List<IType> types = genericType.GetNestedTypes(context, filter).ToList();
			for (int i = 0; i < types.Count; i++) {
				TypeDefinition def = types[i] as TypeDefinition;
				if (def != null && def.TypeParameterCount > 0) {
					// (partially) parameterize the nested type definition
					IType[] newTypeArgs = new IType[def.TypeParameterCount];
					for (int j = 0; j < newTypeArgs.Length; j++) {
						if (j < typeArguments.Length)
							newTypeArgs[j] = typeArguments[i];
						else
							newTypeArgs[j] = def.TypeParameters[j];
					}
					types[i] = new ParameterizedType(def, newTypeArgs);
				} else {
					types[i] = types[i].AcceptVisitor(substitution);
				}
			}
			return types;
		}
		
		public IEnumerable<Property> GetProperties(ITypeResolveContext context, Predicate<Property> filter = null)
		{
			Substitution substitution = new Substitution(typeArguments);
			List<Property> properties = genericType.GetProperties(context, filter).ToList();
			for (int i = 0; i < properties.Count; i++) {
                throw new NotSupportedException();
			}
			return properties;
		}
		
		public IEnumerable<Event> GetEvents(ITypeResolveContext context, Predicate<Event> filter = null)
		{
			Substitution substitution = new Substitution(typeArguments);
			List<Event> events = genericType.GetEvents(context, filter).ToList();
			for (int i = 0; i < events.Count; i++) {
                throw new NotSupportedException("Event class (as well as SpecializedEvent class) is removed.");
			}
			return events;
		}
		
		public override bool Equals(object obj)
		{
			return Equals(obj as IType);
		}
		
		public bool Equals(IType other)
		{
			ParameterizedType c = other as ParameterizedType;
			if (c == null || !genericType.Equals(c.genericType) || typeArguments.Length != c.typeArguments.Length)
				return false;
			for (int i = 0; i < typeArguments.Length; i++) {
				if (!typeArguments[i].Equals(c.typeArguments[i]))
					return false;
			}
			return true;
		}
		
		public override int GetHashCode()
		{
			int hashCode = genericType.GetHashCode();
			unchecked {
				foreach (var ta in typeArguments) {
					hashCode *= 1000000007;
					hashCode += 1000000009 * ta.GetHashCode();
				}
			}
			return hashCode;
		}
		
		public IType AcceptVisitor(TypeVisitor visitor)
		{
			return visitor.VisitParameterizedType(this);
		}
		
		public IType VisitChildren(TypeVisitor visitor)
		{
			IType g = genericType.AcceptVisitor(visitor);
			TypeDefinition def = g as TypeDefinition;
			if (def == null)
				return g;
			// Keep ta == null as long as no elements changed, allocate the array only if necessary.
			IType[] ta = (g != genericType) ? new IType[typeArguments.Length] : null;
			for (int i = 0; i < typeArguments.Length; i++) {
				IType r = typeArguments[i].AcceptVisitor(visitor);
				if (r == null)
					throw new NullReferenceException("TypeVisitor.Visit-method returned null");
				if (ta == null && r != typeArguments[i]) {
					// we found a difference, so we need to allocate the array
					ta = new IType[typeArguments.Length];
					for (int j = 0; j < i; j++) {
						ta[j] = typeArguments[j];
					}
				}
				if (ta != null)
					ta[i] = r;
			}
			if (ta == null)
				return this;
			else
				return new ParameterizedType(def, ta);
		}
	}
	
	/// <summary>
	/// ParameterizedTypeReference is a reference to generic class that specifies the type parameters.
	/// Example: List&lt;string&gt;
	/// </summary>
	public sealed class ParameterizedTypeReference : ITypeReference, ISupportsInterning
	{
		public static ITypeReference Create(ITypeReference genericType, IEnumerable<ITypeReference> typeArguments)
		{
			if (genericType == null)
				throw new ArgumentNullException("genericType");
			if (typeArguments == null)
				throw new ArgumentNullException("typeArguments");
			
			ITypeReference[] typeArgs = typeArguments.ToArray();
			if (typeArgs.Length == 0) {
				return genericType;
			} else if (genericType is TypeDefinition && typeArgs.All(t => t is IType)) {
				IType[] ta = new IType[typeArgs.Length];
				for (int i = 0; i < ta.Length; i++) {
					ta[i] = (IType)typeArgs[i];
				}
				return new ParameterizedType((TypeDefinition)genericType, ta);
			} else {
				return new ParameterizedTypeReference(genericType, typeArgs);
			}
		}
		
		ITypeReference genericType;
		ITypeReference[] typeArguments;
		
		public ParameterizedTypeReference(ITypeReference genericType, IEnumerable<ITypeReference> typeArguments)
		{
			if (genericType == null)
				throw new ArgumentNullException("genericType");
			if (typeArguments == null)
				throw new ArgumentNullException("typeArguments");
			this.genericType = genericType;
			this.typeArguments = typeArguments.ToArray();
			for (int i = 0; i < this.typeArguments.Length; i++) {
				if (this.typeArguments[i] == null)
					throw new ArgumentNullException("typeArguments[" + i + "]");
			}
		}
		
		public ITypeReference GenericType {
			get { return genericType; }
		}
		
		public ReadOnlyCollection<ITypeReference> TypeArguments {
			get {
				return Array.AsReadOnly(typeArguments);
			}
		}
		
		public IType Resolve(ITypeResolveContext context)
		{
			TypeDefinition baseTypeDef = genericType.Resolve(context).GetDefinition();
			if (baseTypeDef == null)
				return SharedTypes.UnknownType;
			int tpc = baseTypeDef.TypeParameterCount;
			if (tpc == 0)
				return baseTypeDef;
			IType[] resolvedTypes = new IType[tpc];
			for (int i = 0; i < resolvedTypes.Length; i++) {
				if (i < typeArguments.Length)
					resolvedTypes[i] = typeArguments[i].Resolve(context);
				else
					resolvedTypes[i] = SharedTypes.UnknownType;
			}
			return new ParameterizedType(baseTypeDef, resolvedTypes);
		}
		
		public override string ToString()
		{
			StringBuilder b = new StringBuilder(genericType.ToString());
			b.Append('[');
			for (int i = 0; i < typeArguments.Length; i++) {
				if (i > 0)
					b.Append(',');
				b.Append('[');
				b.Append(typeArguments[i].ToString());
				b.Append(']');
			}
			b.Append(']');
			return b.ToString();
		}
		
		void ISupportsInterning.PrepareForInterning(IInterningProvider provider)
		{
			genericType = provider.Intern(genericType);
			for (int i = 0; i < typeArguments.Length; i++) {
				typeArguments[i] = provider.Intern(typeArguments[i]);
			}
		}
		
		int ISupportsInterning.GetHashCodeForInterning()
		{
			int hashCode = genericType.GetHashCode();
			unchecked {
				foreach (ITypeReference t in typeArguments) {
					hashCode *= 27;
					hashCode += t.GetHashCode();
				}
			}
			return hashCode;
		}
		
		bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
		{
			ParameterizedTypeReference o = other as ParameterizedTypeReference;
			if (o != null && genericType == o.genericType && typeArguments.Length == o.typeArguments.Length) {
				for (int i = 0; i < typeArguments.Length; i++) {
					if (typeArguments[i] != o.typeArguments[i])
						return false;
				}
				return true;
			}
			return false;
			
		}
	}
}
