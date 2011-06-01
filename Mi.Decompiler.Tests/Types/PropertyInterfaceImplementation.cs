// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace PropertyInterfaceImplementation
{
	public interface IMyInterface
	{
		int MyProperty
		{
			get;
			set;
		}
	}
	public class MyClass : IMyInterface
	{
		public int MyProperty
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
	}
}
