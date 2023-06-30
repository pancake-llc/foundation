#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pancake.Tag
{
    public static class TagEditorStatic
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init() { EditorApplication.playModeStateChanged += ClearListeners; }

        private static void ClearListeners(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode) return;
            ClearListeners();
        }

        private static void ClearListeners()
        {
            TagStatic.AwakeComplete = false;
            TagStatic.StoreTagNames = false;
            TagStatic._AllTaggedGOs.Clear();
            TagStatic._AllTaggedGOsByIndex.Clear();
            TagStatic.AllTags.Clear();
            TagStatic.OnGameObjectStartListeners.Clear();
            TagStatic.OnGameObjectEnabledListeners.Clear();
            TagStatic.OnGameObjectDisabledListeners.Clear();
            TagStatic.OnGameObjectDestroyedListeners.Clear();
            TagGameObjectComponent.TaggedGameObjectsWithEvents.Clear();
        }
    }
}
#endif