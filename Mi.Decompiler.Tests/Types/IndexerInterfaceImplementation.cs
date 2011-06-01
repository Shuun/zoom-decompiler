// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace IndexerInterfaceImplementation
{
	public interface IMyInterface
	{
		int this[string s]
		{
			get;
		}
	}
	public class MyClass : IMyInterface
	{
		public int this[string s]
		{
			get
			{
				return 3;
			}
		}
	}
}
