using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.Finder
{
    [InitializeOnLoad]
    public static partial class ProjectWindowOverlay
    {
        static ProjectWindowOverlay()
        {
            enabled = Enabled;
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
        }

        private static void ProjectWindowItemOnGUI(string guid, Rect rect)
        {
            if (enabled)
            {
                AssetInfo assetInfo = ProjectFinder.GetAsset(AssetDatabase.GUIDToAssetPath(guid));
                if (assetInfo != null)
                {
                    var content = new GUIContent(assetInfo.IsIncludedInBuild ? EditorResources.Instance.linkBlue : EditorResources.Instance.linkBlack,
                        assetInfo.IncludedStatus.ToString());
                    GUI.Label(new Rect(rect.width + rect.x - 20, rect.y + 1, 16, 16), content);
                }
                else
                {
                }
            }
        }

        private static bool enabled;

        public static bool Enabled
        {
            get { return enabled = EditorPrefs.GetBool("project_finder_window_overlay"); }
            set => EditorPrefs.SetBool("project_finder_window_overlay", enabled = value);
        }
    }
}