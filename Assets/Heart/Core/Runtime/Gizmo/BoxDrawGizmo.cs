using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake
{
    [RequireComponent(typeof(BoxCollider))]
    [EditorIcon("icon_gizmo")]
    public sealed class BoxDrawGizmo : GizmoObject
    {
        [InfoBox("This script will only exist on the editor. Don't worry it will be automatically removed during the build process")]
        public Color color = Color.yellow;

        private BoxCollider _collider;
        private Vector3 _size;

        private void Start()
        {
            _collider = GetComponent<BoxCollider>();
            _size = _collider.size;
        }

        private void OnDrawGizmos()
        {
            var originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, _size);
            Gizmos.matrix = originalMatrix;
        }
    }
}