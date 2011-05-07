// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mi
{
	/// <summary>
	/// Contains extension methods for use within NRefactory.
	/// </summary>
	internal static class ExtensionMethods
	{
		public static void AddRange<T>(this ICollection<T> target, IEnumerable<T> input)
		{
			foreach (T item in input)
				target.Add(item);
		}

        public static int RemoveAll<T>(this ICollection<T> items, Predicate<T> filter)
        {
            int count = 0;
            foreach (var item in items.ToArray())
            {
                if (filter(item))
                {
                    items.Remove(item);
                    count++;
                }
            }

            return count;
        }
	}
}
