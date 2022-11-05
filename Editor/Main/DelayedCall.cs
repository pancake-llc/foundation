using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class DelayedCall
    {
        public readonly float delay;
        public readonly Action callback;
        private readonly float _startupTime;

        public DelayedCall(float delay, Action callback)
        {
            this.delay = delay;
            this.callback = callback;
            _startupTime = Time.realtimeSinceStartup;
            EditorApplication.update += new EditorApplication.CallbackFunction(Update);
        }

        private void Update()
        {
            if ((double) Time.realtimeSinceStartup - (double) _startupTime < (double) delay) return;
            if (EditorApplication.update != null) EditorApplication.update -= new EditorApplication.CallbackFunction(Update);
            if (callback == null) return;
            callback();
        }
    }
}