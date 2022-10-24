using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ManipulatorTarget(typeof(DisableInPlayModeAttribute))]
    sealed class DisableInPlayModeManipulator : MemberManipulator
    {
        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute)
        {
            member.VisibilityCallback += () => !EditorApplication.isPlaying;
        }
    }
}