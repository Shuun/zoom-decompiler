// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideNestedClass
{
	public class A
	{
		public class N1
		{
		}
		protected class N2
		{
		}
		private class N3
		{
		}
		internal class N4
		{
		}
		protected internal class N5
		{
		}
	}
	public class B : A
	{
		public new int N1;
		public new int N2;
		public int N3;
		public new int N4;
		public new int N5;
	}
}
