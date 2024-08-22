using Pancake.Common;
using UnityEditor;

namespace PancakeEditor
{
    [InitializeOnLoad]
    internal static class SupportEditorSaveDataInternal
    {
        static SupportEditorSaveDataInternal() { EditorApplication.playModeStateChanged += OnPlayModeStateChanged; }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode) Data.SaveAll();
        }
    }
}