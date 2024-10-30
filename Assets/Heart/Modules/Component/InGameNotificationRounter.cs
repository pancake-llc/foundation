using Pancake.Common;
using Pancake.Pools;
using UnityEngine;

namespace Pancake.Component
{
    [EditorIcon("icon_default")]
    public class InGameNotificationRouter : GameComponent
    {
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private RectTransform root;

        private MessageBinding<SpawnInGameNotiMessage> _binding;

        private void OnEnable()
        {
            _binding ??= new MessageBinding<SpawnInGameNotiMessage>(OnSpawn);
            _binding.Listen = true;
        }

        private void OnDisable() { _binding.Listen = false; }

        private void OnSpawn(SpawnInGameNotiMessage msg)
        {
            var instance = notificationPrefab.Request<InGameNotification>();
            instance.transform.SetParent(root, false);
            instance.transform.localScale = Vector3.one;
            var rectTransform = instance.transform.GetComponent<RectTransform>();
            rectTransform.SetLocalPositionZ(0);
            rectTransform.SetAnchoredPositionY(-444);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, root.rect.width - 100);
            instance.Show(msg.LocaleText);
        }
    }
}