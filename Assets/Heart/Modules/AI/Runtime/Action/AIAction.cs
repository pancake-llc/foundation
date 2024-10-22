#if PANCAKE_AI
using UnityEngine;

namespace Pancake.AI
{
    public abstract class AIAction : ScriptableObject
    {
        public StringConstant targetTag;
        public Consideration consideration;

        public virtual void Initialize(AIContext context) { }

        public float CalculatePriority(AIContext context) => consideration.Evaluate(context);

        public abstract void Execute(AIContext context);
    }
}
#endif