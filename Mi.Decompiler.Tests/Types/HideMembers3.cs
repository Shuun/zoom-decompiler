// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMembers3
{
	public class G<T>
	{
		public void M1(T p)
		{
		}
		public int M2(int t)
		{
			return 3;
		}
	}
	public class G1<T> : G<int>
	{
		public new int M1(int i)
		{
			return 0;
		}
		public int M2(T i)
		{
			return 2;
		}
	}
	public class G2<T> : G<int>
	{
		public int M1(T p)
		{
			return 4;
		}
	}
	public class J
	{
		public int P
		{
			get
			{
				return 2;
			}
		}
	}
	public class J2 : J
	{
		public int get_P;
	}
}
