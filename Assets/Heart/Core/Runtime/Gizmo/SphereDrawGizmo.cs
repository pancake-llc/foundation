using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// A component that draw gizmo for collider
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    [EditorIcon("icon_gizmo")]
    public sealed class SphereDrawGizmo : GizmoObject
    {
        [InfoBox("This script will only exist on the editor. Don't worry it will be automatically removed during the build process")]
        public Color color = Color.yellow;

        private SphereCollider _collider;
        private float _radius;

        private void Start()
        {
            _collider = GetComponent<SphereCollider>();
            _radius = _collider.radius;
        }

        private void OnDrawGizmos()
        {
            var originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(Vector3.zero, _radius);
            Gizmos.matrix = originalMatrix;
        }
    }
}