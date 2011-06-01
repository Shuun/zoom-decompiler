// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMethodGeneric2
{
	public class A
	{
		public virtual void F(int i)
		{
		}
		public void K()
		{
		}
	}
	public class B<T> : A
	{
		protected virtual void F(T t)
		{
		}
		public void K<T2>()
		{
		}
	}
	public class C : B<int>
	{
		protected override void F(int k)
		{
		}
		public new void K<T3>()
		{
		}
	}
	public class D : B<string>
	{
		public override void F(int k)
		{
		}
		public void L<T4>()
		{
		}
	}
	public class E<T>
	{
		public void M<T2>(T t, T2 t2)
		{
		}
	}
	public class F<T> : E<T>
	{
		public void M(T t1, T t2)
		{
		}
	}
}
