using System;
using System.Collections;
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
        private Pool _pool;
        private string _nameContext;
        private bool _isInitialized;
        private Action<Action<string>> _registerAction;
        private Action<Action<string>> _unregisterAction;

        public void Init(Pool pool, bool active, Action<Action<string>> registerAction, Action<Action<string>> unregisterAction)
        {
            if (_isInitialized) return;

            gameObject.SetActive(active);
            _pool = pool;
            _registerAction = registerAction;
            _unregisterAction = unregisterAction;
            _isInitialized = true;
        }

        private void PoolMemberOnsceneUnloaded(string sceneName)
        {
            if (sceneName.Equals(_nameContext)) Return();
        }

        public void SetContext(string context)
        {
            _nameContext = context;
            _unregisterAction?.Invoke(PoolMemberOnsceneUnloaded);
            _registerAction?.Invoke(PoolMemberOnsceneUnloaded);
        }

        public void Return()
        {
            gameObject.SetActive(false);
            _unregisterAction?.Invoke(PoolMemberOnsceneUnloaded);
            _pool.PoolMembers.AddLast(this);
        }

        public void Return(float delay) { App.RunCoroutine(IeReturn(delay)); }

        private IEnumerator IeReturn(float delay)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            Return();
        }

        private void OnDestroy()
        {
            _unregisterAction?.Invoke(PoolMemberOnsceneUnloaded);
            _pool.PoolMembers.Remove(this);
        }
    }
}