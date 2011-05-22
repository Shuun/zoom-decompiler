// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using Mi.NRefactory.TypeSystem;

namespace Mi.CSharpAst.Resolver
{
	/// <summary>
	/// Represents the resolve result of an 'ref x' or 'out x' expression.
	/// </summary>
	public class ByReferenceResolveResult : ResolveResult
	{
		public bool IsOut { get; private set; }
		public bool IsRef { get { return !IsOut;} }
		
		public ByReferenceResolveResult(IType elementType, bool isOut)
			: base(new ByReferenceType(elementType))
		{
			this.IsOut = isOut;
		}
		
		public IType ElementType {
			get { return ((ByReferenceType)this.Type).ElementType; }
		}
		
		public override string ToString()
		{
			return string.Format("[{0} {1} {2}]", GetType().Name, IsOut ? "out" : "ref", ElementType);
		}
	}
}
