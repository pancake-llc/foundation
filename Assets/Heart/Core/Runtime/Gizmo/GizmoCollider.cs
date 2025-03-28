#if UNITY_EDITOR
using Pancake.Draw;
#endif
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake
{
    [RequireComponent(typeof(Collider))]
    [EditorIcon("icon_gizmo")]
    public sealed class GizmoCollider : GizmoObject
    {
        [InfoBox("This script will only exist on the editor. Don't worry it will be automatically removed during the build process")]
        public Color color = Color.yellow;

        private Collider _collider;

        private Collider Col
        {
            get
            {
                if (_collider == null) _collider = GetComponent<Collider>();

                return _collider;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos() { ImGizmos.Collider(Col, color); }
#endif
    }
}