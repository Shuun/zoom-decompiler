using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Mi
{
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] items)
        {
            return items == null || items.Length == 0;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || !items.Any();
        }

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