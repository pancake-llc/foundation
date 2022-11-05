using UnityEditor;

namespace Pancake.Editor
{
    [ManipulatorTarget(typeof(ReadOnlyAttribute))]
    sealed class ReadonlyManipulator : MemberManipulator
    {
        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI() { EditorGUI.BeginDisabledGroup(true); }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI() { EditorGUI.EndDisabledGroup(); }
    }
}