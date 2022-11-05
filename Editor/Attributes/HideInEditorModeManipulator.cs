using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ManipulatorTarget(typeof(HideInEditorModeAttribute))]
    sealed class HideInEditorModeManipulator : MemberManipulator
    {
        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI() { EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying || !EditorApplication.isPaused); }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI() { EditorGUI.EndDisabledGroup(); }
    }
}