using System;
using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public class RaySensor : Sensor
    {
        [Message("How many sensor points should there be along the start and end point\nHigher = less performant but more accurate", Height = 30)] [SerializeField]
        private int sensorNumber = 2;

        [SerializeField] private float radius = 1f;
        [SerializeField] private float range = 1f;
        [SerializeField] private RayDirection direction;

        [Space(8)] [SerializeField] private bool stopAfterFirstHit;
        [SerializeField] private bool detectOnStart = true;
#if UNITY_EDITOR
        [SerializeField] private bool showGizmos = true;
#endif
        [Space(8)] [SerializeField, NotNull] private Transform start;
        [SerializeField, NotNull] private Transform source;
        [SerializeField] private ScriptableEventGameObject detectedEvent;

        private Vector3[] _sensors;
        private RaycastHit[] _hits;
        private HashSet<Collider> _hitObjects = new HashSet<Collider>();

        private int _frames;
        private Vector3 _left;
        private Vector3 _right;

        private enum RayDirection
        {
            Left,
            Right,
            Top,
            Bottom,
            Forward,
            Back
        }

        private void Awake()
        {
            Init();
            if (detectOnStart) Pulse();
        }

        private void CalculateRadius()
        {
            if (source == null || start == null) return;
            var startConvert = source.TransformPoint(start.localPosition);
            switch (direction)
            {
                case RayDirection.Left:
                case RayDirection.Right:
                    _left = startConvert + Vector3.down * radius;
                    _right = startConvert + Vector3.up * radius;
                    break;
                case RayDirection.Top:
                case RayDirection.Bottom:
                case RayDirection.Forward:
                case RayDirection.Back:
                    _left = startConvert + Vector3.left * radius;
                    _right = startConvert + Vector3.right * radius;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Init()
        {
            _sensors = new Vector3[sensorNumber];
            CalculateRadius();

            float d = 1f / (sensorNumber - 1);
            var lerpValue = 0f;
            for (var i = 0; i < sensorNumber; i++)
            {
                _sensors[i] = Vector3.Lerp(_left, _right, lerpValue);
                lerpValue += d;
            }
        }

        protected override void Pulse()
        {
            _hitObjects.Clear();
            isPlaying = true;
        }

        protected override void FixedTick()
        {
            if (!isPlaying) return;

            _frames++;
            if (_frames % raycastRate != 0) return;
            _frames = 0;
            Procedure();
        }

        private void Procedure()
        {
            foreach (var point in _sensors)
            {
                var currentPoint = source.TransformPoint(point);
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
                Raycast(currentPoint, endPosition);
            }
        }

        private void Raycast(Vector3 from, Vector3 to)
        {
#if UNITY_EDITOR
#pragma warning disable 0219
            var hitDetected = false;
#pragma warning restore 0219
#endif
            _hits = Physics.RaycastAll(from, (to - from).normalized, (from - to).magnitude, layer);
            foreach (var hit in _hits)
            {
                if (hit.collider != null && hit.collider.transform != source)
                {
#if UNITY_EDITOR
                    hitDetected = true;
#endif
                    HandleHit(hit);
                }
            }

#if UNITY_EDITOR
            if (showGizmos) Debug.DrawLine(from, to, hitDetected ? Color.red : Color.cyan, 0.4f);
#endif
        }

        private void HandleHit(RaycastHit hit)
        {
            if (_hitObjects.Contains(hit.collider)) return;

            _hitObjects.Add(hit.collider);
            if (detectedEvent != null) detectedEvent.Raise(hit.collider.gameObject);
            if (stopAfterFirstHit) Stop();

#if UNITY_EDITOR
            if (showGizmos)
            {
                Debug.DrawRay(hit.point, Vector2.down * 0.4f, Color.red, 0.6f);
                Debug.DrawRay(hit.point, Vector2.right * 0.4f, Color.red, 0.6f);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                foreach (var s in _sensors) Gizmos.DrawWireSphere(source.TransformPoint(s), 0.075f);
            }
            else
            {
                Gizmos.color = Color.blue;
                if (start != null)
                {
                    CalculateRadius();
                    Gizmos.DrawLine(_left, _right);
                }
            }

            if (start != null && source != null)
            {
                Gizmos.color = Color.yellow;
                var sourceStart = source.TransformPoint(start.localPosition);
                Gizmos.DrawWireSphere(sourceStart, 0.1f);
                switch (direction)
                {
                    case RayDirection.Left:
                        Gizmos.DrawWireSphere(sourceStart + Vector3.left * range, 0.1f);
                        break;
                    case RayDirection.Right:
                        Gizmos.DrawWireSphere(sourceStart + Vector3.right * range, 0.1f);
                        break;
                    case RayDirection.Top:
                        Gizmos.DrawWireSphere(sourceStart + Vector3.up * range, 0.1f);
                        break;
                    case RayDirection.Bottom:
                        Gizmos.DrawWireSphere(sourceStart + Vector3.down * range, 0.1f);
                        break;
                    case RayDirection.Forward:
                        Gizmos.DrawWireSphere(sourceStart + Vector3.forward * range, 0.1f);
                        break;
                    case RayDirection.Back:
                        Gizmos.DrawWireSphere(sourceStart + Vector3.back * range, 0.1f);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
#endif
    }
}