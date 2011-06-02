// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMethodGenericSkipPrivate
{
	public class A<T>
	{
		public virtual void F(T t)
		{
		}
	}
	public class B<T> : A<T>
	{
		private new void F(T t)
		{
		}
		private void K()
		{
		}
	}
	public class C<T> : B<T>
	{
		public override void F(T tt)
		{
		}
		public void K()
		{
		}
	}
	public class D : B<int>
	{
		public override void F(int t)
		{
		}
	}
}
