// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideIndexerGeneric
{
	public class A<T>
	{
		public virtual int this[T r]
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
	public class B : A<int>
	{
		private new int this[int k]
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
	public class C<T> : A<T>
	{
		public override int this[T s]
		{
			set
			{
			}
		}
	}
	public class D<T> : C<T>
	{
		public new virtual int this[T s]
		{
			set
			{
			}
		}
	}
}
