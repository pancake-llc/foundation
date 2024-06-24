using System;
using UnityEngine;

namespace Pancake.Sound
{
    public abstract class InstanceWrapper<T> where T : UnityEngine.Object, IRecyclable<T>
    {
        private T _instance;

        protected T Instance
        {
            get
            {
                if (_instance) return _instance;
                LogInstanceIsNull();
                return null;
            }
        }

        public event Action<T> OnRecycle
        {
            add
            {
                if (_instance) _instance.OnRecycle += value;
            }
            remove
            {
                if (_instance) _instance.OnRecycle -= value;
            }
        }

        protected InstanceWrapper(T instance)
        {
            _instance = instance;
            _instance.OnRecycle += Recycle;
        }

        protected bool IsAvailable() { return _instance != null; }

        public void UpdateInstance(T newInstance)
        {
            ClearEvent();
            _instance = newInstance;
            _instance.OnRecycle += Recycle;
        }

        protected virtual void Recycle(T t)
        {
            ClearEvent();
            _instance = null;
        }

        private void ClearEvent()
        {
            if (_instance) _instance.OnRecycle -= Recycle;
        }

        protected virtual void LogInstanceIsNull() { Debug.LogError(AudioConstant.LOG_HEADER + "The object that you are refering to is null."); }
    }
}