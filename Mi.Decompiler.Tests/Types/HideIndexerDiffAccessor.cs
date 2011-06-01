// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideIndexerDiffAccessor
{
	public class A
	{
		public int this[int i]
		{
			get
			{
				return 2;
			}
		}
	}
	public class B : A
	{
		public new int this[int j]
		{
			set
			{
			}
		}
	}
}
