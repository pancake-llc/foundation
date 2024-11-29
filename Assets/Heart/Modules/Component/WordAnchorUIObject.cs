using Pancake.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.Component
{
    /// <summary>
    /// Anchor to UI object
    /// </summary>
    [EditorIcon("icon_default")]
    public class WordAnchorUIObject : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly] private RectTransform anchor;
        [SerializeField] private bool screenSpaceOverlay;
        [SerializeField, ShowIf(nameof(screenSpaceOverlay)), Indent] private Camera cam;

        private void OnEnable() { transform.position = anchor.ToWorldPosition(screenSpaceOverlay ? cam : null); }
    }
}