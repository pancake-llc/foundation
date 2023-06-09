using Pancake.Apex;
using UnityEditor;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(EnableIfAttribute))]
    public sealed class EnableIfConditionManipulator : ConditionManipulator
    {
        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI() { EditorGUI.BeginDisabledGroup(!EvaluateExpression()); }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI() { EditorGUI.EndDisabledGroup(); }
    }
}