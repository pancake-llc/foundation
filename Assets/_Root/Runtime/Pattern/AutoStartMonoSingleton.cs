using UnityEngine;

namespace Pancake
{
    internal abstract class AutoStartMonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        // ReSharper disable once StaticMemberInGenericType
        private static bool isCreated;

        public static bool IsDestroyed => instance == null && isCreated;

        public static T Instance
        {
            get
            {
                if (!isCreated)
                {
                    var go = new GameObject(typeof(T).Name);
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<T>();

                    isCreated = true;
                }

                return instance;
            }
        }
    }
}