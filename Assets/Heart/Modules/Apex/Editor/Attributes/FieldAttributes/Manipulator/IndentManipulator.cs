using Pancake.Apex;
using UnityEditor;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(IndentAttribute))]
    public sealed class IndentManipulator : MemberManipulator
    {
        private IndentAttribute attribute;
        private int previousLevel;

        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute) { attribute = ManipulatorAttribute as IndentAttribute; }

        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI()
        {
            previousLevel = EditorGUI.indentLevel;
            if (attribute.Additive)
                EditorGUI.indentLevel += attribute.level;
            else
                EditorGUI.indentLevel = attribute.level;
        }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI() { EditorGUI.indentLevel = previousLevel; }
    }
}