// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideProperty
{
	public class A
	{
		public virtual int P
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
	}
	public class B : A
	{
		private new int P
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
	}
	public class C : B
	{
		public override int P
		{
			set
			{
			}
		}
	}
}
