using System;
using System.Collections.Generic;

namespace AuthMovementExample
{
    public static class ListExtensions
    {
        public static void TryAdd<T>(this List<T> list, T item)
        {
            if (list.Contains(item)) return;

            lock (list)
            {
                list.Add(item);
            }
        }

        public static T Pop<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                T item = list[0];
                list.RemoveAt(0);
                return item;
            }
            throw new IndexOutOfRangeException();
        }

        public static T Front<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                return list[0];
            }
            throw new IndexOutOfRangeException();
        }

        public static T Back<T>(this List<T> list)
        {
            if (list.Count > 0)
            {
                return list[list.Count - 1];
            }
            throw new IndexOutOfRangeException();
        }

        public static bool Replace<T>(this List<T> list, T item, Predicate<T> match)
        {
            int i = list.FindIndex(match);
            if (i >= 0)
            {
                list[i] = item;
            }
            return i >= 0;
        }
    }
}