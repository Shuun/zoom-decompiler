// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace PropertyInterfaceExplicitImplementation
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
		int IMyInterface.MyProperty
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
