// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace IndexerOverrideRestrictedAccessorOnly
{
	public class MyClass
	{
		public virtual int this[string s]
		{
			get
			{
				return 3;
			}
			protected set
			{
			}
		}
		protected internal virtual int this[int i]
		{
			protected get
			{
				return 2;
			}
			set
			{
			}
		}
	}
	public class Derived : MyClass
	{
		protected internal override int this[int i]
		{
			protected get
			{
				return 4;
			}
		}
	}
}
