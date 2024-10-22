using Pancake.Common;
using UnityEngine;

#if PANCAKE_AI
namespace Pancake.AI
{
    [CreateAssetMenu(menuName = "Pancake/AI/Considerations/Curve")]
    [EditorIcon("so_blue_consideration")]
    public class CurveConsideration : Consideration
    {
        public AnimationCurve curve;
        public StringConstant key;

        public override float Evaluate(AIContext context)
        {
            var value = context.GetData<float>(key.Value);
            return curve.Evaluate(value).Clamp01();
        }

        private void Reset() { curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)); }
    }
}
#endif