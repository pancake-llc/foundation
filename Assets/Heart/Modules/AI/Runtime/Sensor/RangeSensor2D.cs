using System.Collections.Generic;
using Pancake.Common;
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

        private readonly Collider2D[] _hits = new Collider2D[16];
        private readonly HashSet<Collider2D> _hitObjects = new();

        private int _count;

        public override void Pulse()
        {
            _hitObjects.Clear();
            isPlaying = true;
        }

        protected override void Procedure()
        {
            var legacyFilter = new ContactFilter2D {useTriggers = Physics2D.queriesHitTriggers};
            legacyFilter.SetLayerMask(layer);
            _count = Physics2D.OverlapCircle(source.GetPositionXY(), radius, legacyFilter, _hits);
            if (_count <= 0) return;

            for (var i = 0; i < _count; i++)
            {
                var hit = _hits[i];
                if (hit != null && hit.transform != source) HandleHit(hit);
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

        public override Transform GetClosestTarget(StringConstant tag)
        {
            if (_count == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.position;
            for (var i = 0; i < _count; i++)
            {
                if (newTagSystem)
                {
                    if (!_hits[i].gameObject.HasTag(tag.Value)) continue;
                }
                else
                {
                    if (!_hits[i].CompareTag(tag.Value)) continue;
                }
                
                float distanceToTarget = Vector3.Distance(_hits[i].transform.position, currentPosition);
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = _hits[i].transform;
                }
            }

            return closestTarget;
        }

        public override Transform GetClosestTarget()
        {
            if (_count == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.position;
            for (var i = 0; i < _count; i++)
            {
                float distanceToTarget = Vector3.Distance(_hits[i].transform.position, currentPosition);
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = _hits[i].transform;
                }
            }

            return closestTarget;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (source != null && showGizmos)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(source.position, 0.1f);

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(source.position, radius);
            }
        }
#endif
    }
}