using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.MobileInput
{
    public class TouchPickable : MonoBehaviour
    {
        [InfoBox("pickableTransform only needs to be set \nin case the collider of the pickable item is not on the root object of the pickable item")] [SerializeField]
        private Optional<Transform> pickableTransform;

        [SerializeField]
        [Tooltip(
            "When snapping is enabled, this value defines a position offset that is added to the center of the object when dragging. Note that this value is added on top of the snapOffset defined in the MobilePickingController. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 localSnapOffset = Vector2.zero;

        public Transform PickableTransform { get => pickableTransform.Value; set => pickableTransform = new Optional<Transform>(value); }

        public Vector2 LocalSnapOffset => localSnapOffset;

        public void Awake()
        {
            if (pickableTransform == null) pickableTransform = new Optional<Transform>(transform);

            if (gameObject.GetComponent<Collider>() == null && gameObject.GetComponent<Collider2D>() == null)
                Debug.LogError("TouchPickable must be placed on a gameObject that also has a Collider or Collider2D component attached.");
        }
    }
}