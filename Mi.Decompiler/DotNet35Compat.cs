// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;

internal static class DotNet35Compat
{
	public static string StringJoin<T>(string separator, IEnumerable<T> elements)
	{
		#if DOTNET35
		return string.Join(separator, elements.Select(e => e != null ? e.ToString() : null).ToArray());
		#else
		return string.Join(separator, elements);
		#endif
	}
	
	public static IEnumerable<U> SafeCast<T, U>(this IEnumerable<T> elements) where T : class, U where U : class
	{
		#if DOTNET35
		foreach (T item in elements)
			yield return item;
		#else
		return elements;
		#endif
	}
	
	public static Predicate<U> SafeCast<T, U>(this Predicate<T> predicate) where U : class, T where T : class
	{
		#if DOTNET35
		return e => predicate(e);
		#else
		return predicate;
		#endif
	}
	
	#if DOTNET35
	public static IEnumerable<R> Zip<T1, T2, R>(this IEnumerable<T1> input1, IEnumerable<T2> input2, Func<T1, T2, R> f)
	{
		using (var e1 = input1.GetEnumerator())
			using (var e2 = input2.GetEnumerator())
				while (e1.MoveNext() && e2.MoveNext())
					yield return f(e1.Current, e2.Current);
	}
	#endif
}

#if DOTNET35
namespace System
{
    public sealed class Tuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public Tuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }
}
#endif