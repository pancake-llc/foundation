using System;
using System.Collections.Generic;
using Pancake.Common;
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

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[16];
        private readonly HashSet<Collider2D> _hitObjects = new();
        private int _frames;
        private Vector2 _end;
        private int _count;

        private enum RayDirection
        {
            Left,
            Right,
            Top,
            Bottom
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
                _ => currentPoint
            };
            return endPosition;
        }

        private void Raycast(Vector2 from, Vector2 to)
        {
            var dir = (to - from).normalized;
            switch (shape)
            {
                case Shape.Ray:
                    _count = Physics2D.RaycastNonAlloc(from,
                        dir,
                        _hits,
                        range,
                        layer.value);
                    break;
                case Shape.Circle:
                    var filter = new ContactFilter2D {useTriggers = Physics2D.queriesHitTriggers};
                    filter.SetLayerMask(layer);
                    _count = Physics2D.CircleCast(from,
                        radius,
                        dir,
                        filter,
                        _hits,
                        range);
                    break;
                case Shape.Box:
                    var boxFilter = new ContactFilter2D {useTriggers = Physics2D.queriesHitTriggers};
                    boxFilter.SetLayerMask(layer);
                    _count = Physics2D.BoxCast(from,
                        Vector2.one * radius,
                        0f,
                        dir,
                        boxFilter,
                        _hits,
                        range);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_count <= 0) return;
            for (var i = 0; i < _count; i++)
            {
                var hit = _hits[i];
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

        public override Transform GetClosestTarget(StringConstant tag)
        {
            if (_count == 0) return null;

            Transform closestTarget = null;
            float closestDistance = Mathf.Infinity;
            var currentPosition = source.GetPositionXY();
            for (var i = 0; i < _count; i++)
            {
                if (newTagSystem)
                {
                    if (!_hits[i].collider.gameObject.HasTag(tag.Value)) continue;
                }
                else
                {
                    if (!_hits[i].collider.CompareTag(tag.Value)) continue;
                }

                float distanceToTarget = Vector2.Distance(_hits[i].point, currentPosition);
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
            var currentPosition = source.GetPositionXY();
            for (var i = 0; i < _count; i++)
            {
                float distanceToTarget = Vector2.Distance(_hits[i].point, currentPosition);
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
                var currentPoint = source.GetPositionXY();
                var endPosition = CalculateEndPosition(currentPoint);
                var dir = (endPosition - currentPoint).normalized;
                switch (shape)
                {
                    case Shape.Ray:
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(currentPoint, 0.1f);
                        Gizmos.DrawWireSphere(endPosition, 0.1f);

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(currentPoint, endPosition);
                        break;
                    case Shape.Circle:
                        DebugEditor.DrawCircleCast(currentPoint, radius, dir, range);
                        break;
                    case Shape.Box:
                        DebugEditor.DrawBoxCast2D(currentPoint,
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