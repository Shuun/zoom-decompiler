// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using Mi.NRefactory.Utils;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Base class for <see cref="IMember"/> implementations.
	/// </summary>
	public abstract class AbstractMember : AbstractFreezable, IMember
	{
		// possible optimizations to reduce the memory usage of AbstractMember:
		// - put 'bool isFrozen' into flags
		// - store regions in more compact form (e.g. assume both file names are identical; use ushort for columns)
		
		ITypeDefinition declaringTypeDefinition;
		ITypeReference returnType = SharedTypes.UnknownType;
		IList<IAttribute> attributes;
		IList<IExplicitInterfaceImplementation> interfaceImplementations;
		DomRegion region;
		DomRegion bodyRegion;
		string name;
		
		// 1 byte per enum + 2 bytes for flags
		Accessibility accessibility;
		EntityType entityType;
		
		protected MemberFlags flags;

        [Flags]
        protected enum MemberFlags
        {
		    Sealed    = 0x0001,
		    Abstract  = 0x0002,
		    Shadowing = 0x0004,
		    Synthetic = 0x0008,
		    Virtual   = 0x0010,
		    Override  = 0x0020,
		    Static    = 0x0040,

            ReadOnly = 0x1000,
		    Volatile = 0x2000,

            ExtensionMethod = 0x1000
        }
		// Flags of form 0xY000 are reserved for use in derived classes (DefaultMethod etc.)
		
		protected override void FreezeInternal()
		{
			attributes = FreezeList(attributes);
			interfaceImplementations = FreezeList(interfaceImplementations);
			base.FreezeInternal();
		}
		
		protected AbstractMember(ITypeDefinition declaringTypeDefinition, string name, EntityType entityType)
		{
			if (declaringTypeDefinition == null)
				throw new ArgumentNullException("declaringTypeDefinition");
			if (name == null)
				throw new ArgumentNullException("name");
			this.declaringTypeDefinition = declaringTypeDefinition;
			this.entityType = entityType;
			this.name = name;
		}
		
		/// <summary>
		/// Copy constructor
		/// </summary>
		protected AbstractMember(IMember member)
		{
			if (member == null)
				throw new ArgumentNullException("member");
			this.declaringTypeDefinition = member.DeclaringTypeDefinition;
			this.returnType = member.ReturnType;
			this.attributes = CopyList(member.Attributes);
			this.interfaceImplementations = CopyList(member.InterfaceImplementations);
			this.region = member.Region;
			this.bodyRegion = member.BodyRegion;
			this.name = member.Name;
			this.accessibility = member.Accessibility;
			this.entityType = member.EntityType;
			this.IsSealed = member.IsSealed;
			this.IsAbstract = member.IsAbstract;
			this.IsShadowing = member.IsShadowing;
			this.IsSynthetic = member.IsSynthetic;
			this.IsVirtual = member.IsVirtual;
			this.IsOverride = member.IsOverride;
			this.IsStatic = member.IsStatic;
		}
		
		public ITypeDefinition DeclaringTypeDefinition {
			get { return declaringTypeDefinition; }
		}
		
		public virtual IType DeclaringType {
			get { return declaringTypeDefinition; }
		}
		
		public virtual IMember MemberDefinition {
			get { return this; }
		}
		
		public ITypeReference ReturnType {
			get { return returnType; }
			set {
				CheckBeforeMutation();
				if (value == null)
					throw new ArgumentNullException();
				returnType = value;
			}
		}
		
		public IList<IExplicitInterfaceImplementation> InterfaceImplementations {
			get {
				if (interfaceImplementations == null)
					interfaceImplementations = new List<IExplicitInterfaceImplementation>();
				return interfaceImplementations;
			}
		}
		
		public bool IsVirtual {
			get { return (flags & MemberFlags.Virtual)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Virtual :
                    flags & ~MemberFlags.Virtual;
			}
		}
		
		public bool IsOverride {
			get { return (flags & MemberFlags.Override)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Override :
                    flags & ~MemberFlags.Override;
			}
		}
		
		public bool IsOverridable {
			get {
				return (IsVirtual || IsOverride) && !IsSealed;
			}
		}
		
		public EntityType EntityType {
			get { return entityType; }
			set {
				CheckBeforeMutation();
				entityType = value;
			}
		}
		
		public DomRegion Region {
			get { return region; }
			set {
				CheckBeforeMutation();
				region = value;
			}
		}
		
		public DomRegion BodyRegion {
			get { return bodyRegion; }
			set {
				CheckBeforeMutation();
				bodyRegion = value;
			}
		}
		
		public IList<IAttribute> Attributes {
			get {
				if (attributes == null)
					attributes = new List<IAttribute>();
				return attributes;
			}
		}
		
		public virtual string Documentation {
			get {
				// To save memory, we don't store the documentation provider within the member,
				// but simply use our declaring type definition as documentation provider.
				// If that fails, we try if the project content is a documentation provider:
				IDocumentationProvider provider = declaringTypeDefinition as IDocumentationProvider
					?? declaringTypeDefinition.ProjectContent as IDocumentationProvider;
				if (provider != null)
					return provider.GetDocumentation(this);
				else
					return null;
			}
		}
		
		public Accessibility Accessibility {
			get { return accessibility; }
			set {
				CheckBeforeMutation();
				accessibility = value;
			}
		}
		
		public bool IsStatic {
			get { return (flags & MemberFlags.Static)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Static :
                    flags & ~MemberFlags.Static;
			}
		}
		
		public bool IsAbstract {
			get { return (flags & MemberFlags.Abstract) != 0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Abstract :
                    flags & ~MemberFlags.Abstract;
			}
		}
		
		public bool IsSealed {
			get { return (flags & MemberFlags.Sealed) != 0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Sealed :
                    flags & ~MemberFlags.Sealed;
			}
		}
		
		public bool IsShadowing {
			get { return (flags & MemberFlags.Shadowing)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Shadowing :
                    flags & ~MemberFlags.Shadowing;
			}
		}
		
		public bool IsSynthetic {
			get { return (flags & MemberFlags.Synthetic)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Synthetic :
                    flags & ~MemberFlags.Synthetic;
			}
		}
		
		public ITypeResolveContext ProjectContent {
			get { return declaringTypeDefinition.ProjectContent; }
		}
		
		public string Name {
			get { return name; }
			set {
				CheckBeforeMutation();
				if (value == null)
					throw new ArgumentNullException();
				name = value;
			}
		}
		
		public virtual string FullName {
			get {
				return this.DeclaringType.FullName + "." + this.Name;
			}
		}
		
		public string Namespace {
			get { return declaringTypeDefinition.Namespace; }
		}
		
		public virtual string ReflectionName {
			get { return this.DeclaringType.ReflectionName + "." + this.Name; }
		}
		
		public override string ToString()
		{
			return "[" + EntityType + " " + ReflectionName + ":" + ReturnType + "]";
		}
		
		public virtual void ApplyInterningProvider(IInterningProvider provider)
		{
			if (provider != null) {
				returnType = provider.Intern(returnType);
				attributes = provider.InternList(attributes);
				interfaceImplementations = provider.InternList(interfaceImplementations);
				name = provider.Intern(name);
			}
		}
	}
}
