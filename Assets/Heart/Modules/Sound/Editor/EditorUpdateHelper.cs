using System;
using UnityEditor;

namespace PancakeEditor.Sound
{
    public abstract class EditorUpdateHelper : IDisposable
    {
        public event Action OnUpdate;

        private double _lastUpdateTime;
        protected float DeltaTime;
        protected abstract float UpdateInterval { get; }

        public virtual void Start()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            _lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        public virtual void End() { EditorApplication.update -= UpdateInternal; }

        private void UpdateInternal()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - _lastUpdateTime >= UpdateInterval)
            {
                DeltaTime = (float) (currentTime - _lastUpdateTime);
                _lastUpdateTime = currentTime;
                Update();
            }
        }

        protected virtual void Update() { OnUpdate?.Invoke(); }

        public virtual void Dispose()
        {
            EditorApplication.update -= UpdateInternal;
            OnUpdate = null;
        }
    }
}