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
        protected AbstractMember()
        {
            throw new NotSupportedException("AbstractMember is disabled.");
        }

        public IType DeclaringType
        {
            get { throw new NotImplementedException(); }
        }

        public IMember MemberDefinition
        {
            get { throw new NotImplementedException(); }
        }

        public ITypeReference ReturnType
        {
            get { throw new NotImplementedException(); }
        }

        public IList<ExplicitInterfaceImplementation> InterfaceImplementations
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsVirtual
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOverride
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsOverridable
        {
            get { throw new NotImplementedException(); }
        }

        public EntityType EntityType
        {
            get { throw new NotImplementedException(); }
        }

        public DomRegion Region
        {
            get { throw new NotImplementedException(); }
        }

        public DomRegion BodyRegion
        {
            get { throw new NotImplementedException(); }
        }

        public TypeDefinition DeclaringTypeDefinition
        {
            get { throw new NotImplementedException(); }
        }

        public IList<IAttribute> Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public string Documentation
        {
            get { throw new NotImplementedException(); }
        }

        public Accessibility Accessibility
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsStatic
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsAbstract
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSealed
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsShadowing
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSynthetic
        {
            get { throw new NotImplementedException(); }
        }

        public ITypeResolveContext ProjectContent
        {
            get { throw new NotImplementedException(); }
        }

        public string FullName
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string Namespace
        {
            get { throw new NotImplementedException(); }
        }

        public string ReflectionName
        {
            get { throw new NotImplementedException(); }
        }
    }
}
