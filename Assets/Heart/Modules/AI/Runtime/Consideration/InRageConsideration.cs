#if PANCAKE_AI
using Pancake.Common;
using UnityEngine;

namespace Pancake.AI
{
    [CreateAssetMenu(menuName = "Pancake/AI/Considerations/Range")]
    [EditorIcon("so_blue_consideration")]
    public class InRageConsideration : Consideration
    {
        public float maxDistance = 10f;
        public float maxAngle = 360f;
        public StringConstant tag;
        public AnimationCurve curve;

        public override float Evaluate(AIContext context)
        {
            if (!context.Sensor.tags.Contains(tag)) context.Sensor.tags.Add(tag);

            var targetTransform = context.Sensor.GetClosestTarget(tag);
            if (targetTransform == null) return 0f;

            var agentTransform = context.Agent.transform;

            bool isInRange = agentTransform.InRangeOf(targetTransform, maxDistance, maxAngle);
            if (!isInRange) return 0f;

            var directionToTarget = targetTransform.position - agentTransform.position;
            directionToTarget.y = 0;
            float distanceToTarget = directionToTarget.magnitude;

            float normalizedDistance = Mathf.Clamp01(distanceToTarget / maxDistance);

            float utility = curve.Evaluate(normalizedDistance);
            return Mathf.Clamp01(utility);
        }

        private void Reset() { curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)); }
    }
}
#endif