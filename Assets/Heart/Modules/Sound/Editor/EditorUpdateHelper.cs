using System;
using UnityEditor;

namespace PancakeEditor.Sound
{
    public abstract class EditorUpdateHelper
    {
        // TODO: use AnimBase instaed?
        public event Action OnUpdate;

        private double _lastUpdateTime;
        protected abstract float UpdateInterval { get; }

        public virtual void Start()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;

            _lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        public virtual void End() { EditorApplication.update -= Update; }

        private void Update()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - _lastUpdateTime >= UpdateInterval)
            {
                _lastUpdateTime = currentTime;

                OnUpdate?.Invoke();
            }
        }
    }
}