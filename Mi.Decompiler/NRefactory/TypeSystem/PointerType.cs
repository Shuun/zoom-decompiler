// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	public class PointerTypeReference : ITypeReference
	{
		readonly ITypeReference elementType;
		
		public PointerTypeReference(ITypeReference elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");
			this.elementType = elementType;
		}
		
		public ITypeReference ElementType {
			get { return elementType; }
		}
		
		public IType Resolve(ITypeResolveContext context)
		{
			throw new NotSupportedException();
		}
		
		public override string ToString()
		{
			return elementType.ToString() + "*";
		}
		
		public static ITypeReference Create(ITypeReference elementType)
		{
			return new PointerTypeReference(elementType);
		}
	}
}
