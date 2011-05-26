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
		
		public override IEnumerable<IMethod> GetConstructors(ITypeResolveContext context, Predicate<IMethod> filter)
		{
            return Empty.ReadOnlyCollection<IMethod>();
		}
		
		public override IEnumerable<Event> GetEvents(ITypeResolveContext context, Predicate<Event> filter)
		{
            return Empty.ReadOnlyCollection<Event>();
		}
		
		public override IEnumerable<IField> GetFields(ITypeResolveContext context, Predicate<IField> filter)
		{
            return Empty.ReadOnlyCollection<IField>();
		}
		
		public override IEnumerable<IMethod> GetMethods(ITypeResolveContext context, Predicate<IMethod> filter)
		{
            return Empty.ReadOnlyCollection<IMethod>();
		}
		
		public override IEnumerable<IProperty> GetProperties(ITypeResolveContext context, Predicate<IProperty> filter)
		{
            return Empty.ReadOnlyCollection<IProperty>();
		}
	}
}
