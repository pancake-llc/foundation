using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pancake;

namespace PancakeEditor
{
    public sealed class GizmoObjectProcessor : IProcessSceneWithReport
    {
        public int callbackOrder => 1;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var hierarchyObjects = scene.GetRootGameObjects()
                .SelectMany(x => x.GetComponentsInChildren<GizmoObject>())
                .Where(x => x != null)
                .OrderByDescending(x => GetDepth(x.transform));

            foreach (var obj in hierarchyObjects)
            {
                if (obj == null) continue;

                if (EditorApplication.isPlaying) break;
                obj.transform.DetachChildren();
                Object.DestroyImmediate(obj.gameObject);
            }
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