using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public class RangeSensor : Sensor
    {
        [Space(8)] [SerializeField] private float radius = 1f;

        [Space(8)] [SerializeField] private bool stopAfterFirstHit;
        [SerializeField] private bool detectOnStart = true;
#if UNITY_EDITOR
        [SerializeField] private bool showGizmos = true;
#endif
        [Space(8)] [SerializeField, NotNull] private Transform center;
        [SerializeField, NotNull] private Transform source;
        [SerializeField] private ScriptableEventGameObject detectedEvent;

        private Collider[] _hits;
        private HashSet<Collider> _hitObjects = new HashSet<Collider>();
        private int _frames;

        private void Awake()
        {
            if (detectOnStart) Pulse();
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
            var currentPosition = source.TransformPoint(center.localPosition);
            Raycast(currentPosition);
        }

        private void Raycast(Vector3 center)
        {
            _hits = Physics.OverlapSphere(center, radius, layer);
            foreach (var hit in _hits)
            {
                if (hit != null && hit.transform != source) HandleHit(hit);
            }
        }

        private void HandleHit(Collider hit)
        {
            if (_hitObjects.Contains(hit)) return;
            _hitObjects.Add(hit);
            if (detectedEvent != null) detectedEvent.Raise(hit.gameObject);
            if (stopAfterFirstHit) Stop();

#if UNITY_EDITOR
            if (showGizmos)
            {
                Debug.DrawRay(hit.transform.position, Vector2.down * 0.4f, Color.red, 0.6f);
                Debug.DrawRay(hit.transform.position, Vector2.right * 0.4f, Color.red, 0.6f);
            }
#endif
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (center != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(center.position, 0.1f);

                if (showGizmos)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(center.position, radius);
                }
            }
        }
#endif
    }
}