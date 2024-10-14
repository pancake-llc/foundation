#if PANCAKE_AI

namespace Pancake.AI
{
    [EditorIcon("so_blue_action")]
    public class IdleAIAction : AIAction
    {
        public override void Execute(AIContext context) { context.Agent.SetDestination(context.Agent.transform.position); }
    }
}
#endif