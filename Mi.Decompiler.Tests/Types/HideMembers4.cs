// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMembers4
{
	public class A
	{
		public void M<T>(T t)
		{
		}
	}
	public class A1 : A
	{
		public new void M<K>(K t)
		{
		}
		public void M(int t)
		{
		}
	}
	public class B
	{
		public void M<T>()
		{
		}
		public void M1<T>()
		{
		}
		public void M2<T>(T t)
		{
		}
	}
	public class B1 : B
	{
		public void M<T1, T2>()
		{
		}
		public new void M1<R>()
		{
		}
		public new void M2<R>(R r)
		{
		}
	}
	public class C<T>
	{
		public void M<TT>(T t)
		{
		}
	}
	public class C1<K> : C<K>
	{
		public void M<TT>(TT t)
		{
		}
	}
}
