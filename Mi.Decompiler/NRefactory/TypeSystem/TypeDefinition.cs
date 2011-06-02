// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

using Mi.NRefactory.Utils;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
    public class TypeDefinition : AbstractFreezable, IType, IEntity
	{
		readonly ITypeResolveContext projectContent;
		readonly TypeDefinition declaringTypeDefinition;
		
		string ns;
		string name;
		
		IList<ITypeReference> baseTypes;
		IList<TypeParameter> typeParameters;
		IList<TypeDefinition> innerClasses;
		
		// 1 byte per enum + 2 bytes for flags
		ClassType classType;
		Accessibility accessibility;
		TypeDefinitionFlags flags;

        [Flags]
        private enum TypeDefinitionFlags
        {
		    Sealed    = 0x0001,
		    Abstract  = 0x0002,
		    Shadowing = 0x0004,
		    Synthetic = 0x0008,
		    AddDefaultConstructorIfRequired = 0x0010,
		    HasExtensionMethods = 0x0020
        }
		
		protected override void FreezeInternal()
		{
			baseTypes = FreezeList(baseTypes);
			typeParameters = FreezeList(typeParameters);
			innerClasses = FreezeList(innerClasses);
			base.FreezeInternal();
		}
		
		public TypeDefinition(TypeDefinition declaringTypeDefinition, string name)
		{
			if (declaringTypeDefinition == null)
				throw new ArgumentNullException("declaringTypeDefinition");
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name");
			this.projectContent = declaringTypeDefinition.ProjectContent;
			this.declaringTypeDefinition = declaringTypeDefinition;
			this.name = name;
			this.ns = declaringTypeDefinition.Namespace;
		}
		
		public TypeDefinition(ITypeResolveContext projectContent, string ns, string name)
		{
			if (projectContent == null)
				throw new ArgumentNullException("projectContent");
			if (string.IsNullOrEmpty(name))
				throw new ArgumentException("name");
			this.projectContent = projectContent;
			this.ns = ns ?? string.Empty;
			this.name = name;
		}
		
		public ClassType ClassType {
			get { return classType; }
			set {
				CheckBeforeMutation();
				classType = value;
			}
		}
		
		public IList<ITypeReference> BaseTypes {
			get {
				if (baseTypes == null)
					baseTypes = new List<ITypeReference>();
				return baseTypes;
			}
		}
		
		public void ApplyInterningProvider(IInterningProvider provider)
		{
			if (provider != null) {
				ns = provider.Intern(ns);
				name = provider.Intern(name);
				baseTypes = provider.InternList(baseTypes);
				typeParameters = provider.InternList(typeParameters);
			}
		}
		
		public IList<TypeParameter> TypeParameters {
			get {
				if (typeParameters == null)
					typeParameters = new List<TypeParameter>();
				return typeParameters;
			}
		}
		
		public IList<TypeDefinition> InnerClasses {
			get {
				if (innerClasses == null)
					innerClasses = new List<TypeDefinition>();
				return innerClasses;
			}
		}
		
		public IEnumerable<IMember> Members {
			get {
				return Enumerable.Empty<IMember>();
			}
		}
		
		public bool? IsReferenceType {
			get {
				switch (this.ClassType) {
					case ClassType.Class:
					case ClassType.Interface:
					case ClassType.Delegate:
						return true;
					case ClassType.Enum:
					case ClassType.Struct:
						return false;
					default:
						return null;
				}
			}
		}
		
		public string FullName {
			get {
				if (declaringTypeDefinition != null) {
					return declaringTypeDefinition.FullName + "." + this.name;
				} else if (string.IsNullOrEmpty(ns)) {
					return this.name;
				} else {
					return this.ns + "." + this.name;
				}
			}
		}
		
		public string Name {
			get { return this.name; }
		}
		
		public string Namespace {
			get { return this.ns; }
		}
		
		public string ReflectionName {
			get {
				if (declaringTypeDefinition != null) {
					int tpCount = this.TypeParameterCount - declaringTypeDefinition.TypeParameterCount;
					string combinedName;
					if (tpCount > 0)
						combinedName = declaringTypeDefinition.ReflectionName + "+" + this.Name + "`" + tpCount.ToString(CultureInfo.InvariantCulture);
					else
						combinedName = declaringTypeDefinition.ReflectionName + "+" + this.Name;
					return combinedName;
				} else {
					int tpCount = this.TypeParameterCount;
					if (string.IsNullOrEmpty(ns)) {
						if (tpCount > 0)
							return this.Name + "`" + tpCount.ToString(CultureInfo.InvariantCulture);
						else
							return this.Name;
					} else {
						if (tpCount > 0)
							return this.Namespace + "." + this.Name + "`" + tpCount.ToString(CultureInfo.InvariantCulture);
						else
							return this.Namespace + "." + this.Name;
					}
				}
			}
		}
		
		public int TypeParameterCount {
			get { return typeParameters != null ? typeParameters.Count : 0; }
		}
		
		public EntityType EntityType {
			get { return EntityType.TypeDefinition; }
		}
		
		public TypeDefinition DeclaringTypeDefinition {
			get { return declaringTypeDefinition; }
		}
		
		public IType DeclaringType {
			get { return declaringTypeDefinition; }
		}
		
		public virtual string Documentation {
			get { return null; }
		}
		
		public Accessibility Accessibility {
			get { return accessibility; }
			set {
				CheckBeforeMutation();
				accessibility = value;
			}
		}
		
		public bool IsStatic {
			get { return IsAbstract && IsSealed; }
		}
		
		public bool IsAbstract {
			get { return (flags & TypeDefinitionFlags.Abstract)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | TypeDefinitionFlags.Abstract :
                    flags & ~TypeDefinitionFlags.Abstract;
			}
		}
		
		public bool IsSealed {
			get { return (flags & TypeDefinitionFlags.Sealed) !=0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                    flags | TypeDefinitionFlags.Sealed :
                    flags & ~TypeDefinitionFlags.Sealed;
			}
		}
		
		public bool IsShadowing {
            get { return (flags & TypeDefinitionFlags.Shadowing) != 0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                   flags | TypeDefinitionFlags.Shadowing :
                   flags & ~TypeDefinitionFlags.Shadowing;
			}
		}
		
		public bool IsSynthetic {
            get { return (flags & TypeDefinitionFlags.Synthetic) != 0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                  flags | TypeDefinitionFlags.Synthetic :
                  flags & ~TypeDefinitionFlags.Synthetic;
			}
		}
		
		public bool HasExtensionMethods {
            get { return (flags & TypeDefinitionFlags.HasExtensionMethods) != 0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                    flags | TypeDefinitionFlags.HasExtensionMethods :
                    flags & ~TypeDefinitionFlags.HasExtensionMethods;
			}
		}
		
		public ITypeResolveContext ProjectContent {
			get { return projectContent; }
		}
		
		public IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
			bool hasNonInterface = false;
			if (baseTypes != null && this.ClassType != ClassType.Enum) {
				foreach (ITypeReference baseTypeRef in baseTypes) {
					IType baseType = baseTypeRef.Resolve(context);
					TypeDefinition baseTypeDef = baseType.GetDefinition();
					if (baseTypeDef == null || baseTypeDef.ClassType != ClassType.Interface)
						hasNonInterface = true;
					yield return baseType;
				}
			}
			if (!hasNonInterface && !(this.Name == "Object" && this.Namespace == "System" && this.TypeParameterCount == 0)) {
				Type primitiveBaseType;
				switch (classType) {
					case ClassType.Enum:
						primitiveBaseType = typeof(Enum);
						break;
					case ClassType.Struct:
						primitiveBaseType = typeof(ValueType);
						break;
					case ClassType.Delegate:
						primitiveBaseType = typeof(Delegate);
						break;
					default:
						primitiveBaseType = typeof(object);
						break;
				}
                IType t = GetClass(context, primitiveBaseType);
				if (t != null)
					yield return t;
			}
		}

        public TypeDefinition GetClass(ITypeResolveContext context, Type type)
        {
            if (type == null)
                return null;
            while (type.IsArray || type.IsPointer || type.IsByRef)
                type = type.GetElementType();
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                type = type.GetGenericTypeDefinition();
            if (type.IsGenericParameter)
                return null;
            if (type.DeclaringType != null)
            {
                var declaringType = GetClass(context, type.DeclaringType);
                if (declaringType != null)
                {
                    int typeParameterCount;
                    string name = SplitTypeParameterCountFromReflectionName(type.Name, out typeParameterCount);
                    typeParameterCount += declaringType.TypeParameterCount;
                    foreach (var innerClass in declaringType.InnerClasses)
                    {
                        if (innerClass.Name == name && innerClass.TypeParameterCount == typeParameterCount)
                        {
                            return innerClass;
                        }
                    }
                }
                return null;
            }
            else
            {
                int typeParameterCount;
                string name = SplitTypeParameterCountFromReflectionName(type.Name, out typeParameterCount);
                return context.GetClass(type.Namespace, name, typeParameterCount, StringComparer.Ordinal);
            }
        }

        /// <summary>
        /// Removes the ` with type parameter count from the reflection name.
        /// </summary>
        /// <remarks>Do not use this method with the full name of inner classes.</remarks>
        public static string SplitTypeParameterCountFromReflectionName(string reflectionName)
        {
            int typeParameterCount;
            return SplitTypeParameterCountFromReflectionName(reflectionName, out typeParameterCount);
        }

        public static string SplitTypeParameterCountFromReflectionName(string reflectionName, out int typeParameterCount)
        {
            int pos = reflectionName.LastIndexOf('`');
            if (pos < 0)
            {
                typeParameterCount = 0;
                return reflectionName;
            }
            else
            {
                string typeCount = reflectionName.Substring(pos + 1);
                if (int.TryParse(typeCount, out typeParameterCount))
                    return reflectionName.Substring(0, pos);
                else
                    return reflectionName;
            }
        }
		
		public virtual TypeDefinition GetCompoundClass()
		{
			return this;
		}
		
		public virtual IList<TypeDefinition> GetParts()
		{
			return new TypeDefinition[] { this };
		}
		
		public IType GetElementType()
		{
			throw new InvalidOperationException();
		}
		
		public TypeDefinition GetDefinition()
		{
			return this;
		}
		
		public IType Resolve(ITypeResolveContext context)
		{
			if (context == null)
				throw new ArgumentNullException("context");
			return this;
		}
		
		public virtual IEnumerable<IType> GetNestedTypes(ITypeResolveContext context, Predicate<TypeDefinition> filter = null)
		{
			TypeDefinition compound = GetCompoundClass();
			if (compound != this)
				return compound.GetNestedTypes(context, filter);
			
			List<IType> nestedTypes = new List<IType>();
			using (var busyLock = BusyManager.Enter(this)) {
				if (busyLock.Success) {
					foreach (var baseTypeRef in this.BaseTypes) {
						IType baseType = baseTypeRef.Resolve(context);
						TypeDefinition baseTypeDef = baseType.GetDefinition();
						if (baseTypeDef != null && baseTypeDef.ClassType != ClassType.Interface) {
							// get nested types from baseType (not baseTypeDef) so that generics work correctly
							nestedTypes.AddRange(baseType.GetNestedTypes(context, filter));
							break; // there is at most 1 non-interface base
						}
					}
					foreach (TypeDefinition innerClass in this.InnerClasses) {
						if (filter == null || filter(innerClass)) {
							nestedTypes.Add(innerClass);
						}
					}
				}
			}
			return nestedTypes;
		}
		
		/// <summary>
		/// Removes duplicate members from the list.
		/// This is necessary when the same member can be inherited twice due to multiple inheritance.
		/// </summary>
		static void RemoveDuplicates<T>(List<T> list) where T : class
		{
			if (list.Count > 1) {
				HashSet<T> hash = new HashSet<T>();
				list.RemoveAll(m => !hash.Add(m));
			}
		}
		
		// we use reference equality
		bool IEquatable<IType>.Equals(IType other)
		{
			return this == other;
		}
		
		public override string ToString()
		{
			return ReflectionName;
		}
		
		/// <summary>
		/// Gets whether a default constructor should be added to this class if it is required.
		/// Such automatic default constructors will not appear in ITypeDefinition.Methods, but will be present
		/// in IType.GetMethods().
		/// </summary>
		/// <remarks>This way of creating the default constructor is necessary because
		/// we cannot create it directly in the IClass - we need to consider partial classes.</remarks>
		public bool AddDefaultConstructorIfRequired {
			get { return (flags & TypeDefinitionFlags.AddDefaultConstructorIfRequired)!=0; }
			set {
				CheckBeforeMutation();
                flags = value ?
                    flags | TypeDefinitionFlags.AddDefaultConstructorIfRequired :
                    flags & ~TypeDefinitionFlags.AddDefaultConstructorIfRequired;
			}
		}
		
		public IType AcceptVisitor(TypeVisitor visitor)
		{
			return visitor.VisitTypeDefinition(this);
		}
		
		public IType VisitChildren(TypeVisitor visitor)
		{
			return this;
		}
	}
}
