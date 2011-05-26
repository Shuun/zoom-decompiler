﻿// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Event"/>.
	/// </summary>
	public class Event : AbstractMember
	{
		IAccessor addAccessor, removeAccessor, invokeAccessor;
		
		protected override void FreezeInternal()
		{
			base.FreezeInternal();
			if (addAccessor != null)    addAccessor.Freeze();
			if (removeAccessor != null) removeAccessor.Freeze();
			if (invokeAccessor != null) invokeAccessor.Freeze();
		}
		
		public Event(ITypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Event)
		{
		}
		
		/// <summary>
		/// Copy constructor
		/// </summary>
		protected Event(Event ev)
			: base(ev)
		{
			this.addAccessor = ev.AddAccessor;
			this.removeAccessor = ev.RemoveAccessor;
			this.invokeAccessor = ev.InvokeAccessor;
		}
		
		public bool CanAdd {
			get { return addAccessor != null; }
		}
		
		public bool CanRemove {
			get { return removeAccessor != null; }
		}
		
		public bool CanInvoke {
			get { return invokeAccessor != null; }
		}
		
		public IAccessor AddAccessor{
			get { return addAccessor; }
			set {
				CheckBeforeMutation();
				addAccessor = value;
			}
		}
		
		public IAccessor RemoveAccessor {
			get { return removeAccessor; }
			set {
				CheckBeforeMutation();
				removeAccessor = value;
			}
		}
		
		public IAccessor InvokeAccessor {
			get { return invokeAccessor; }
			set {
				CheckBeforeMutation();
				invokeAccessor = value;
			}
		}
	}
}
