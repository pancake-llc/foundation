using System;
using UnityEngine;

namespace Pancake.Core
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T monoInstance;

        protected void Init(T instance)
        {
            if (instance == null)
            {
                throw new Exception($"Instance of type {nameof(T)} cannot be null");
            }

            monoInstance = instance;
        }

        public static T Instance
        {
            get
            {
                if (monoInstance == null)
                {
                    throw new Exception($"{nameof(MonoSingleton<T>)} used before initialization. " + $"Please use {nameof(Init)} before using the singleton instance");
                }

                return monoInstance;
            }
        }
    }
}