using System;
using Pancake.Common;
using Pancake.Localization;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public class InGameNotificationRouter : GameComponent
    {
        [Header("POOL")] [SerializeField] private ScriptableEventLocaleText spawnEvent;
        [SerializeField] private GameObject notificationPrefab;
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

        private void ReturnToPool(GameObject prefab) { prefab.Return(); }

        private void Spawn(LocaleText localeText)
        {
            var instance = notificationPrefab.Request<InGameNotification>();
            instance.transform.SetParent(_canvasRectTransform, false);
            instance.transform.localScale = Vector3.one;
            var rectTransform = instance.transform.GetComponent<RectTransform>();
            rectTransform.SetLocalPositionZ(0);
            rectTransform.SetAnchoredPositionY(-444);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _canvasRectTransform.rect.width - 100);
            instance.Show(localeText);
        }
    }
}