// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMembers
{
	public class A
	{
		public int F;
		public int Prop
		{
			get
			{
				return 3;
			}
		}
		public int G
		{
			get
			{
				return 3;
			}
		}
	}
	public class B : A
	{
		public new int F
		{
			get
			{
				return 3;
			}
		}
		public new string Prop
		{
			get
			{
				return "a";
			}
		}
	}
	public class C : A
	{
		public new int G;
	}
	public class D : A
	{
		public new void F()
		{
		}
	}
	public class D1 : D
	{
		public new int F;
	}
	public class E : A
	{
		private new class F
		{
		}
	}
}
