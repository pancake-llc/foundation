using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    public static partial class C
    {
        public static CoroutineHandle RunCoroutine(this MonoBehaviour owner, IEnumerator coroutine) { return new CoroutineHandle(owner, coroutine); }

        /// <summary>
        /// Sets the x/y/z transform.position using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetPosition(x: 5, z: 10), for example, only changing transform.position.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.position at.</param>
        /// <param name="x">If this is not null, transform.position.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.position.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.position.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform ChangePosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.position = transform.position.Change(x, y, z);
            return transform;
        }

        /// <summary>
        /// Sets the x/y/z transform.localPosition using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetLocalPosition(x: 5, z: 10), for example, only changing transform.localPosition.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.localPosition at.</param>
        /// <param name="x">If this is not null, transform.localPosition.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.localPosition.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.localPosition.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform ChangeLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.localPosition = transform.localPosition.Change(x, y, z);
            return transform;
        }

        /// <summary>
        /// Sets the x/y/z transform.localScale using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetLocalScale(x: 5, z: 10), for example, only changing transform.localScale.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.localScale at.</param>
        /// <param name="x">If this is not null, transform.localScale.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.localScale.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.localScale.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform ChangeLocalScale(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.localScale = transform.localScale.Change(x, y, z);
            return transform;
        }

        /// <summary>
        /// Sets the x/y/z transform.lossyScale using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetLossyScale(x: 5, z: 10), for example, only changing transform.lossyScale.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.lossyScale at.</param>
        /// <param name="x">If this is not null, transform.lossyScale.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.lossyScale.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.lossyScale.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform ChangeLossyScale(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            var lossyScale = transform.lossyScale.Change(x, y, z);

            transform.localScale = Vector3.one;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.localScale = new Vector3(lossyScale.x / transform.lossyScale.x,
                // ReSharper disable once Unity.InefficientPropertyAccess
                lossyScale.y / transform.lossyScale.y,
                // ReSharper disable once Unity.InefficientPropertyAccess
                lossyScale.z / transform.lossyScale.z);

            return transform;
        }

        /// <summary>
        /// Sets the x/y/z transform.eulerAngles using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetEulerAngles(x: 5, z: 10), for example, only changing transform.eulerAngles.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.eulerAngles at.</param>
        /// <param name="x">If this is not null, transform.eulerAngles.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.eulerAngles.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.eulerAngles.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform ChangeEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.eulerAngles = transform.eulerAngles.Change(x, y, z);
            return transform;
        }

        /// <summary>
        /// Sets the x/y/z transform.localEulerAngles using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetLocalEulerAngles(x: 5, z: 10), for example, only changing transform.localEulerAngles.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.localEulerAngles at.</param>
        /// <param name="x">If this is not null, transform.localEulerAngles.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.localEulerAngles.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.localEulerAngles.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform ChangeLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.localEulerAngles = transform.localEulerAngles.Change(x, y, z);
            return transform;
        }

        /// <summary>
        /// Destroy your self
        /// </summary>
        /// <param name="gameObject"></param>
        public static void Destroy(this GameObject gameObject)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(gameObject);
#else
            Object.Destroy(gameObject);
#endif
        }
        
        /// <summary>
        /// Destroys all gameObject's children
        /// </summary>
        /// <param name="transform"></param>
        public static void RemoveAllChildren(this Transform transform)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, "Transform RemoveAllChildren");
#endif

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(transform.GetChild(i).gameObject);
                }
                else
                {
                    Object.DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        /// Destroys all gameObject children
        /// </summary>
        /// <param name="gameObject"></param>
        public static void RemoveAllChildren(this GameObject gameObject) { gameObject.transform.RemoveAllChildren(); }

        /// <summary>
        /// Finds children by name, breadth first
        /// </summary>
        /// <param name="root"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform FindDeepChildBreadthFirst(this Transform root, string transformName)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                if (child.name == transformName)
                {
                    return child;
                }

                foreach (Transform t in child)
                {
                    queue.Enqueue(t);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds children by name, depth first
        /// </summary>
        /// <param name="root"></param>
        /// <param name="transformName"></param>
        /// <returns></returns>
        public static Transform FindDeepChildDepthFirst(this Transform root, string transformName)
        {
            foreach (Transform child in root)
            {
                if (child.name == transformName)
                {
                    return child;
                }

                var result = child.FindDeepChildDepthFirst(transformName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Changes the layer of a transform and all its children to the new one
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layerName"></param>
        public static void ChangeLayersRecursively(this Transform transform, string layerName)
        {
            transform.gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerName);
            }
        }

        /// <summary>
        /// Changes the layer of a transform and all its children to the new one
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layerIndex"></param>
        public static void ChangeLayersRecursively(this Transform transform, int layerIndex)
        {
            transform.gameObject.layer = layerIndex;
            foreach (Transform child in transform)
            {
                child.ChangeLayersRecursively(layerIndex);
            }
        }

        /// <summary>
        /// Traverse transform tree (root node first).
        /// </summary>
        /// <param name="root"> The root node of transform tree. </param>
        /// <param name="operate"> A custom operation on every transform node. </param>
        /// <param name="depthLimit"> Negative value means no limit, zero means root only, positive value means maximum children depth </param>
        public static void TraverseHierarchy(this Transform root, Action<Transform> operate, int depthLimit = -1)
        {
            operate(root);

            if (depthLimit != 0)
            {
                int count = root.childCount;
                for (int i = 0; i < count; i++)
                {
                    TraverseHierarchy(root.GetChild(i), operate, depthLimit - 1);
                }
            }
        }

        /// <summary>
        /// Traverse transform tree (leaf node first).
        /// </summary>
        /// <param name="root"> The root node of transform tree. </param>
        /// <param name="operate"> A custom operation on every transform node. </param>
        /// <param name="depthLimit"> Negative value means no limit, zero means root only, positive value means maximum children depth </param>
        public static void InverseTraverseHierarchy(this Transform root, Action<Transform> operate, int depthLimit = -1)
        {
            if (depthLimit != 0)
            {
                int count = root.childCount;
                for (int i = 0; i < count; i++)
                {
                    InverseTraverseHierarchy(root.GetChild(i), operate, depthLimit - 1);
                }
            }

            operate(root);
        }

        /// <summary>
        /// Find a transform in the transform tree (root node first)
        /// </summary>
        /// <param name="root"> The root node of transform tree. </param>
        /// <param name="match"> match function. </param>
        /// <param name="depthLimit"> Negative value means no limit, zero means root only, positive value means maximum children depth </param>
        /// <returns> The matched node or null if no matched. </returns>
        public static Transform SearchHierarchy(this Transform root, Predicate<Transform> match, int depthLimit = -1)
        {
            if (match(root)) return root;
            if (depthLimit == 0) return null;

            int count = root.childCount;
            Transform result = null;

            for (int i = 0; i < count; i++)
            {
                result = SearchHierarchy(root.GetChild(i), match, depthLimit - 1);
                if (result) break;
            }

            return result;
        }
    }
}