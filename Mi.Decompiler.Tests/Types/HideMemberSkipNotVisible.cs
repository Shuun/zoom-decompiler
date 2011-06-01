// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMemberSkipNotVisible
{
	public class A
	{
		protected int F;
		protected string P
		{
			get
			{
				return null;
			}
		}
	}
	public class B : A
	{
		private new string F;
		private new int P
		{
			set
			{
			}
		}
	}
}
