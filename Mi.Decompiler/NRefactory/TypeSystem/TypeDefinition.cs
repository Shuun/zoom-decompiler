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
    public abstract class TypeDefinition : AbstractFreezable, IType, IEntity
    {
        bool? IType.IsReferenceType
        {
            get { throw new NotImplementedException(); }
        }

        TypeDefinition IType.GetDefinition()
        {
            throw new NotImplementedException();
        }

        IType IType.DeclaringType
        {
            get { throw new NotImplementedException(); }
        }

        IEnumerable<IType> IType.GetNestedTypes(object context, Predicate<TypeDefinition> filter)
        {
            throw new NotImplementedException();
        }

        IType ITypeReference.Resolve()
        {
            throw new NotImplementedException();
        }

        string INamedElement.FullName
        {
            get { throw new NotImplementedException(); }
        }

        string INamedElement.Name
        {
            get { throw new NotImplementedException(); }
        }

        string INamedElement.Namespace
        {
            get { throw new NotImplementedException(); }
        }

        bool IEquatable<IType>.Equals(IType other)
        {
            throw new NotImplementedException();
        }

        EntityType IEntity.EntityType
        {
            get { throw new NotImplementedException(); }
        }

        TypeDefinition IEntity.DeclaringTypeDefinition
        {
            get { throw new NotImplementedException(); }
        }

        string IEntity.Documentation
        {
            get { throw new NotImplementedException(); }
        }

        Accessibility IEntity.Accessibility
        {
            get { throw new NotImplementedException(); }
        }

        bool IEntity.IsStatic
        {
            get { throw new NotImplementedException(); }
        }

        bool IEntity.IsAbstract
        {
            get { throw new NotImplementedException(); }
        }

        bool IEntity.IsSealed
        {
            get { throw new NotImplementedException(); }
        }

        bool IEntity.IsShadowing
        {
            get { throw new NotImplementedException(); }
        }

        bool IEntity.IsSynthetic
        {
            get { throw new NotImplementedException(); }
        }

        object IEntity.ProjectContent
        {
            get { throw new NotImplementedException(); }
        }

        bool IFreezable.IsFrozen
        {
            get { throw new NotImplementedException(); }
        }

        void IFreezable.Freeze()
        {
            throw new NotImplementedException();
        }
    }
}