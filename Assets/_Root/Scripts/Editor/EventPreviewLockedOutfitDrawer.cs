#if UNITY_EDITOR
using Pancake.SceneFlow;
using UnityEditor;

namespace PancakeEditor
{
    [CustomEditor(typeof(ScriptableEventPreviewLockedOutfit))]
    public class EventPreviewLockedOutfitDrawer : UnityEditor.Editor
    {
    }
}
#endif