#if PANCAKE_AI
using UnityEngine;

namespace Pancake.AI
{
    [CreateAssetMenu(menuName = "Pancake/AI/Considerations/Constant")]
    [EditorIcon("so_blue_consideration")]
    public class ConstantConsideration : Consideration
    {
        public float value;

        public override float Evaluate(AIContext context) => value;
    }
}
#endif