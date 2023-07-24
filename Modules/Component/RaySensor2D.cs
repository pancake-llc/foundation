using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public class RaySensor2D : Sensor
    {
        [Message("How many sensor points should there be along the start and end point\nHigher = less performant but more accurate", Height = 30)] [SerializeField]
        private int sensorNumber = 2;

        [SerializeField] private float radius = 1f;
        [SerializeField] private LayerMask layer;
        
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

        private void Awake()
        {
            Init();
            if (detectOnStart) Pulse();
        }

        private void Init()
        {
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

        public override void Pulse() { throw new System.NotImplementedException(); }

        public override void Stop() { throw new System.NotImplementedException(); }
    }
}