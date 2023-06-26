using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sensor
{
    [System.Serializable]
    public class SteerAvoid
    {
        public List<Sensor> Sensors = new List<Sensor>();
        [SerializeField] List<GameObject> ignoreList = new List<GameObject>();
        public List<GameObject> IgnoreList => ignoreList;
        public float DesiredDistance = 1f;

        public void PulseSensors()
        {
            if (Sensors == null)
            {
                return;
            }

            foreach (var sensor in Sensors)
            {
                sensor?.PulseAll();
            }
        }

        public void SetAvoid(DirectionalGrid grid)
        {
            grid.Fill(0);
            if (Sensors == null)
            {
                return;
            }

            foreach (var sensor in Sensors)
            {
                foreach (var signal in sensor?.Signals)
                {
                    if (IgnoreList.Contains(signal.Object))
                    {
                        continue;
                    }

                    AvoidSignal(grid, sensor, signal);
                }
            }
        }

        void AvoidSignal(DirectionalGrid grid, Sensor sensor, Signal signal)
        {
            var vAvoid = AvoidVector(sensor, signal);

            if (vAvoid == Vector3.zero)
            {
                return;
            }

            grid.GradientFill(vAvoid, Mathf.Lerp(0.9f, 0f, vAvoid.magnitude / DesiredDistance));
        }

        Vector3 toSignal(Vector3 position, Signal signal) => signal.Bounds.ClosestPoint(position) - position;

        Vector3 AvoidVector(Sensor sensor, Signal signal)
        {
            var delta = toSignal(sensor.transform.position, signal);
            if (delta == Vector3.zero)
            {
                // We're inside the bounds of the signal. Avoid NaNs.
                delta = (signal.Bounds.center - sensor.transform.position).normalized * .01f;
            }

            var dist = delta.magnitude;
            var dir = delta / dist;

            return Mathf.Clamp(DesiredDistance - dist, 0, DesiredDistance) * dir;
        }

        public void DrawGizmos()
        {
            if (Sensors == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            foreach (var sensor in Sensors)
            {
                foreach (var signal in sensor?.Signals)
                {
                    if (IgnoreList.Contains(signal.Object))
                    {
                        continue;
                    }

                    var position = sensor.transform.position;
                    var delta = toSignal(position, signal);
                    SensorGizmos.RaycastHitGizmo(position + delta, -delta.normalized, delta.magnitude < DesiredDistance);
                }
            }
        }
    }
}