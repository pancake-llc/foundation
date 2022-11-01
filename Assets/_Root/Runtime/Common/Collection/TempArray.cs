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
        
        public static T[] Temp<T>(T value1, T value2, T value3, T value4, T value5) { return TempArrayImpl<T>.Temp(value1, value2, value3, value4, value5); }
        
        public static T[] Temp<T>(T value1, T value2, T value3, T value4, T value5, T value6) { return TempArrayImpl<T>.Temp(value1, value2, value3, value4, value5, value6); }
        
        public static T[] Temp<T>(T value1, T value2, T value3, T value4, T value5, T value6, T value7) { return TempArrayImpl<T>.Temp(value1, value2, value3, value4, value5, value6, value7); }
        
        public static T[] Temp<T>(T value1, T value2, T value3, T value4, T value5, T value6, T value7, T value8) { return TempArrayImpl<T>.Temp(value1, value2, value3, value4, value5, value6, value7, value8); }

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

            if (coll is IReadOnlyList<T> rl)
            {
                return Temp<T>(rl);
            }

            using (var lst = TempCollection.GetList<T>(coll))
            {
                return Temp<T>((IList<T>) lst);
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
                case 5:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4]);
                case 6:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4], coll[5]);
                case 7:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4], coll[5], coll[6]);
                case 8:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4], coll[5], coll[6], coll[7]);
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
                case 5:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4]);
                case 6:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4], coll[5]);
                case 7:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4], coll[5], coll[6]);
                case 8:
                    return TempArrayImpl<T>.Temp(coll[0], coll[1], coll[2], coll[3], coll[4], coll[5], coll[6], coll[7]);
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
        private static object @lock = new object();
        private static volatile T[] empty;
        private static volatile T[] oneArray;
        private static volatile T[] twoArray;
        private static volatile T[] threeArray;
        private static volatile T[] fourArray;
        private static volatile T[] fifthArray;
        private static volatile T[] sixthArray;
        private static volatile T[] seventhArray;
        private static volatile T[] eighthArray;

        public static T[] Empty
        {
            get
            {
                if (empty == null) empty = new T[0];
                return empty;
            }
        }

        public static T[] TryGetTemp(int len)
        {
            T[] result;
            lock (@lock)
            {
                switch (len)
                {
                    case 0:
                        result = Empty;
                        break;
                    case 1:
                        if (oneArray != null)
                        {
                            result = oneArray;
                            oneArray = null;
                        }
                        else
                        {
                            result = new T[1];
                        }

                        break;
                    case 2:
                        if (twoArray != null)
                        {
                            result = twoArray;
                            twoArray = null;
                        }
                        else
                        {
                            result = new T[2];
                        }

                        break;
                    case 3:
                        if (threeArray != null)
                        {
                            result = threeArray;
                            threeArray = null;
                        }
                        else
                        {
                            result = new T[3];
                        }

                        break;
                    case 4:
                        if (fourArray != null)
                        {
                            result = fourArray;
                            fourArray = null;
                        }
                        else
                        {
                            result = new T[4];
                        }

                        break;
                    
                    case 5:
                        if (fifthArray != null)
                        {
                            result = fifthArray;
                            fifthArray = null;
                        }
                        else
                        {
                            result = new T[5];
                        }

                        break;
                    
                    case 6:
                        if (sixthArray != null)
                        {
                            result = sixthArray;
                            sixthArray = null;
                        }
                        else
                        {
                            result = new T[6];
                        }

                        break;
                    
                    case 7:
                        if (seventhArray != null)
                        {
                            result = seventhArray;
                            seventhArray = null;
                        }
                        else
                        {
                            result = new T[7];
                        }

                        break;
                    
                    case 8:
                        if (eighthArray != null)
                        {
                            result = eighthArray;
                            eighthArray = null;
                        }
                        else
                        {
                            result = new T[8];
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

            lock (@lock)
            {
                if (oneArray != null)
                {
                    arr = oneArray;
                    oneArray = null;
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

            lock (@lock)
            {
                if (twoArray != null)
                {
                    arr = twoArray;
                    twoArray = null;
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

            lock (@lock)
            {
                if (threeArray != null)
                {
                    arr = threeArray;
                    threeArray = null;
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

            lock (@lock)
            {
                if (fourArray != null)
                {
                    arr = fourArray;
                    fourArray = null;
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

        public static T[] Temp(T value1, T value2, T value3, T value4, T value5)
        {
            T[] arr;

            lock (@lock)
            {
                if (fifthArray != null)
                {
                    arr = fifthArray;
                    fifthArray = null;
                }
                else
                {
                    arr = new T[5];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            arr[2] = value3;
            arr[3] = value4;
            arr[4] = value5;
            return arr;
        }

        public static T[] Temp(T value1, T value2, T value3, T value4, T value5, T value6)
        {
            T[] arr;

            lock (@lock)
            {
                if (sixthArray != null)
                {
                    arr = sixthArray;
                    sixthArray = null;
                }
                else
                {
                    arr = new T[6];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            arr[2] = value3;
            arr[3] = value4;
            arr[4] = value5;
            arr[5] = value6;
            return arr;
        }

        public static T[] Temp(T value1, T value2, T value3, T value4, T value5, T value6, T value7)
        {
            T[] arr;

            lock (@lock)
            {
                if (seventhArray != null)
                {
                    arr = seventhArray;
                    seventhArray = null;
                }
                else
                {
                    arr = new T[7];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            arr[2] = value3;
            arr[3] = value4;
            arr[4] = value5;
            arr[5] = value6;
            arr[6] = value7;
            return arr;
        }

        public static T[] Temp(T value1, T value2, T value3, T value4, T value5, T value6, T value7, T value8)
        {
            T[] arr;

            lock (@lock)
            {
                if (eighthArray != null)
                {
                    arr = eighthArray;
                    eighthArray = null;
                }
                else
                {
                    arr = new T[8];
                }
            }

            arr[0] = value1;
            arr[1] = value2;
            arr[2] = value3;
            arr[3] = value4;
            arr[4] = value5;
            arr[5] = value6;
            arr[6] = value7;
            arr[7] = value8;
            return arr;
        }


        public static void Release(T[] arr)
        {
            if (arr == null) return;
            System.Array.Clear(arr, 0, arr.Length);

            lock (@lock)
            {
                switch (arr.Length)
                {
                    case 1:
                        oneArray = arr;
                        break;
                    case 2:
                        twoArray = arr;
                        break;
                    case 3:
                        threeArray = arr;
                        break;
                    case 4:
                        fourArray = arr;
                        break;
                    case 5:
                        fifthArray = arr;
                        break;
                    case 6:
                        sixthArray = arr;
                        break;
                    case 7:
                        seventhArray = arr;
                        break;
                    case 8:
                        eighthArray = arr;
                        break;
                }
            }
        }
    }
}