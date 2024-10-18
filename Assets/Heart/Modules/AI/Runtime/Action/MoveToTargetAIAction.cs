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
            if (!context.Sensor.tags.Contains(tag)) context.Sensor.tags.Add(tag);
        }

        public override void Execute(AIContext context)
        {
            var target = context.Sensor.GetClosestTarget(tag);
            if (target == null) return;

            context.target = target;
            context.Agent.SetDestination(target.position);
        }
    }
}
#endif