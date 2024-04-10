using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    public abstract class WindowBase : EditorWindow
    {
        protected const int LOAD_TIME_IN_FRAMES = 72;
        protected static int waitFramesTillReload = LOAD_TIME_IN_FRAMES;
        protected static bool showOnReload = true;
        protected GUIStyle htmlLabel;
        protected GUIStyle headerLabel;

        protected void OnEnable()
        {
            htmlLabel ??= new GUIStyle(EditorStyles.label) {richText = true};
            headerLabel ??= new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold, richText = true};

            Init();
        }

        protected virtual void Init() { }

        protected virtual void OnGUI()
        {
            GUILayout.FlexibleSpace();
            Uniform.DrawFooter("\u00a9 2024 pancake-llc");
        }
    }
}