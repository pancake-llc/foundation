using System;
using System.Collections.Generic;
using Pancake.Common;
using Pancake.Draw;
using Pancake.ExTag;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.AI
{
    public class RaySensor2D : Sensor
    {
        [Space(8), SerializeField] private float range = 1f;
        [SerializeField] private RayDirection direction;
        [SerializeField] private Shape shape;
        [SerializeField, HideIf(nameof(shape), Shape.Ray), Indent] private float radius = 1f;
        [Space(8)] [SerializeField] private bool stopAfterFirstHit;

#if UNITY_EDITOR
        [SerializeField] private bool showGizmos = true;
#endif
        [Space(8), SerializeField, Required] private Transform source;
        [SerializeField] private GameObjectUnityEvent detectedEvent;

        private DynamicArray<RaycastHit2D> _hits = new();
        private readonly HashSet<Collider2D> _hitObjects = new();
        private int _frames;
        private Vector2 _end;

        private enum RayDirection
        {
            Left,
            Right,
            Top,
            Bottom,
            GameObject
        }

        private enum Shape
        {
            Ray,
            Circle,
            Box
        }

        public override void Pulse()
        {
            _hitObjects.Clear();
            isPlaying = true;
        }

        protected void FixedUpdate()
        {
            if (!isPlaying) return;

            _frames++;
            if (_frames % raycastRate != 0) return;
            _frames = 0;
            Procedure();
        }

        protected override void Procedure()
        {
            var currentPoint = source.GetPositionXY();
            var endPosition = CalculateEndPosition(currentPoint);
            Raycast(currentPoint, endPosition);
        }

        private Vector2 CalculateEndPosition(Vector2 currentPoint)
        {
            var endPosition = direction switch
            {
                RayDirection.Left => currentPoint + Vector2.left * range,
                RayDirection.Right => currentPoint + Vector2.right * range,
                RayDirection.Top => currentPoint + Vector2.up * range,
                RayDirection.Bottom => currentPoint + Vector2.down * range,
                RayDirection.GameObject => currentPoint + (Vector2) source.forward * range,
                _ => currentPoint
            };
            return endPosition;
        }

        private void Raycast(Vector2 from, Vector2 to)
        {
            var dir = (to - from).normalized;
            var filter = new ContactFilter2D {useTriggers = Physics2D.queriesHitTriggers};
            filter.SetLayerMask(layer);
            switch (shape)
            {
                case Shape.Ray:
                    Physics2DCast.RaycastNonAlloc(from,
                        dir,
                        filter,
                        _hits,
                        range);
                    break;
                case Shape.Circle:

                    Physics2DCast.CircleCastNonAlloc(from,
                        radius,
                        dir,
                        filter,
                        _hits,
                        range);
                    break;
                case Shape.Box:

                    Physics2DCast.BoxCastNonAlloc(from,
                        Vector2.one * radius,
                        0f,
                        dir,
                        filter,
                        _hits,
                        range);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_hits.Length <= 0) return;
            foreach (var hit in _hits)
            {
                if (hit.collider != null && hit.collider.transform != source)
                {
                    HandleHit(hit);

#if UNITY_EDITOR
                    if (showGizmos) Debug.DrawLine(from, hit.point, Color.red, 0f);
#endif
                }
            }
        }

        private void HandleHit(RaycastHit2D hit)
        {
            var col = hit.collider;
            if (!TagVerify(col)) return;
            if (_hitObjects.Contains(col)) return;
            _hitObjects.Add(col);
            detectedEvent?.Invoke(col.gameObject);
            if (stopAfterFirstHit) Stop();

#if UNITY_EDITOR
            if (showGizmos)
            {
                Debug.DrawRay(hit.point + new Vector2(0, 0.2f), Vector3.down * 0.4f, Color.red, 0.6f);
                Debug.DrawRay(hit.point + new Vector2(-0.2f, 0), Vector3.right * 0.4f, Color.red, 0.6f);
            }
#endif
        }

        public override Transform GetClosestTarget(StringKey tag)
        {
            if (_hits.Length == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.GetPositionXY();
            foreach (var hit in _hits)
            {
                if (newTagSystem)
                {
                    if (!hit.collider.gameObject.HasTag(tag.Name)) continue;
                }
                else
                {
                    if (!hit.collider.CompareTag(tag.Name)) continue;
                }

                float distanceToTarget = Vector2.Distance(hit.point, currentPosition);
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = hit.transform;
                }
            }

            return closestTarget;
        }

        public override Transform GetClosestTarget()
        {
            if (_hits.Length == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.GetPositionXY();
            foreach (var hit in _hits)
            {
                float distanceToTarget = Vector2.Distance(hit.point, currentPosition);
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = hit.transform;
                }
            }

            return closestTarget;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (source != null && showGizmos)
            {
                var currentPoint = source.GetPositionXY();
                var endPosition = CalculateEndPosition(currentPoint);
                var dir = (endPosition - currentPoint).normalized;
                switch (shape)
                {
                    case Shape.Ray:
                        ImGizmos.WireSphere3D(currentPoint, Quaternion.identity, 0.1f, Color.green);
                        ImGizmos.WireSphere3D(endPosition, Quaternion.identity, 0.1f, Color.green);
                        ImGizmos.Line3D(currentPoint, endPosition, Color.yellow);
                        break;
                    case Shape.Circle:
                        ImGizmos.DrawCircleCast(currentPoint, radius, dir, range);
                        break;
                    case Shape.Box:
                        ImGizmos.DrawBoxCast2D(currentPoint,
                            Vector3.one * radius,
                            0f,
                            dir,
                            range);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
#endif
    }
}