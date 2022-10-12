namespace Pancake.Editor
{
    [ManipulatorTarget(typeof(LabelAttribute))]
    sealed class LabelManipulator : MemberManipulator
    {
        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute)
        {
            LabelAttribute attribute = ManipulatorAttribute as LabelAttribute;
            member.GetLabel().text = attribute.name;
        }
    }
}