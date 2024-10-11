using UnityEditor;

namespace PancakeEditor.Common
{
    public abstract class WindowBase : EditorWindow
    {
        protected const int LOAD_TIME_IN_FRAMES = 72;
        protected static int waitFramesTillReload = LOAD_TIME_IN_FRAMES;
        protected static bool showOnReload = true;

        protected void OnEnable() { Init(); }

        protected virtual void Init() { }

        protected virtual void OnGUI() { Uniform.DrawFooter("\u00a9 2024 pancake-llc"); }
    }
}