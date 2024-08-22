using Pancake.Common;
using Pancake.Pools;
using UnityEngine;
using VitalRouter;

namespace Pancake.Component
{
    [Routes]
    [EditorIcon("icon_default")]
    public partial class InGameNotificationRouter : GameComponent
    {
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private RectTransform root;
        
        private void Awake() { MapTo(Router.Default); }

        public void OnSpawn(SpawnInGameNotiCommand cmd)
        {
            var instance = notificationPrefab.Request<InGameNotification>();
            instance.transform.SetParent(root, false);
            instance.transform.localScale = Vector3.one;
            var rectTransform = instance.transform.GetComponent<RectTransform>();
            rectTransform.SetLocalPositionZ(0);
            rectTransform.SetAnchoredPositionY(-444);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, root.rect.width - 100);
            instance.Show(cmd.LocaleText);
        }
    }
}