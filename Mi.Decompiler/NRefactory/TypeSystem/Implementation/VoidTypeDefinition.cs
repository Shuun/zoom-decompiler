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
		
		public override IEnumerable<Method> GetConstructors(ITypeResolveContext context, Predicate<Method> filter)
		{
            return Empty.ReadOnlyCollection<Method>();
		}
		
		public override IEnumerable<Event> GetEvents(ITypeResolveContext context, Predicate<Event> filter)
		{
            return Empty.ReadOnlyCollection<Event>();
		}
		
		public override IEnumerable<Field> GetFields(ITypeResolveContext context, Predicate<Field> filter)
		{
            return Empty.ReadOnlyCollection<Field>();
		}
		
		public override IEnumerable<Method> GetMethods(ITypeResolveContext context, Predicate<Method> filter)
		{
            return Empty.ReadOnlyCollection<Method>();
		}
		
		public override IEnumerable<Property> GetProperties(ITypeResolveContext context, Predicate<Property> filter)
		{
            return Empty.ReadOnlyCollection<Property>();
		}
	}
}
