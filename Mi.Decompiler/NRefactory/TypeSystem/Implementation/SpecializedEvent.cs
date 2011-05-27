// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace Mi.NRefactory.TypeSystem.Implementation
{
	/// <summary>
	/// Represents a specialized IEvent (e.g. after type substitution).
	/// </summary>
	public class SpecializedEvent : Event
	{
		public SpecializedEvent(Event e) : base(e)
		{
            throw new NotSupportedException("Event class (as well as SpecializedEvent class) is removed.");
		}
	}
}
