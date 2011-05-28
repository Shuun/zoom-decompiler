// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Attribute"/>.
	/// </summary>
	public sealed class Attribute : AbstractFreezable, IAttribute, ISupportsInterning
	{
		public Attribute(ITypeReference attributeType, IEnumerable<ITypeReference> constructorParameterTypes)
		{
            throw new NotSupportedException();
		}
		
		public ITypeReference AttributeType {
            get { throw new NotSupportedException(); }
		}
		
		public ReadOnlyCollection<ITypeReference> ConstructorParameterTypes {
            get { throw new NotSupportedException(); }
		}
		
		public DomRegion Region {
            get { throw new NotSupportedException(); }
			set {
                throw new NotSupportedException();
			}
		}
		
		public IList<IConstantValue> PositionalArguments {
			get {
                throw new NotSupportedException();
			}
		}
		
		public IList<KeyValuePair<string, IConstantValue>> NamedArguments {
			get {
                throw new NotSupportedException();
			}
		}

        void ISupportsInterning.PrepareForInterning(IInterningProvider provider)
        {
            throw new NotImplementedException();
        }

        int ISupportsInterning.GetHashCodeForInterning()
        {
            throw new NotImplementedException();
        }

        bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
        {
            throw new NotImplementedException();
        }

        DomRegion IAttribute.Region
        {
            get { throw new NotImplementedException(); }
        }

        ITypeReference IAttribute.AttributeType
        {
            get { throw new NotImplementedException(); }
        }

        IList<IConstantValue> IAttribute.GetPositionalArguments(ITypeResolveContext context)
        {
            throw new NotImplementedException();
        }

        IList<KeyValuePair<string, IConstantValue>> IAttribute.GetNamedArguments(ITypeResolveContext context)
        {
            throw new NotImplementedException();
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