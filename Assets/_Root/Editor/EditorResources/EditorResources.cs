#if UNITY_EDITOR
using UnityEngine;

namespace Pancake.Editor
{
    public class EditorResources : ScriptableAssetSingleton<EditorResources>
    {
        [Header("save-data")] public Texture2D circleCheckmark;
        public TextAsset classTypeTemplate;
        public TextAsset valueTypeTemplate;
    }
}
#endif