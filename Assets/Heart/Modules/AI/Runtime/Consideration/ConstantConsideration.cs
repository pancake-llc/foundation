#if PANCAKE_AI
using UnityEngine;

namespace Pancake.AI
{
    [CreateAssetMenu(menuName = "Pancake/AI/Consideration/Constant")]
    public class ConstantConsideration : Consideration
    {
        public float value;

        public override float Evaluate(AIContext context) => value;
    }
}
#endif