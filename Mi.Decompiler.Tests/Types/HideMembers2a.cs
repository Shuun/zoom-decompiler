// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMembers2a
{
	public interface IA
	{
		int this[int i]
		{
			get;
		}
	}
	public class A : IA
	{
		int IA.this[int i]
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
	public class A1 : A
	{
		public int this[int i]
		{
			get
			{
				return 3;
			}
		}
	}
}
