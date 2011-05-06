using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace ICSharpCode.Decompiler
{
    public static class CollectionExtensions
    {
        public static ReadOnlyCollection<T> ToReadOnlyCollectionOrNull<T>(this IEnumerable<T> items)
        {
            if (items == null)
                return null;

            var listItems = items as IList<T>;
            if (listItems != null)
            {
                if (listItems.Count == 0)
                    return null;

                return new ReadOnlyCollection<T>(listItems);
            }
            else
            {
                var collectionItems = items as ICollection<T>;
                if (collectionItems != null)
                {
                    if (collectionItems.Count == 0)
                        return null;

                    T[] array = new T[collectionItems.Count];
                    collectionItems.CopyTo(array, 0);

                    return new ReadOnlyCollection<T>(array);
                }
                else
                {
                    List<T> cache = null;
                    foreach (var item in items)
                    {
                        if (cache == null)
                            cache = new List<T>();
                        cache.Add(item);
                    }

                    if (cache.Count == 0)
                        return null;

                    return new ReadOnlyCollection<T>(cache);
                }
            }
        }
    }
}
