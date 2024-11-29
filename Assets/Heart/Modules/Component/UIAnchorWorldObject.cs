using Pancake.Common;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.Component
{
    /// <summary>
    /// Anchor to World object
    /// </summary>
    [EditorIcon("icon_default")]
    public class UIAnchorWorldObject : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly, InfoBox("GameObject Transform not UI RectTransform")]
        private Transform anchor;

        [SerializeField] private bool screenSpaceOverlay;
        [SerializeField, ShowIf(nameof(screenSpaceOverlay)), Indent] private Camera cam;
        private void OnEnable() { transform.position = anchor.ToUIPosition(screenSpaceOverlay ? cam : null); }
    }
}