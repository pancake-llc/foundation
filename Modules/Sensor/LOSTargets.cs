using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pancake.Sensor
{
    [AddComponentMenu("Sensors/LOS Targets")]
    public class LOSTargets : MonoBehaviour
    {
        public List<Transform> Targets;

        protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);

        public virtual void OnDrawGizmosSelected()
        {
            if (Targets == null) return;

            Gizmos.color = GizmoColor;
            foreach (Transform t in Targets)
            {
                if (t == null) return;
                Gizmos.DrawCube(t.position, Vector3.one * 0.1f);
            }
        }
    }
}