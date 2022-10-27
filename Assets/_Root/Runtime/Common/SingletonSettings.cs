using UnityEngine;

namespace Pancake
{
    public abstract class SingletonSettings<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = Resources.Load<T>(nameof(T));

                if (instance == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"{nameof(T)} not found! Please check again!");
#endif
                    instance = CreateInstance<T>();
                }
                
                return instance;
            }
        }
        
        public static T Load() => Resources.Load<T>(nameof(T));
    }
}