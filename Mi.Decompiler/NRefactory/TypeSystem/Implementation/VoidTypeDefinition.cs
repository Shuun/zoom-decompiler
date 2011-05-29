// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Special type definition for 'void'.
	/// </summary>
	public class VoidTypeDefinition : TypeDefinition
	{
		public VoidTypeDefinition(ITypeResolveContext projectContent)
			: base(projectContent, "System", "Void")
		{
			this.ClassType = ClassType.Struct;
			this.Accessibility = Accessibility.Public;
			this.IsSealed = true;
		}
	}
}
