using UnityEngine;

namespace Pancake
{
    public class Vector2Nudger : BaseBehaviour
    {
        public BaseSpringComponent nudgeable;
        public Vector2 value = new Vector2(20f, 20f);
        [InlineProperty(LabelWidth = 20)] public FloatRange frequency = new FloatRange(2f, 10f);
        public bool autoNudge;

        private float _lastNudgeTime;
        private float _nextNudgeFrequency;

        private void Awake() { _nextNudgeFrequency = Random.Range(frequency.a, frequency.b); }

        private void Update()
        {
            if (autoNudge && _lastNudgeTime + _nextNudgeFrequency < Time.time)
            {
                Nudge();
            }
        }

        public void Nudge()
        {
            (nudgeable as INudgeable<Vector2>)?.Nudge(value);
            _lastNudgeTime = Time.time;
            _nextNudgeFrequency = Random.Range(frequency.a, frequency.b);
        }
    }
}