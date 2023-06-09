using Pancake.Apex;
using UnityEditor;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(DisableInPauseModeAttribute))]
    public sealed class DisableInPauseModeManipulator : MemberManipulator
    {
        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI() { EditorGUI.BeginDisabledGroup(EditorApplication.isPaused); }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI() { EditorGUI.EndDisabledGroup(); }
    }
}