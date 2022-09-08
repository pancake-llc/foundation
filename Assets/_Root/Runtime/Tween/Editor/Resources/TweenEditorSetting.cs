#if UNITY_EDITOR

using UnityEngine;

namespace Pancake.Tween
{
    public class TweenEditorSetting : ScriptableAssetSingleton<TweenEditorSetting>
    {
        [Header("path")] public Texture2D moveToolPan;
        public Texture2D moveTool3D;
        public Texture2D rotateTool;
        public Texture2D addNodeForward;
        public Texture2D addNodeBack;
        public Texture2D removeNode;

        [Header("tween")] public Texture2D play;
        public Texture2D rightArrow;
        public Texture2D leftArrow;
    }
}

#endif