
using System;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	public class ByReferenceTypeReference : ITypeReference
	{
		readonly ITypeReference elementType;
		
		public ByReferenceTypeReference(ITypeReference elementType)
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
			return elementType.ToString() + "&";
		}
		
		public static ITypeReference Create(ITypeReference elementType)
		{
			return new ByReferenceTypeReference(elementType);
		}
	}
}
