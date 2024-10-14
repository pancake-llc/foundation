#if PANCAKE_AI

namespace Pancake.AI
{
    [EditorIcon("so_blue_action")]
    public class MoveToTargetAIAction : AIAction
    {
        public override void Initialize(AIContext context) { context.Sensor.tags.Add(tag); }

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