// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMembers5
{
	public class A
	{
		public void M(int t)
		{
		}
	}
	public class A1 : A
	{
		public void M(ref int t)
		{
		}
	}
	public class B
	{
		public void M(ref int l)
		{
		}
	}
	public class B1 : B
	{
		public void M(out int l)
		{
			l = 2;
		}
		public void M(ref long l)
		{
		}
	}
}
