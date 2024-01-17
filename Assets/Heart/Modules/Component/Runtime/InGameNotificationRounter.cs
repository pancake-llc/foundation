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

        private RectTransform _canvasRectTransform;
        
        protected void OnEnable()
        {
            spawnEvent.OnRaised += Spawn;
            returnPoolEvent.OnRaised += ReturnToPool;

            // trycatch only in editor to avoid case startup from any scene
#if UNITY_EDITOR
            try
            {
#endif
                _canvasRectTransform = getCanvasMasterEvent.Raise().GetComponent<RectTransform>();
                pool.SetParent(_canvasRectTransform, true);
#if UNITY_EDITOR
            }
            catch (Exception)
            {
                // ignored
            }
#endif
        }

        protected void OnDisable()
        {
            spawnEvent.OnRaised -= Spawn;
            returnPoolEvent.OnRaised -= ReturnToPool;
        }

        private void ReturnToPool(GameObject prefab) { pool.Return(prefab); }

        private void Spawn(LocaleText localeText)
        {
            var instance = pool.Request();
            instance.transform.localScale = Vector3.one;
            var rectTransform = instance.transform.GetComponent<RectTransform>();
            rectTransform.SetLocalPositionZ(0);
            rectTransform.SetAnchoredPositionY(-444);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _canvasRectTransform.rect.width - 100);
            var noti = instance.GetComponent<InGameNotification>();
            noti.Show(localeText);
        }
    }
}