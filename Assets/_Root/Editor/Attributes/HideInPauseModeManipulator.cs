using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ManipulatorTarget(typeof(HideInPauseModeAttribute))]
    sealed class HideInPauseModeManipulator : MemberManipulator
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