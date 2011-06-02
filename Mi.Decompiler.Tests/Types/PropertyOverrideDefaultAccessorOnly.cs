// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace PropertyOverrideDefaultAccessorOnly
{
	public class BaseClass
	{
		public virtual int MyProperty
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

    public class Derived : BaseClass
	{
		public override int MyProperty
		{
			get
			{
				return 4;
			}
		}
	}
}