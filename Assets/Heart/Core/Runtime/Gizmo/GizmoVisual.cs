using Pancake.Draw;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
namespace Pancake
{
    [EditorIcon("icon_gizmo")]
    public class GizmoVisual : GizmoObject
    {
        [LabelText("GameObject Name")] public bool showName;
        [ShowIf(nameof(showName)), LabelText("Color")] public Color nameColor = Color.red;
        public bool previewSphereArea;
        [ShowIf(nameof(previewSphereArea))] public float radius;
        [ShowIf(nameof(previewSphereArea)), LabelText("Color")] public Color sphereColor = Color.yellow;

        private void OnDrawGizmos()
        {
            if (showName)
            {
                ImGizmos.Label(transform.position,
                    nameColor,
                    LabelPivot.UpperCenter,
                    LabelAlignment.Center,
                    gameObject.name,
                    float.MaxValue);
            }

            if (previewSphereArea) ImGizmos.WireSphere3D(transform.position, transform.rotation, radius, sphereColor);
        }
    }
}
#endif