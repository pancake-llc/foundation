using System.Collections.Generic;
using Pancake.Common;
#if UNITY_EDITOR
using Pancake.Draw;
#endif
using Pancake.ExTag;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.AI
{
    public class RangeSensor2D : Sensor
    {
        [Space(8), SerializeField] private float radius = 1f;
        [SerializeField] private bool stopAfterFirstHit;
#if UNITY_EDITOR
        [SerializeField] private bool showGizmos = true;
#endif
        [Space(8), SerializeField, Required] private Transform source;

        [SerializeField] private GameObjectUnityEvent detectedEvent;

        private readonly HashSet<Collider2D> _hitObjects = new();
        private DynamicArray<Collider2D> _hits = new();
        
        public override void Pulse()
        {
            _hitObjects.Clear();
            isPlaying = true;
        }

        protected override void Procedure()
        {
            var legacyFilter = new ContactFilter2D {useTriggers = Physics2D.queriesHitTriggers};
            legacyFilter.SetLayerMask(layer);
            var result = Physics2DCast.OverlapCircleNonAlloc(source.GetPositionXY(), radius, legacyFilter, _hits);
            if (result.Length <= 0) return;

            foreach (var col in result)
            {
                if (col != null && col.transform != source) HandleHit(col);
            }
        }

        private void HandleHit(Collider2D hit)
        {
            if (!TagVerify(hit)) return;
            if (_hitObjects.Contains(hit)) return;
            _hitObjects.Add(hit);
            detectedEvent?.Invoke(hit.gameObject);
            if (stopAfterFirstHit) Stop();

#if UNITY_EDITOR
            if (showGizmos)
            {
                Debug.DrawRay((Vector2) hit.transform.position + new Vector2(0, 0.2f), Vector2.down * 0.4f, Color.red, 0.6f);
                Debug.DrawRay((Vector2) hit.transform.position + new Vector2(-0.2f, 0), Vector2.right * 0.4f, Color.red, 0.6f);
            }
#endif
        }

        public override Transform GetClosestTarget(StringKey tag)
        {
            if (_hits.Length == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.position;
            foreach (var col in _hits)
            {
                if (newTagSystem)
                {
                    if (!col.gameObject.HasTag(tag.Name)) continue;
                }
                else
                {
                    if (!col.CompareTag(tag.Name)) continue;
                }

                float distanceToTarget = Vector3.Distance(col.transform.position, currentPosition);
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = col.transform;
                }
            }

            return closestTarget;
        }

        public override Transform GetClosestTarget()
        {
            if (_hits.Length == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.position;
            foreach (var col in _hits)
            {
                float distanceToTarget = Vector3.Distance(col.transform.position, currentPosition);
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = col.transform;
                }
            }

            return closestTarget;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (source != null && showGizmos)
            {
                ImGizmos.WireDisc3D(source.position,
                    Quaternion.identity,
                    0.1f,
                    GizmoDrawAxis.Z,
                    Color.green);
                ImGizmos.WireDisc3D(source.position,
                    Quaternion.identity,
                    radius,
                    GizmoDrawAxis.Z,
                    Color.white);
            }
        }
#endif
    }
}