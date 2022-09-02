using System;

namespace Pancake
{
    public class Singleton<T> where T : class
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null) instance = Activator.CreateInstance<T>();
                return instance;
            }
        }
    }
}