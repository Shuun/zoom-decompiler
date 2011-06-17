// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMethod
{
	public class A
	{
		public virtual void F()
		{
		}
	}
	public class B : A
	{
		private new void F()
		{
			base.F();
		}
	}
	public class C : B
	{
		public override void F()
		{
			base.F();
		}
	}
}
