// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;

namespace Mi.NRefactory.Utils
{
	/// <summary>
	/// This class is used to prevent stack overflows by representing a 'busy' flag
	/// that prevents reentrance when another call is running.
	/// However, using a simple 'bool busy' is not thread-safe, so we use a
	/// thread-static BusyManager.
	/// </summary>
	static class BusyManager
	{
        public interface IBusy : IDisposable
        {
            bool Success { get; }
        }

		public static IBusy Enter(object obj)
		{
            throw new NotSupportedException();
		}
	}
}
