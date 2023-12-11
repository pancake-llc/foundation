using System;
using Pancake.Localization;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public class InGameNotificationRounter : GameComponent
    {
        [Header("POOL")] [SerializeField] private ScriptableEventLocaleText spawnEvent;
        [SerializeField] private GameObjectPool pool;
        [SerializeField] private ScriptableEventGameObject returnPoolEvent;

        [SerializeField] private ScriptableEventGetGameObject getCanvasMasterEvent;

        protected override void OnEnabled()
        {
            spawnEvent.OnRaised += Spawn;
            returnPoolEvent.OnRaised += ReturnToPool;

            // trycatch only in editor to avoid case startup from any scene
#if UNITY_EDITOR
            try
            {
#endif
                pool.SetParent(getCanvasMasterEvent.Raise().transform, true);
#if UNITY_EDITOR
            }
            catch (Exception)
            {
                // ignored
            }
#endif
            
        }

        protected override void OnDisabled()
        {
            spawnEvent.OnRaised -= Spawn;
            returnPoolEvent.OnRaised -= ReturnToPool;
        }

        private void ReturnToPool(GameObject prefab) { pool.Return(prefab); }

        private void Spawn(LocaleText localeText)
        {
            var instance = pool.Request();
            var noti = instance.GetComponent<InGameNotification>();
            noti.Show(localeText);
        }
    }
}