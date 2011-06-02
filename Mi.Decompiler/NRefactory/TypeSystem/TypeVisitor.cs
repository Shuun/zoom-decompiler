// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Base class for the visitor pattern on <see cref="IType"/>.
	/// </summary>
	public abstract class TypeVisitor
	{
		public virtual IType VisitTypeDefinition(TypeDefinition type)
		{
			return type.VisitChildren(this);
		}
		
		public virtual IType VisitTypeParameter(TypeParameter type)
		{
			return type.VisitChildren(this);
		}
		
		public virtual IType VisitOtherType(IType type)
		{
			return type.VisitChildren(this);
		}
	}
}
