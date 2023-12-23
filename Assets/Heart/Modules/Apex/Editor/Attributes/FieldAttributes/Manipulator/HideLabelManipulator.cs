using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(HideLabelAttribute))]
    public sealed class HideLabelManipulator : MemberManipulator
    {
        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute) { member.SetLabel(GUIContent.none); }
    }
}