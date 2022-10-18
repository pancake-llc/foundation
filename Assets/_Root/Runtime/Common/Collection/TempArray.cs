namespace Pancake
{
    internal class TempArray<T>
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