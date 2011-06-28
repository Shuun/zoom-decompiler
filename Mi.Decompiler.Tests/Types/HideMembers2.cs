// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;

namespace HideMembers2
{
	public class G
	{
		public int Item
		{
			get
			{
				return 1;
			}
		}
	}
	public class G2 : G
	{
		public int this[int i]
		{
			get
			{
				return 2;
			}
		}
	}
	public class G3 : G2
	{
		public new int Item
		{
			get
			{
				return 4;
			}
		}
	}
	public class H
	{
		public int this[int j]
		{
			get
			{
				return 0;
			}
		}
	}
	public class H2 : H
	{
		public int Item
		{
			get
			{
				return 2;
			}
		}
	}
	public class H3 : H2
	{
		public new string this[int j]
		{
			get
			{
				return null;
			}
		}
	}
}
