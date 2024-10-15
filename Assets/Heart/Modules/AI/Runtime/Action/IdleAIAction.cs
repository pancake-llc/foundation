using UnityEngine;

#if PANCAKE_AI

namespace Pancake.AI
{
    [EditorIcon("so_blue_action")]
    [CreateAssetMenu(menuName = "Pancake/AI/Actions/Idle")]
    public class IdleAIAction : AIAction
    {
        public override void Execute(AIContext context) { context.Agent.SetDestination(context.Agent.transform.position); }
    }
}
#endif