// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace IndexerInInterface
{
	public interface IInterface
	{
		int this[string s, string s2]
		{
			set;
		}
	}
}
