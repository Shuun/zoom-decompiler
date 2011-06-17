// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMethodStatic
{
	public class A
	{
		public int N
		{
			get
			{
				return 0;
			}
		}
	}
	public class B
	{
		public int N()
		{
			return 0;
		}
	}
}
