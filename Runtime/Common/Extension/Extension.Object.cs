using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// thanks for @carloswilkes
        /// <summary>This method allows you to destroy the target object in game and in edit mode, and it returns null.</summary>
        public static T Destroy<T>(this T o) where T : Object
        {
            if (o != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    Object.Destroy(o);
                }
                else
                {
                    Object.DestroyImmediate(o);
                }
#else
				Object.Destroy(o);
#endif
            }

            return null;
        }

        /// <summary>
        /// Instantiate object and connect prefab if possible
        /// </summary>
        public static T Instantiate<T>(this T prefab, Transform parent, bool connectPrefab = true) where T : Object
        {
#if UNITY_EDITOR
            if (!connectPrefab || Application.isPlaying) return Object.Instantiate(prefab, parent);
            return UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent) as T;
#else
            return Object.Instantiate(prefab, parent);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static Bounds GetRendererBounds(this GameObject go, bool includeInactive = true) { return GetBounds<Renderer>(go, includeInactive); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go"></param>
        /// <param name="includeInactive"></param>
        /// <param name="getBounds"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Bounds GetBounds<T>(this GameObject go, bool includeInactive = true, System.Func<T, Bounds> getBounds = null) where T : Component
        {
            if (getBounds == null)
                getBounds = (t) => (t as Collider)?.bounds ?? (t as Collider2D)?.bounds ?? (t as Renderer)?.bounds ?? default;
            var comps = go.GetComponentsInChildren<T>(includeInactive);

            Bounds bound = default;
            bool found = false;

            foreach (var comp in comps)
            {
                if (comp)
                {
                    if (!includeInactive)
                    {
                        if (!(comp as Collider)?.enabled ?? false) continue;
                        if (!(comp as Collider2D)?.enabled ?? false) continue;
                        if (!(comp as Renderer)?.enabled ?? false) continue;
                        if (!(comp as MonoBehaviour)?.enabled ?? false) continue;
                    }

                    if (!found || bound.size == Vector3.zero)
                    {
                        bound = getBounds(comp);
                        found = true;
                    }
                    else bound.Encapsulate(getBounds(comp));
                }
            }

            return bound;
        }

        /// <summary>
        /// Get scene objects of type T in the active scene, including inactive
        /// </summary>
        public static IEnumerable<T> GetSceneObjectsOfType<T>() where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            var rootObjs = scene.GetRootGameObjects();
            var objs = rootObjs.SelectMany(ro => ro.GetComponentsInChildren<T>(true));
            return objs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bounds"></param>
        /// <param name="space"></param>
        /// <param name="renderers"></param>
        /// <param name="colliders"></param>
        /// <param name="meshes"></param>
        /// <param name="graphics"></param>
        /// <param name="particles"></param>
        /// <returns></returns>
        public static bool CalculateBounds(
            this GameObject root,
            out Bounds bounds,
            Space space,
            bool renderers = true,
            bool colliders = true,
            bool meshes = false,
            bool graphics = true,
            bool particles = false)
        {
            bounds = new Bounds();

            var first = true;

            if (space == Space.Self)
            {
                if (renderers)
                {
                    var results = TempList<Renderer>.Get();
                    root.GetComponentsInChildren(results);

                    foreach (var renderer in results)
                    {
                        if (!renderer.enabled)
                        {
                            continue;
                        }

                        if (!particles && renderer is ParticleSystemRenderer)
                        {
                            continue;
                        }

                        var rendererBounds = renderer.bounds;

                        rendererBounds.SetMinMax(root.transform.InverseTransformPoint(rendererBounds.min), root.transform.InverseTransformPoint(rendererBounds.max));

                        if (first)
                        {
                            bounds = rendererBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(rendererBounds);
                        }
                    }

                    results.Dispose(); ;
                }

                if (meshes)
                {
                    var meshFilters = TempList<MeshFilter>.Get();
                    root.GetComponentsInChildren(meshFilters);

                    foreach (var meshFilter in meshFilters)
                    {
                        var mesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;

                        if (mesh == null)
                        {
                            continue;
                        }

                        var meshBounds = mesh.bounds;

                        meshBounds.SetMinMax(root.transform.InverseTransformPoint(meshFilter.transform.TransformPoint(meshBounds.min)),
                            root.transform.InverseTransformPoint(meshFilter.transform.TransformPoint(meshBounds.max)));

                        if (first)
                        {
                            bounds = meshBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(meshBounds);
                        }
                    }

                    meshFilters.Dispose();
                }

                if (graphics)
                {
                    var results = TempList<Graphic>.Get();
                    root.GetComponentsInChildren(results);

                    foreach (var graphic in results)
                    {
                        if (!graphic.enabled)
                        {
                            continue;
                        }

                        var graphicCorners =  TempArray.Temp(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
                        graphic.rectTransform.GetLocalCorners(graphicCorners);
                        var graphicsBounds = BoundsFromCorners(graphicCorners);
                        TempArray.Release(graphicCorners);

                        if (first)
                        {
                            bounds = graphicsBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(graphicsBounds);
                        }
                    }

                    results.Dispose();
                }

                if (colliders && first)
                {
                    var results = TempList<Collider>.Get();
                    root.GetComponentsInChildren(results);

                    foreach (var collider in results)
                    {
                        if (!collider.enabled)
                        {
                            continue;
                        }

                        var colliderBounds = collider.bounds;

                        colliderBounds.SetMinMax(root.transform.InverseTransformPoint(colliderBounds.min), root.transform.InverseTransformPoint(colliderBounds.max));

                        if (first)
                        {
                            bounds = colliderBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(colliderBounds);
                        }
                    }

                    results.Dispose();
                }

                return !first;
            }
            else // if (space == Space.World)
            {
                if (renderers)
                {
                    var results = TempList<Renderer>.Get();
                    root.GetComponentsInChildren(results);

                    foreach (var renderer in results)
                    {
                        if (!renderer.enabled)
                        {
                            continue;
                        }

                        if (!particles && renderer is ParticleSystemRenderer)
                        {
                            continue;
                        }

                        if (first)
                        {
                            bounds = renderer.bounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(renderer.bounds);
                        }
                    }

                    results.Dispose();
                }

                if (meshes)
                {
                    var filters = TempList<MeshFilter>.Get();
                    root.GetComponentsInChildren(filters);

                    foreach (var meshFilter in filters)
                    {
                        var mesh = (Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh);

                        if (mesh == null)
                        {
                            continue;
                        }

                        var meshBounds = mesh.bounds;

                        meshBounds.SetMinMax(root.transform.TransformPoint(meshBounds.min), root.transform.TransformPoint(meshBounds.max));

                        if (first)
                        {
                            bounds = meshBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(meshBounds);
                        }
                    }

                    filters.Dispose();
                }

                if (graphics)
                {
                    var results = TempList<Graphic>.Get();
                    root.GetComponentsInChildren(results);

                    foreach (var graphic in results)
                    {
                        if (!graphic.enabled)
                        {
                            continue;
                        }

                        var graphicCorners =  TempArray.Temp(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
                        graphic.rectTransform.GetWorldCorners(graphicCorners);
                        var graphicsBounds = BoundsFromCorners(graphicCorners);
                        TempArray.Release(graphicCorners);

                        if (first)
                        {
                            bounds = graphicsBounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(graphicsBounds);
                        }
                    }

                    results.Dispose();
                }

                if (colliders && first)
                {
                    var results = TempList<Collider>.Get();
                    root.GetComponentsInChildren(results);

                    foreach (var collider in results)
                    {
                        if (!collider.enabled)
                        {
                            continue;
                        }

                        if (first)
                        {
                            bounds = collider.bounds;
                            first = false;
                        }
                        else
                        {
                            bounds.Encapsulate(collider.bounds);
                        }
                    }

                    results.Dispose();
                }
            }

            return !first;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corners"></param>
        /// <returns></returns>
        public static Bounds BoundsFromCorners(Vector3[] corners)
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var minZ = float.MaxValue;

            var maxX = float.MinValue;
            var maxY = float.MinValue;
            var maxZ = float.MinValue;

            foreach (var corner in corners)
            {
                if (corner.x < minX)
                {
                    minX = corner.x;
                }

                if (corner.y < minY)
                {
                    minY = corner.y;
                }

                if (corner.z < minZ)
                {
                    minZ = corner.z;
                }

                if (corner.x > minX)
                {
                    maxX = corner.x;
                }

                if (corner.y > minY)
                {
                    maxY = corner.y;
                }

                if (corner.z > minZ)
                {
                    maxZ = corner.z;
                }
            }

            return new Bounds() { min = new Vector3(minX, minY, minZ), max = new Vector3(maxX, maxY, maxZ) };
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="array">array has 8 element</param>
        public static void GetPointsNoAlloc(this Bounds bounds, Vector3[] array)
        {
            var min = bounds.min;
            var max = bounds.max;

            array[0] = new Vector3(min.x, min.y, min.z);
            array[1] = new Vector3(min.x, min.y, max.z);
            array[2] = new Vector3(min.x, max.y, min.z);
            array[3] = new Vector3(min.x, max.y, max.z);
            array[4] = new Vector3(max.x, min.y, min.z);
            array[5] = new Vector3(max.x, min.y, max.z);
            array[6] = new Vector3(max.x, max.y, min.z);
            array[7] = new Vector3(max.x, max.y, max.z);
        }

        /// <summary>
        /// Get all components of specified Layer in childs
        /// </summary>
        public static List<Transform> GetObjectsOfLayerInChilds(this GameObject gameObject, int layer)
        {
            List<Transform> list = new List<Transform>();
            CheckChildsOfLayer(gameObject.transform, layer, list);
            return list;
        }

        /// <summary>
        /// Get all components of specified Layer in childs
        /// </summary>
        public static List<Transform> GetObjectsOfLayerInChilds(this GameObject gameObject, string layer)
        {
            return gameObject.GetObjectsOfLayerInChilds(LayerMask.NameToLayer(layer));
        }

        /// <summary>
        /// Get all components of specified Layer in childs
        /// </summary>
        public static List<Transform> GetObjectsOfLayerInChilds(this Component component, string layer)
        {
            return component.GetObjectsOfLayerInChilds(LayerMask.NameToLayer(layer));
        }

        /// <summary>
        /// Get all components of specified Layer in childs
        /// </summary>
        public static List<Transform> GetObjectsOfLayerInChilds(this Component component, int layer) { return component.gameObject.GetObjectsOfLayerInChilds(layer); }

        private static void CheckChildsOfLayer(Transform transform, int layer, List<Transform> childsCache)
        {
            foreach (Transform t in transform)
            {
                CheckChildsOfLayer(t, layer, childsCache);

                if (t.gameObject.layer != layer) continue;
                childsCache.Add(t);
            }
        }
        
#if !UNITY_2019_2_OR_NERER
        public static bool TryGetComponent<T>(this GameObject gameObject, out T component)
        {
            component = gameObject.GetComponent<T>();
            return component != null;
        }
#endif

        /// <summary>
        /// Get component if it exists or add a new one.
        /// </summary>
        public static T GetComponentSafely<T>(this GameObject target) where T : Component
        {
            if (!target.TryGetComponent(out T component))
            {
                component = target.AddComponent<T>();
            }

            return component;
        }


        /// <summary>
        /// Get component if it exists or add a new one.
        /// </summary>
        public static T GetComponentSafely<T>(this Component target) where T : Component { return target.gameObject.GetComponentSafely<T>(); }
        
        /// <summary>
        /// Get the RectTransform component.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static RectTransform rectTransform(this GameObject target) { return target.transform as RectTransform; }
        
        /// <summary>
        /// add blank button
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal static Button AddBlankButtonComponent(this GameObject target)
        {
            var button = target.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            return button;
        }
    }
}