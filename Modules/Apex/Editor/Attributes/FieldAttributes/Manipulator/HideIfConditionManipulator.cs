using Pancake.Apex;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(HideIfAttribute))]
    public sealed class HideIfConditionManipulator : ConditionManipulator
    {
        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute)
        {
            base.Initialize(member, ManipulatorAttribute);
            member.VisibilityCallback += () => !EvaluateExpression();
        }
    }
}