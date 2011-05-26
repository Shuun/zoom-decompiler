// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

using Mi.NRefactory.Utils;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Default implementation of <see cref="ITypeParameter"/>.
	/// </summary>
	public sealed class DefaultTypeParameter : AbstractFreezable, ITypeParameter, ISupportsInterning
	{
		string name;
		int index;
		IList<ITypeReference> constraints;
		IList<IAttribute> attributes;
		
		DomRegion region;
		
		// Small fields: byte+byte+short
		VarianceModifier variance;
		EntityType ownerType;
		ParameterConstraint flags;

        [Flags]
        private enum ParameterConstraint
        {
            ReferenceType = 1,
            ValueType = 2,
            DefaultConstructor = 4
        }
		
		protected override void FreezeInternal()
		{
			constraints = FreezeList(constraints);
			attributes = FreezeList(attributes);
			base.FreezeInternal();
		}
		
		public DefaultTypeParameter(EntityType ownerType, int index, string name)
		{
			if (!(ownerType == EntityType.TypeDefinition || ownerType == EntityType.Method))
				throw new ArgumentException("owner must be a type or a method", "ownerType");
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");
			if (name == null)
				throw new ArgumentNullException("name");
			this.ownerType = ownerType;
			this.index = index;
			this.name = name;
		}
		
		public string Name {
			get { return name; }
		}
		
		string INamedElement.FullName {
			get { return name; }
		}
		
		string INamedElement.Namespace {
			get { return string.Empty; }
		}
		
		public string ReflectionName {
			get {
				if (ownerType == EntityType.Method)
					return "``" + index.ToString();
				else
					return "`" + index.ToString();
			}
		}
		
		public bool? IsReferenceType {
			get {
				switch (flags & (ParameterConstraint.ReferenceType | ParameterConstraint.ValueType)) {
					case ParameterConstraint.ReferenceType:
						return true;
					case ParameterConstraint.ValueType:
						return false;
					default:
						return null;
				}
			}
		}
		
		int IType.TypeParameterCount {
			get { return 0; }
		}
		
		IType IType.DeclaringType {
			get { return null; }
		}
		
		ITypeDefinition IType.GetDefinition()
		{
			return null;
		}
		
		IType ITypeReference.Resolve(ITypeResolveContext context)
		{
			return this;
		}
		
		public override int GetHashCode()
		{
			unchecked {
				return (int)ownerType * 178256151 + index;
			}
		}
		
		public override bool Equals(object obj)
		{
			return Equals(obj as IType);
		}
		
		public bool Equals(IType other)
		{
			DefaultTypeParameter p = other as DefaultTypeParameter;
			if (p == null)
				return false;
			return ownerType == p.ownerType && index == p.index;
		}
		
		public EntityType OwnerType {
			get {
				return ownerType;
			}
		}
		
		public int Index {
			get { return index; }
		}
		
		public IList<IAttribute> Attributes {
			get {
				if (attributes == null)
					attributes = new List<IAttribute>();
				return attributes;
			}
		}
		
		public IList<ITypeReference> Constraints {
			get {
				if (constraints == null)
					constraints = new List<ITypeReference>();
				return constraints;
			}
		}
		
		public bool HasDefaultConstructorConstraint {
			get { return (flags & ParameterConstraint.DefaultConstructor)!=0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                    flags | ParameterConstraint.DefaultConstructor :
                    flags & ~ParameterConstraint.DefaultConstructor;
			}
		}
		
		public bool HasReferenceTypeConstraint {
            get { return (flags & ParameterConstraint.ReferenceType) != 0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                   flags | ParameterConstraint.ReferenceType :
                   flags & ~ParameterConstraint.ReferenceType;
			}
		}
		
		public bool HasValueTypeConstraint {
            get { return (flags & ParameterConstraint.ValueType) != 0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                   flags | ParameterConstraint.ValueType :
                   flags & ~ParameterConstraint.ValueType;
			}
		}
		
		public VarianceModifier Variance {
			get { return variance; }
			set {
				CheckBeforeMutation();
				variance = value;
			}
		}
		
		public DomRegion Region {
			get { return region; }
			set {
				CheckBeforeMutation();
				region = value;
			}
		}
		
		IType ITypeParameter.BoundTo {
			get { return null; }
		}
		
		ITypeParameter ITypeParameter.UnboundTypeParameter {
			get { return null; }
		}
		
		public IType AcceptVisitor(TypeVisitor visitor)
		{
			return visitor.VisitTypeParameter(this);
		}
		
		public IType VisitChildren(TypeVisitor visitor)
		{
			return this;
		}
		
		static readonly SimpleProjectContent dummyProjectContent = new SimpleProjectContent();
		
		DefaultTypeDefinition GetDummyClassForTypeParameter()
		{
			DefaultTypeDefinition c = new DefaultTypeDefinition(dummyProjectContent, string.Empty, this.Name);
			c.Region = this.Region;
			if (HasValueTypeConstraint) {
				c.ClassType = ClassType.Struct;
			} else if (HasDefaultConstructorConstraint) {
				c.ClassType = ClassType.Class;
			} else {
				c.ClassType = ClassType.Interface;
			}
			return c;
		}
		
		public IEnumerable<IMethod> GetConstructors(ITypeResolveContext context, Predicate<IMethod> filter = null)
		{
			if (HasDefaultConstructorConstraint || HasValueTypeConstraint) {
				DefaultMethod m = DefaultMethod.CreateDefaultConstructor(GetDummyClassForTypeParameter());
				if (filter(m))
					return new [] { m };
			}
            return Empty.ReadOnlyCollection<IMethod>();
		}
		
		public IEnumerable<IMethod> GetMethods(ITypeResolveContext context, Predicate<IMethod> filter = null)
		{
			// TODO: get methods from constraints
			IType objectType = context.GetClass("System", "Object", 0, StringComparer.Ordinal);
			IEnumerable<IMethod> objectMethods;
			if (objectType != null)
				objectMethods = objectType.GetMethods(context, filter);
			else
                objectMethods = Empty.ReadOnlyCollection<IMethod>();
			
			// don't return static methods (those cannot be called from type parameter)
			return objectMethods.Where(m => !m.IsStatic);
		}
		
		public IEnumerable<IProperty> GetProperties(ITypeResolveContext context, Predicate<IProperty> filter = null)
		{
            return Empty.ReadOnlyCollection<IProperty>();
		}
		
		public IEnumerable<IField> GetFields(ITypeResolveContext context, Predicate<IField> filter = null)
		{
            return Empty.ReadOnlyCollection<IField>();
		}
		
		public IEnumerable<Event> GetEvents(ITypeResolveContext context, Predicate<Event> filter = null)
		{
            return Empty.ReadOnlyCollection<Event>();
		}
		
		IEnumerable<IType> IType.GetNestedTypes(ITypeResolveContext context, Predicate<ITypeDefinition> filter)
		{
            return Empty.ReadOnlyCollection<IType>();
		}
		
		public IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
			IType defaultBaseType = context.GetClass("System", HasValueTypeConstraint ? "ValueType" : "Object", 0, StringComparer.Ordinal);
			if (defaultBaseType != null)
				yield return defaultBaseType;
			
			foreach (ITypeReference constraint in this.Constraints) {
				yield return constraint.Resolve(context);
			}
		}
		
		void ISupportsInterning.PrepareForInterning(IInterningProvider provider)
		{
			constraints = provider.InternList(constraints);
			attributes = provider.InternList(attributes);
		}
		
		int ISupportsInterning.GetHashCodeForInterning()
		{
			unchecked {
				int hashCode = GetHashCode();
				if (name != null)
					hashCode += name.GetHashCode();
				if (attributes != null)
					hashCode += attributes.GetHashCode();
				if (constraints != null)
					hashCode += constraints.GetHashCode();
				hashCode += 771 * (int)flags + 900103 * (int)variance;
				return hashCode;
			}
		}
		
		bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
		{
			DefaultTypeParameter o = other as DefaultTypeParameter;
			return o != null 
				&& this.attributes == o.attributes
				&& this.constraints == o.constraints
				&& this.flags == o.flags
				&& this.ownerType == o.ownerType
				&& this.index == o.index
				&& this.variance == o.variance;
		}
		
		public override string ToString()
		{
			return this.ReflectionName;
		}
	}
}
