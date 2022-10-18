using System.Collections.Generic;
using System.Linq;

namespace Pancake
{
    public static class ArrayUtil
    {
        #region Array Methods

        /// <summary>
        /// Returns a temp array of length len if small enough for a cached array. 
        /// Otherwise returns a new array of length len.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="len"></param>
        /// <returns></returns>
        public static T[] TryGetTemp<T>(int len) { return TempArray<T>.TryGetTemp(len); }

        public static T[] Empty<T>() { return TempArray<T>.Empty; }

        public static T[] Temp<T>(T value) { return TempArray<T>.Temp(value); }

        public static T[] Temp<T>(T value1, T value2) { return TempArray<T>.Temp(value1, value2); }

        public static T[] Temp<T>(T value1, T value2, T value3) { return TempArray<T>.Temp(value1, value2, value3); }

        public static T[] Temp<T>(T value1, T value2, T value3, T value4) { return TempArray<T>.Temp(value1, value2, value3, value4); }

        /// <summary>
        /// Attempts to create a small temp array if coll is small enough, otherwise generates a new array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static T[] Temp<T>(IEnumerable<T> coll)
        {
            if (coll is IList<T> l)
            {
                return Temp<T>(l);
            }
            else if (coll is IReadOnlyList<T> rl)
            {
                return Temp<T>(rl);
            }
            else
            {
                using (var lst = TempCollection.GetList<T>(coll))
                {
                    return Temp<T>((IList<T>) lst);
                }
            }
        }

        public static T[] Temp<T>(IList<T> coll)
        {
            switch (coll.Count)
            {
                case 0:
                    return ArrayUtil.Empty<T>();
                case 1:
                    return TempArray<T>.Temp(coll[0]);
                case 2:
                    return TempArray<T>.Temp(coll[0], coll[1]);
                case 3:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2]);
                case 4:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2], coll[3]);
                default:
                    return coll.ToArray();
            }
        }

        private static T[] Temp<T>(IReadOnlyList<T> coll)
        {
            switch (coll.Count)
            {
                case 0:
                    return ArrayUtil.Empty<T>();
                case 1:
                    return TempArray<T>.Temp(coll[0]);
                case 2:
                    return TempArray<T>.Temp(coll[0], coll[1]);
                case 3:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2]);
                case 4:
                    return TempArray<T>.Temp(coll[0], coll[1], coll[2], coll[3]);
                default:
                    return coll.ToArray();
            }
        }

        public static void ReleaseTemp<T>(T[] arr) { TempArray<T>.Release(arr); }


        public static int IndexOf<T>(this T[] lst, T obj) { return System.Array.IndexOf(lst, obj); }

        public static int IndexOf<T>(this IList<T> lst, System.Func<T, bool> predicate)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (predicate(lst[i])) return i;
            }

            return -1;
        }

        public static bool InBounds(this System.Array arr, int index) { return index >= 0 && index <= arr.Length - 1; }

        public static void Clear(this System.Array arr)
        {
            if (arr == null) return;
            System.Array.Clear(arr, 0, arr.Length);
        }

        public static void Copy<T>(IEnumerable<T> source, System.Array destination, int index)
        {
            if (source is System.Collections.ICollection)
                (source as System.Collections.ICollection).CopyTo(destination, index);
            else
            {
                int i = 0;
                foreach (var el in source)
                {
                    destination.SetValue(el, i + index);
                    i++;
                }
            }
        }

        #endregion
    }
}