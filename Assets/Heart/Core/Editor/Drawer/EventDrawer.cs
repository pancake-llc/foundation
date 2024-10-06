using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [CustomEditor(typeof(Pancake.Event), true), CanEditMultipleObjects]
    public sealed class EventDrawer : Editor
    {
        private static readonly string[] ScriptField = {"m_Script"};

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, ScriptField);
            DrawTriggerEventGUI();
        }

        private void DrawTriggerEventGUI()
        {
            if (target is not IEventTrigger) return;

            GUILayout.Space(5f);

            if (!GUILayout.Button("Trigger Event")) return;

            foreach (var t in targets)
            {
                if (t is IEventTrigger eventTrigger) eventTrigger.Trigger();
            }
        }
    }
}