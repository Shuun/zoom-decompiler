// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace PropertyPrivateGetPublicSet
{
	public class MyClass
	{
		public int MyProperty
		{
			private get
			{
				return 3;
			}
			set
			{
			}
		}
	}
}
