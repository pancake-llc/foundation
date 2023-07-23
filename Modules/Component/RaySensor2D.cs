using System;
using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sensors
{
    [EditorIcon("sensor2d")]
    public class RaySensor2D : GameComponent
    {
        [Message("Raycast after raycastRate frame\nraycastRate = 1 => Raycast after every frame\nraycastRate = 2 => Raycast every 2 frames", Height = 42)]
        [SerializeField, Range(1, 8)]
        private int raycastRate = 1;

        [Message("How many sensor points should there be along the start and end point\nHigher = less performant but more accurate", Height = 30)] [SerializeField]
        private int sensorNumber = 2;

        [Space(8)] [SerializeField] private LayerMask layer;
        [SerializeField] private RaycastType raycastType;
        [SerializeField] private RaycastMode raycastMode;

        [SerializeField, ShowIf(nameof(raycastMode), RaycastMode.Vison), Indent] [OnValueChanged(nameof(OnOffsetChanged))]
        private float distance = 1f;

        [Space(8)] [SerializeField] private bool stopAfterFirstHit;
        [SerializeField] private bool detectOnStart = true;
#if UNITY_EDITOR
        [SerializeField] private bool showGizmos = true;
#endif
        [Space(8)] [SerializeField, NotNull] private Transform start;
        [SerializeField, NotNull] private Transform end;
        [SerializeField, NotNull] private Transform source;
        [SerializeField] private ScriptableEventGameObject detectedEvent;

        private Vector2[] _sensors;
        private Vector2[] _lastPositions;
        private RaycastHit2D[] _hits;
        private HashSet<Collider2D> _hitObjects = new HashSet<Collider2D>();

        private bool _isPlaying;
        private int _frames;
        private Vector2 _offset;

        [Flags]
        private enum RaycastType
        {
            Horizontal = 1 << 0,
            Vertical = 1 << 1,
            IntersectionTop = 1 << 2,
            IntersectionBottom = 1 << 3,
        };

        private enum RaycastMode
        {
            Transform,
            Vison
        }

        private void Awake()
        {
            Init();
            if (detectOnStart) Pulse();
        }

        private void Init()
        {
            _offset = new Vector2(distance, 0);
            _sensors = new Vector2[sensorNumber];
            _lastPositions = new Vector2[sensorNumber];

            float d = 1f / (sensorNumber - 1);
            var lerpValue = 0f;
            for (var i = 0; i < sensorNumber; i++)
            {
                _sensors[i] = Vector2.Lerp(start.localPosition, end.localPosition, lerpValue);
                lerpValue += d;
            }
        }

        public void Pulse()
        {
            // Reset _lastPositions
            for (var i = 0; i < _lastPositions.Length; ++i) _lastPositions[i] = source.TransformPoint(_sensors[i]);
            _hitObjects.Clear();
            _isPlaying = true;
        }

        public void Stop() { _isPlaying = false; }

        protected override void FixedTick()
        {
            if (!_isPlaying) return;

            _frames++;
            if (_frames % raycastRate != 0) return;
            _frames = 0;
            if (raycastMode == RaycastMode.Transform) ProcedureTransform();
            else ProcedureTransform(_offset);
        }

        private void ProcedureTransform(Vector2 offset = default)
        {
            for (int i = 0; i < _sensors.Length; i++)
            {
                Vector2 currentPosition = source.TransformPoint(_sensors[i]);

                if (raycastType.HasFlag(RaycastType.Horizontal)) Raycast(_lastPositions[i], currentPosition, RaycastType.Horizontal);

                //Raycast in intersection shape ( \ shape ) Top-to-Bottom
                if (raycastType.HasFlag(RaycastType.IntersectionTop) && i > 0)
                    Raycast(_lastPositions[i], source.TransformPoint(_sensors[i - 1]), RaycastType.IntersectionTop);

                //Raycast in intersection shape ( / shape ) Bottom-to-Top
                if (raycastType.HasFlag(RaycastType.IntersectionBottom) && i < sensorNumber - 1)
                    Raycast(_lastPositions[i], source.TransformPoint(_sensors[i + 1]), RaycastType.IntersectionBottom);

                _lastPositions[i] = currentPosition + offset;
            }

            if (raycastType.HasFlag(RaycastType.Vertical)) Raycast(_lastPositions[0], _lastPositions[sensorNumber - 1], RaycastType.Vertical);
        }

        private void Raycast(Vector2 from, Vector2 to, RaycastType type)
        {
            bool hitDetected = false;
            _hits = Physics2D.LinecastAll(from, to, layer);
            foreach (var hit in _hits)
            {
                if (hit.collider != null && hit.collider.transform != source)
                {
                    hitDetected = true;
                    HandleHit(hit);
                }
            }

#if UNITY_EDITOR
            if (showGizmos)
            {
                var lineColor = type switch
                {
                    RaycastType.Horizontal => Color.white,
                    RaycastType.Vertical => Color.cyan,
                    RaycastType.IntersectionTop => Color.magenta,
                    RaycastType.IntersectionBottom => Color.yellow,
                    _ => Color.white
                };

                Debug.DrawLine(from, to, hitDetected ? Color.red : lineColor, 0.4f);
            }
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

        private void OnOffsetChanged()
        {
            if (Application.isPlaying) _offset = new Vector2(distance, 0);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                foreach (var s in _sensors) Gizmos.DrawWireSphere(source.TransformPoint(s), 0.075f);
            }

            if (start != null && end != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(start.position, 0.1f);
                Gizmos.DrawWireSphere(end.position, 0.1f);
            }
        }
#endif
    }
}