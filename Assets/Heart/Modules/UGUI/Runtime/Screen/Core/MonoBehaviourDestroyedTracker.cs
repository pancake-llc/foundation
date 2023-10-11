namespace Pancake
{
    using UnityEngine;
    using System;

    internal sealed class MonoBehaviourDestroyedTracker : MonoBehaviour
    {
        public event Action Callback;

        private void OnDestroy() { Callback?.Invoke(); }
    }
}