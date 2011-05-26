// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Represents an array type.
	/// </summary>
	public class ArrayType : TypeWithElementType
	{
		readonly int dimensions;
		
		public ArrayType(IType elementType, int dimensions = 1) : base(elementType)
		{
			if (dimensions <= 0)
				throw new ArgumentOutOfRangeException("dimensions");
			this.dimensions = dimensions;
		}
		
		public int Dimensions {
			get { return dimensions; }
		}
		
		public override string NameSuffix {
			get {
				return "[" + new string(',', dimensions-1) + "]";
			}
		}
		
		public override Nullable<bool> IsReferenceType {
			get { return true; }
		}
		
		public override int GetHashCode()
		{
			return unchecked(elementType.GetHashCode() * 71681 + dimensions);
		}
		
		public override bool Equals(IType other)
		{
			ArrayType a = other as ArrayType;
			return a != null && elementType.Equals(a.elementType) && a.dimensions == dimensions;
		}
		
		static readonly GetClassTypeReference systemArray = new GetClassTypeReference("System", "Array", 0);
		static readonly GetClassTypeReference listInterface = new GetClassTypeReference("System.Collections.Generic", "IList", 1);
		
		public override IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
			List<IType> baseTypes = new List<IType>();
			IType t = systemArray.Resolve(context);
			if (t != SharedTypes.UnknownType)
				baseTypes.Add(t);
			if (dimensions == 1) { // single-dimensional arrays implement IList<T>
				TypeDefinition def = listInterface.Resolve(context) as TypeDefinition;
				if (def != null)
					baseTypes.Add(new ParameterizedType(def, new[] { elementType }));
			}
			return baseTypes;
		}
		
		public override IEnumerable<Method> GetMethods(ITypeResolveContext context, Predicate<Method> filter = null)
		{
			return systemArray.Resolve(context).GetMethods(context, filter);
		}
		
		static readonly Parameter indexerParam = new Parameter(KnownTypeReference.Int32, string.Empty);
		
		public override IEnumerable<Property> GetProperties(ITypeResolveContext context, Predicate<Property> filter = null)
		{
			TypeDefinition arrayDef = systemArray.Resolve(context) as TypeDefinition;
			if (arrayDef != null) {
				foreach (Property p in arrayDef.GetProperties(context, filter)) {
					yield return p;
				}
				Property indexer = new Property(arrayDef, "Items") {
					EntityType = EntityType.Indexer,
					ReturnType = elementType,
					Accessibility = Accessibility.Public,
					Getter = Accessor.GetFromAccessibility(Accessibility.Public),
					Setter = Accessor.GetFromAccessibility(Accessibility.Public),
					IsSynthetic = true
				};
				for (int i = 0; i < dimensions; i++) {
					indexer.Parameters.Add(indexerParam);
				}
				indexer.Freeze();
				if (filter == null || filter(indexer)) {
					yield return indexer;
				}
			}
		}
		
		// Events, Fields: System.Array doesn't have any; so we can use the AbstractType default implementation
		// that simply returns an empty list
		
		public override IType AcceptVisitor(TypeVisitor visitor)
		{
			return visitor.VisitArrayType(this);
		}
		
		public override IType VisitChildren(TypeVisitor visitor)
		{
			IType e = elementType.AcceptVisitor(visitor);
			if (e == elementType)
				return this;
			else
				return new ArrayType(e, dimensions);
		}
	}
	
	public sealed class ArrayTypeReference : ITypeReference, ISupportsInterning
	{
		ITypeReference elementType;
		int dimensions;
		
		public ArrayTypeReference(ITypeReference elementType, int dimensions = 1)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");
			if (dimensions <= 0)
				throw new ArgumentOutOfRangeException("dimensions");
			this.elementType = elementType;
			this.dimensions = dimensions;
		}
		
		public ITypeReference ElementType {
			get { return elementType; }
		}
		
		public int Dimensions {
			get { return dimensions; }
		}
		
		public IType Resolve(ITypeResolveContext context)
		{
			return new ArrayType(elementType.Resolve(context), dimensions);
		}
		
		public override string ToString()
		{
			return elementType.ToString() + "[" + new string(',', dimensions - 1) + "]";
		}
		
		public static ITypeReference Create(ITypeReference elementType, int dimensions)
		{
			if (elementType is IType)
				return new ArrayType((IType)elementType, dimensions);
			else
				return new ArrayTypeReference(elementType, dimensions);
		}
		
		void ISupportsInterning.PrepareForInterning(IInterningProvider provider)
		{
			elementType = provider.Intern(elementType);
		}
		
		int ISupportsInterning.GetHashCodeForInterning()
		{
			return elementType.GetHashCode() ^ dimensions;
		}
		
		bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
		{
			ArrayTypeReference o = other as ArrayTypeReference;
			return o != null && elementType == o.elementType && dimensions == o.dimensions;
		}
	}
}
