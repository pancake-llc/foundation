using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Pancake
{
    public static class TempArray
    {
        /// <summary>
        /// Returns a temp array of length len if small enough for a cached array. 
        /// Otherwise returns a new array of length len.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="len"></param>
        /// <returns></returns>
        public static T[] TryGetTemp<T>(int len) { return TempArrayImpl<T>.TryGetTemp(len); }

        public static T[] Empty<T>() { return TempArrayImpl<T>.Empty; }

        public static T[] Temp<T>(T value) { return TempArrayImpl<T>.Temp(value); }

        public static T[] Temp<T>(T value1, T value2) { return TempArrayImpl<T>.Temp(value1, value2); }

        public static T[] Temp<T>(T value1, T value2, T value3) { return TempArrayImpl<T>.Temp(value1, value2, value3); }

        public static T[] Temp<T>(T value1, T value2, T value3, T value4) { return TempArrayImpl<T>.Temp(value1, value2, value3, value4); }

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
                    return TempArray.Empty<T>();
                case 1:
                    return TempArrayImpl<T>.Temp(coll[0]);
                case 2:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1]);
                case 3:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2]);
                case 4:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3]);
                default:
                    return coll.ToArray();
            }
        }

        private static T[] Temp<T>(IReadOnlyList<T> coll)
        {
            switch (coll.Count)
            {
                case 0:
                    return TempArray.Empty<T>();
                case 1:
                    return TempArrayImpl<T>.Temp(coll[0]);
                case 2:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1]);
                case 3:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2]);
                case 4:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3]);
                default:
                    return coll.ToArray();
            }
        }

        public static void Release<T>(T[] arr) { TempArrayImpl<T>.Release(arr); }


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
    }

    [UsedImplicitly]
    internal class TempArrayImpl<T>
    {
        private static object _lock = new object();
        private static volatile T[] _empty;
        private static volatile T[] _oneArray;
        private static volatile T[] _twoArray;
        private static volatile T[] _threeArray;
        private static volatile T[] _fourArray;

        public static T[] Empty
        {
            get
            {
                if (_empty == null) _empty = new T[0];
                return _empty;
            }
        }

        public static T[] TryGetTemp(int len)
        {
            T[] result;
            lock (_lock)
            {
                switch (len)
                {
                    case 0:
                        result = Empty;
                        break;
                    case 1:
                        if (_oneArray != null)
                        {
                            result = _oneArray;
                            _oneArray = null;
                        }
                        else
                        {
                            result = new T[1];
                        }

                        break;
                    case 2:
                        if (_twoArray != null)
                        {
                            result = _twoArray;
                            _twoArray = null;
                        }
                        else
                        {
                            result = new T[2];
                        }

                        break;
                    case 3:
                        if (_threeArray != null)
                        {
                            result = _threeArray;
                            _threeArray = null;
                        }
                        else
                        {
                            result = new T[3];
                        }

                        break;
                    case 4:
                        if (_fourArray != null)
                        {
                            result = _fourArray;
                            _fourArray = null;
                        }
                        else
                        {
                            result = new T[4];
                        }

                        break;
                    default:
                        result = new T[len];
                        break;
                }
            }

            return result;
        }

        public static T[] Temp(T value)
        {
            T[] arr;

            lock (_lock)
            {
                if (_oneArray != null)
                {
                    arr = _oneArray;
                    _oneArray = null;
                }
                else
                {
                    arr = new T[1];
                }
            }

            arr[0] = value;
            return arr;
        }

        public static T[] Temp(T value1, T value2)
        {
            T[] arr;

            lock (_lock)
            {
                if (_twoArray != null)
                {
                    arr = _twoArray;
                    _twoArray = null;
                }
                else
                {
                    arr = new T[2];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            return arr;
        }

        public static T[] Temp(T value1, T value2, T value3)
        {
            T[] arr;

            lock (_lock)
            {
                if (_threeArray != null)
                {
                    arr = _threeArray;
                    _threeArray = null;
                }
                else
                {
                    arr = new T[3];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            arr[2] = value3;
            return arr;
        }

        public static T[] Temp(T value1, T value2, T value3, T value4)
        {
            T[] arr;

            lock (_lock)
            {
                if (_fourArray != null)
                {
                    arr = _fourArray;
                    _fourArray = null;
                }
                else
                {
                    arr = new T[4];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            arr[2] = value3;
            arr[3] = value4;
            return arr;
        }


        public static void Release(T[] arr)
        {
            if (arr == null) return;
            System.Array.Clear(arr, 0, arr.Length);

            lock (_lock)
            {
                switch (arr.Length)
                {
                    case 1:
                        _oneArray = arr;
                        break;
                    case 2:
                        _twoArray = arr;
                        break;
                    case 3:
                        _threeArray = arr;
                        break;
                    case 4:
                        _fourArray = arr;
                        break;
                }
            }
        }
    }
}