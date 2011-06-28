// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace IndexerAbstract
{
	public abstract class MyClass
	{
		public abstract int this[string s, string s2]
		{
			set;
		}
		protected abstract string this[int index]
		{
			get;
		}
	}
}
