#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pancake.BTag
{
    public static class BTagEditorExtensions
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
            BTag.AwakeComplete = false;
            BTag.StoreTagNames = false;
            BTag._AllTaggedGOs.Clear();
            BTag._AllTaggedGOsByIndex.Clear();
            BTag.AllTags.Clear();
            BTag.OnGameObjectStartListeners.Clear();
            BTag.OnGameObjectEnabledListeners.Clear();
            BTag.OnGameObjectDisabledListeners.Clear();
            BTag.OnGameObjectDestroyedListeners.Clear();
            TagGameObjectMasterUpdate.TaggedGameObjectsWithEvents.Clear();
        }
    }
}
#endif