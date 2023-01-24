using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public static partial class InEditor
    {
        internal static ProbeHit? GetMousePosition(out Vector3 mousePosition, SceneView sceneView)
        {
            var e = Event.current;
            var position = sceneView.GetInnerGuiPosition();

            mousePosition = Vector3.zero;
            if (position.Contains(e.mousePosition))
            {
                var filter = ProbeFilter.Default;
                var hit = Probe.Pick(filter, sceneView, e.mousePosition, out var point);
                if (e.type == EventType.MouseDown && e.button == 0) mousePosition = point;

                return hit;
            }

            return null;
        }

        public static bool Get2DMouseScenePosition(out Vector2 result)
        {
            result = Vector2.zero;

            var cam = Camera.current;
            var mp = Event.current.mousePosition;
            mp.y = cam.pixelHeight - mp.y;
            var ray = cam.ScreenPointToRay(mp);
            if (ray.direction != Vector3.forward) return false;

            result = ray.origin;
            return true;
        }

        public static Rect GetInnerGuiPosition(this SceneView sceneView)
        {
            var position = sceneView.position;
            position.x = position.y = 0;
            position.height -= EditorStyles.toolbar.fixedHeight;
            return position;
        }

        public static bool? IsWithinView(this SceneView sceneView, Rect? guiBounds, Vector2? guiCenter)
        {
            var innerPosition = GetInnerGuiPosition(sceneView);

            if (guiBounds != null)
            {
                return innerPosition.Overlaps(guiBounds.Value);
            }
            else if (guiCenter != null)
            {
                return innerPosition.Contains(guiCenter.Value);
            }

            return null;
        }

        private static Vector3 WorldToGUIPoint(this SceneView sceneView, Vector3 world)
        {
            // Does the same as this, but reimplemented to also work outside Handles.GUI scope
            // return HandleUtility.WorldToGUIPoint(worldPoint);

            world = Handles.matrix.MultiplyPoint(world);
            var screen = sceneView.camera.WorldToScreenPoint(world);
            var points = EditorGUIUtility.PixelsToPoints(screen);
            points.y = sceneView.GetInnerGuiPosition().height - points.y;
            return new Vector3(points.x, points.y, screen.z);
        }

        public static Rect? BoundsToGUIRect(this SceneView sceneView, Bounds worldBounds)
        {
            var worldPoints = new Vector3[8]{Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero,
                Vector3.zero};            
            worldBounds.GetPointsNoAlloc(worldPoints);

            Rect? guiRect = null;

            foreach (var worldPoint in worldPoints)
            {
                var guiPoint = sceneView.WorldToGUIPoint(worldPoint);

                if (guiPoint.z < 0)
                {
                    continue;
                }

                guiRect = guiRect.Encompass(guiPoint);
            }

            worldPoints = null;
            return guiRect;
        }

        public static void CalculateGuiBounds(this SceneView sceneView, GameObject[] targets, out Rect? guiBounds, out Vector2? guiCenter)
        {
            guiBounds = null;
            guiCenter = null;

            foreach (var target in targets)
            {
                if (target.CalculateBounds(out var worldBounds,
                        Space.World,
                        true,
                        true,
                        true,
                        true,
                        false))
                {
                    var targetGuiBounds = sceneView.BoundsToGUIRect(worldBounds);
                    guiBounds = guiBounds.Encompass(targetGuiBounds);
                }
            }

            var worldCenter = Vector3.zero;

            foreach (var target in targets)
            {
                worldCenter += target.transform.position;
            }

            worldCenter /= targets.Length;

            var _guiCenter = sceneView.WorldToGUIPoint(worldCenter);

            if (_guiCenter.z > 0)
            {
                guiCenter = _guiCenter;
            }
        }

        public static void FrameSelectedIfOutOfView(this SceneView sceneView)
        {
            CalculateGuiBounds(sceneView, Selection.transforms.Select(t => t.gameObject).ToArray(), out var guiBounds, out var guiCenter);

            if (!(IsWithinView(sceneView, guiBounds, guiCenter) ?? true))
            {
                sceneView.FrameSelected();
            }
        }

        /// <summary>
        /// Render an object on sceneView using sprite renderers
        /// </summary>
        public static void FakeRenderSprite(GameObject obs, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            var rends = obs.GetComponentsInChildren<SpriteRenderer>();
            foreach (var rend in rends)
            {
                var bounds = rend.bounds;
                var pos = rend.transform.position - obs.transform.position + position;
                DrawSprite(rend.sprite, pos, bounds.size.Multiply(scale));
            }
        }

        /// <summary>
        /// get inspector type to display window
        /// </summary>
        public static System.Type InspectorWindow => typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
    }
}