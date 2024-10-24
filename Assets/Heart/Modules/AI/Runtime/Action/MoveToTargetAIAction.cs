#if PANCAKE_AI
using UnityEngine;


namespace Pancake.AI
{
    [EditorIcon("so_blue_action")]
    [CreateAssetMenu(menuName = "Pancake/AI/Actions/Move To Target")]
    public class MoveToTargetAIAction : AIAction
    {
        public override void Initialize(AIContext context)
        {
            if (!context.Sensor.tags.Contains(targetTag)) context.Sensor.tags.Add(targetTag);
        }

        public override void Execute(AIContext context)
        {
            var target = context.Sensor.GetClosestTarget(targetTag);
            if (target == null) return;

            context.Target = target;
            context.Agent.SetDestination(target.position);
        }
    }
}
#endif