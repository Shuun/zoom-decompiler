using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace ICSharpCode.Decompiler
{
    public static class Empty
    {
        private static class Singletons<T>
        {
            public static readonly T[] Array = new T[] { };
            public static readonly ReadOnlyCollection<T> ReadOnlyCollection = new ReadOnlyCollection<T>(Array);
        }

        public static T[] Array<T>() { return Singletons<T>.Array; }
        public static ReadOnlyCollection<T> ReadOnlyCollection<T>() { return Singletons<T>.ReadOnlyCollection; }
    }
}