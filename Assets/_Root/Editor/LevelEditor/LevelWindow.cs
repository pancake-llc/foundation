using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.LevelEditor
{
    internal static class LevelWindow
    {
        private static PreviewGenerator previewGenerator;

        private static PreviewGenerator PreviewGenerator
        {
            get
            {
                var generator = previewGenerator;
                if (generator != null) return generator;

                return previewGenerator = new PreviewGenerator {width = 512, height = 512, transparentBackground = true, sizingType = PreviewGenerator.ImageSizeType.Fit};
            }
        }

        private static Dictionary<GameObject, Texture2D> previewDict;

        public static void ClearPreviews()
        {
            if (previewDict != null)
            {
                foreach (var kvp in previewDict)
                {
                    Object.DestroyImmediate(kvp.Value);
                }

                previewDict.Clear();
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static void ClearPreview(GameObject go)
        {
            // ReSharper disable once RedundantAssignment
            // ReSharper disable once InlineOutVariableDeclaration
            Texture2D tex = null;
            if (previewDict?.TryGetValue(go, out tex) ?? false)
            {
                Object.DestroyImmediate(tex);
                previewDict.Remove(go);
            }
        }

        public static Texture2D GetPreview(GameObject go, bool canCreate = true)
        {
            if (!go) return null;
            if (!canCreate) return previewDict?.GetOrDefault(go);
            
            if (previewDict == null) previewDict = new Dictionary<GameObject, Texture2D>();
            previewDict.TryGetValue(go, out var tex);
            if (!tex)
            {
                tex = PreviewGenerator.CreatePreview(go.gameObject);
                previewDict[go] = tex;
            }

            return tex;
        }

        [MenuItem("Tools/Pancake/Level Editor &_3")]
        public static void OpenEditor()
        {
            var window = EditorWindow.GetWindow<LevelEditor>("Level Editor", true, InEditor.InspectorWindow);

            if (window)
            {
                window.Init();
                window.minSize = new Vector2(275, 0);
                window.Show(true);
            }
        }
    }
}