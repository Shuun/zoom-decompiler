// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace PropertyOverrideOneAccessor
{
	public class MyClass
	{
		protected internal virtual int MyProperty
		{
			get
			{
				return 3;
			}
			protected set
			{
			}
		}
	}
	public class DerivedNew : MyClass
	{
		public new virtual int MyProperty
		{
			set
			{
			}
		}
	}
	public class DerivedOverride : DerivedNew
	{
		public override int MyProperty
		{
			set
			{
			}
		}
	}
}
