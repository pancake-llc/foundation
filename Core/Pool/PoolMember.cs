using System;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// Attached in each object marked as object in the pool
    /// Auto add by pool
    /// </summary>
    [AddComponentMenu("")]
    public class PoolMember : Passenger
    {
        public Pool Pool { get; private set; }
        public string NameContext { get; private set; }

        private bool _isInitialized;
        private Action<Action<string>> _registerAction;
        private Action<Action<string>> _unregisterAction;

        public void Init(Pool pool, bool active, Action<Action<string>> registerAction, Action<Action<string>> unregisterAction)
        {
            if (_isInitialized) return;

            gameObject.SetActive(active);
            Pool = pool;
            _registerAction = registerAction;
            _unregisterAction = unregisterAction;
            _isInitialized = true;
        }

        private void PoolMemberOnsceneUnloaded(string sceneName)
        {
            _unregisterAction?.Invoke(PoolMemberOnsceneUnloaded);
            if (sceneName.Equals(NameContext)) Pool.Despawn(this);
        }

        public void SetContext(string context)
        {
            NameContext = context;
            _unregisterAction?.Invoke(PoolMemberOnsceneUnloaded);
            _registerAction?.Invoke(PoolMemberOnsceneUnloaded);
        }
        

        private void OnDestroy()
        {
            if (Pool != null)
            {
                _unregisterAction?.Invoke(PoolMemberOnsceneUnloaded);
                Pool.PoolMembers.Remove(this);
            }
        }
    }
}