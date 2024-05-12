using Pancake.Common;
using UnityEngine;

namespace Pancake.Component
{
    public class InGameNotificationRouter : GameComponent
    {
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private RectTransform root;

        private EventBinding<SpawnInGameNotiEvent> _binding;

        private void Awake() { _binding = new EventBinding<SpawnInGameNotiEvent>(OnSpawn); }

        protected void OnEnable() { _binding.Listen = true; }

        protected void OnDisable() { _binding.Listen = false; }

        private void OnSpawn(SpawnInGameNotiEvent arg)
        {
            var instance = notificationPrefab.Request<InGameNotification>();
            instance.transform.SetParent(root, false);
            instance.transform.localScale = Vector3.one;
            var rectTransform = instance.transform.GetComponent<RectTransform>();
            rectTransform.SetLocalPositionZ(0);
            rectTransform.SetAnchoredPositionY(-444);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, root.rect.width - 100);
            instance.Show(arg.localeText);
        }
    }
}