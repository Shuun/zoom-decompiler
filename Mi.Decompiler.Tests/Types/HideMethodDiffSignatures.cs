// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMethodDiffSignatures
{
	public class C1<T>
	{
		public virtual void M(T arg)
		{
		}
	}
	
    public class C2<T1, T2> : C1<T2>
	{
		public new virtual void M(T2 arg)
		{
		}
	}

    public class C3 : C2<int, bool>
	{
		public new virtual void M(bool arg)
		{
		}
	}
}
