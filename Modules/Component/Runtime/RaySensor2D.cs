using System;
using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    [HideMonoScript]
    public class RaySensor2D : Sensor
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

        private Vector2[] _sensors;
        private RaycastHit2D[] _hits;
        private HashSet<Collider2D> _hitObjects = new HashSet<Collider2D>();

        private int _frames;
        private Vector2 _left;
        private Vector2 _right;

        private enum RayDirection
        {
            Left,
            Right,
            Top,
            Bottom
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
                    _left = startConvert + Vector3.up * radius;
                    _right = startConvert + Vector3.down * radius;
                    break;
                case RayDirection.Top:
                case RayDirection.Bottom:
                    _left = startConvert + Vector3.left * radius;
                    _right = startConvert + Vector3.right * radius;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Init()
        {
            _sensors = new Vector2[sensorNumber];
            CalculateRadius();

            float d = 1f / (sensorNumber - 1);
            var lerpValue = 0f;
            for (var i = 0; i < sensorNumber; i++)
            {
                _sensors[i] = Vector2.Lerp(_left, _right, lerpValue);
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
                Vector2 currentPoint = source.TransformPoint(point);
                var endPosition = direction switch
                {
                    RayDirection.Left => currentPoint + Vector2.left * range,
                    RayDirection.Right => currentPoint + Vector2.right * range,
                    RayDirection.Top => currentPoint + Vector2.up * range,
                    RayDirection.Bottom => currentPoint + Vector2.down * range,
                    _ => currentPoint
                };
                Raycast(currentPoint, endPosition);
            }
        }

        private void Raycast(Vector2 from, Vector2 to)
        {
#if UNITY_EDITOR
#pragma warning disable 0219
            var hitDetected = false;
#pragma warning restore 0219
#endif
            _hits = Physics2D.LinecastAll(from, to, layer);
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

        private void HandleHit(RaycastHit2D hit)
        {
            if (_hitObjects.Contains(hit.collider)) return;

            _hitObjects.Add(hit.collider);
            if (detectedEvent != null) detectedEvent.Raise(hit.collider.gameObject);
            if (stopAfterFirstHit) Stop();

#if UNITY_EDITOR
            if (showGizmos)
            {
                Debug.DrawRay(hit.point + new Vector2(0, 0.2f), Vector2.down * 0.4f, Color.red, 0.6f);
                Debug.DrawRay(hit.point + new Vector2(-0.2f, 0), Vector2.right * 0.4f, Color.red, 0.6f);
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

            if (start != null && source!= null)
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
                }
            }
        }
#endif
    }
}