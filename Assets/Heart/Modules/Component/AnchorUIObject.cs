using Pancake.Common;

namespace Pancake.Component
{
    using UnityEngine;

    /// <summary>
    /// Anchor to UI object
    /// </summary>
    public class AnchorUIObject : MonoBehaviour
    {
        [SerializeField] private RectTransform anchor;

        private void OnEnable() { transform.position = anchor.ToWorldPosition(); }
    }
}