using UnityEngine;

namespace Pancake.Spring
{
    public class Vector3Nudger : GameComponent
    {
        public BaseSpringComponent nudgeable;

        public Vector3 value = new Vector3(20f, 20f, 20f);

        //[InlineProperty(LabelWidth = 20)]
        public Vector2 frequency = new Vector2(2f, 10f);
        public bool autoNudge;

        private float _lastNudgeTime;
        private float _nextNudgeFrequency;

        private void Awake() { _nextNudgeFrequency = Random.Range(frequency.x, frequency.y); }

        protected override void Tick()
        {
            if (autoNudge && _lastNudgeTime + _nextNudgeFrequency < Time.time) Nudge();
        }

        public void Nudge()
        {
            (nudgeable as INudgeable<Vector3>)?.Nudge(value);
            _lastNudgeTime = Time.time;
            _nextNudgeFrequency = Random.Range(frequency.x, frequency.y);
        }
    }
}