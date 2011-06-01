// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMethodGeneric
{
	public class A<T>
	{
		public virtual void F(T s)
		{
		}
		public new static bool Equals(object o1, object o2)
		{
			return true;
		}
	}
	public class B : A<string>
	{
		private new void F(string k)
		{
		}
		public void F(int i)
		{
		}
	}
	public class C<T> : A<T>
	{
		public override void F(T r)
		{
		}
		public void G(T t)
		{
		}
	}
	public class D<T1> : C<T1>
	{
		public new virtual void F(T1 k)
		{
		}
		public virtual void F<T2>(T2 k)
		{
		}
		public virtual void G<T2>(T2 t)
		{
		}
	}
}
