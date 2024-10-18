using System;
using System.Collections.Generic;
using Pancake.Common;
using Pancake.ExTag;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.AI
{
    public class RaySensor : Sensor
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

        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private readonly HashSet<Collider> _hitObjects = new();
        private int _count;

        private enum RayDirection
        {
            Left,
            Right,
            Top,
            Bottom,
            Forward,
            Back
        }

        private enum Shape
        {
            Ray,
            Sphere,
            Box
        }

        public override void Pulse()
        {
            _hitObjects.Clear();
            isPlaying = true;
        }

        protected override void Procedure()
        {
            var currentPoint = source.position;
            var endPosition = CalculateEndPosition(currentPoint);
            Raycast(currentPoint, endPosition);
        }

        private Vector3 CalculateEndPosition(Vector3 currentPoint)
        {
            var endPosition = direction switch
            {
                RayDirection.Left => currentPoint + Vector3.left * range,
                RayDirection.Right => currentPoint + Vector3.right * range,
                RayDirection.Top => currentPoint + Vector3.up * range,
                RayDirection.Bottom => currentPoint + Vector3.down * range,
                RayDirection.Forward => currentPoint + Vector3.forward * range,
                RayDirection.Back => currentPoint + Vector3.back * range,
                _ => currentPoint
            };
            return endPosition;
        }

        private void Raycast(Vector3 from, Vector3 to)
        {
            var dir = (to - from).normalized;
            var ray = new Ray(from, dir);

            switch (shape)
            {
                case Shape.Ray:
                    _count = Physics.RaycastNonAlloc(ray, _hits, range, layer.value);
                    break;
                case Shape.Sphere:
                    _count = Physics.SphereCastNonAlloc(from,
                        radius,
                        dir,
                        _hits,
                        range,
                        layer.value);

                    break;
                case Shape.Box:
                    _count = Physics.BoxCastNonAlloc(from,
                        Vector3.one * radius,
                        dir,
                        _hits,
                        Quaternion.identity,
                        range,
                        layer.value);
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

        private void HandleHit(RaycastHit hit)
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
                Debug.DrawRay(hit.point, Vector3.down * 0.4f, Color.red, 0.6f);
                Debug.DrawRay(hit.point, Vector3.right * 0.4f, Color.red, 0.6f);
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
                    if (!_hits[i].collider.gameObject.HasTag(tag.Value)) continue;
                }
                else
                {
                    if (!_hits[i].collider.CompareTag(tag.Value)) continue;
                }

                float distanceToTarget = Vector3.Distance(_hits[i].point, currentPosition);
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
                float distanceToTarget = Vector3.Distance(_hits[i].point, currentPosition);
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
            var currentPoint = source.position;
            var endPosition = CalculateEndPosition(currentPoint);
            var dir = (endPosition - currentPoint).normalized;
            if (source != null && showGizmos)
            {
                switch (shape)
                {
                    case Shape.Ray:
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(currentPoint, 0.1f);
                        Gizmos.DrawWireSphere(endPosition, 0.1f);

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(currentPoint, endPosition);
                        break;
                    case Shape.Sphere:
                        DebugEditor.DrawSphereCast3D(currentPoint, radius, dir, range);
                        break;
                    case Shape.Box:
                        DebugEditor.DrawBoxCast3D(currentPoint,
                            Vector3.one * radius,
                            dir,
                            Quaternion.identity,
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