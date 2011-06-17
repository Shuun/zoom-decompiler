// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HidePropertyReservedMethod
{
	public class A
	{
		public int P
		{
			get
			{
				return 1;
			}
		}
	}
	public class B : A
	{
		public int get_P()
		{
			return 2;
		}
		public void set_P(int value)
		{
		}
	}
}
