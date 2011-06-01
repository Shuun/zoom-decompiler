// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace OverloadedIndexer
{
	public class MyClass
	{
		public int this[int t]
		{
			get
			{
				return 0;
			}
		}
		public int this[string s]
		{
			get
			{
				return 0;
			}
			set
			{
				Console.WriteLine(value + " " + s);
			}
		}
	}
}
