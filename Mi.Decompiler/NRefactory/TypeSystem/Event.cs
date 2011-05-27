// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Event"/>.
	/// </summary>
	public abstract class Event : AbstractMember
	{
		public Event(TypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Event)
		{
            throw new NotSupportedException("Event class is removed.");
		}
		
		/// <summary>
		/// Copy constructor
		/// </summary>
		protected Event(Event ev)
			: base(ev)
		{
            throw new NotSupportedException("Event class is removed.");
		}
	}
}
