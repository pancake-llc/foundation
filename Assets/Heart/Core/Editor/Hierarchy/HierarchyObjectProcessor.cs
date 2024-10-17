using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pancake;

namespace PancakeEditor
{
    public sealed class HierarchyObjectProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 0;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var hierarchyObjects = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<HierarchyObject>())
                .Where(x => x != null)
                .OrderByDescending(x => GetDepth(x.transform));

            foreach (var obj in hierarchyObjects)
            {
                if (obj == null) continue;

                switch (GetHierarchyObjectMode(obj))
                {
                    case HierarchyObjectMode.None:
                        break;
                    case HierarchyObjectMode.RemoveInPlayMode:
                        obj.transform.DetachChildren();
                        Object.DestroyImmediate(obj.gameObject);
                        break;
                    case HierarchyObjectMode.RemoveInBuild:
                        if (EditorApplication.isPlaying) break;
                        obj.transform.DetachChildren();
                        Object.DestroyImmediate(obj.gameObject);
                        break;
                }
            }
        }

        private static HierarchyObjectMode GetHierarchyObjectMode(HierarchyObject obj)
        {
            return obj.HierarchyObjectMode != HierarchyObject.Mode.UseSettings
                ? (HierarchyObjectMode) (obj.HierarchyObjectMode - 1)
                : HierarchySettings.HierarchyObjectMode;
        }

        private static int GetDepth(Transform transform)
        {
            var depth = 0;
            var parent = transform.parent;
            while (parent != null)
            {
                depth++;
                parent = parent.parent;
            }

            return depth;
        }
    }
}