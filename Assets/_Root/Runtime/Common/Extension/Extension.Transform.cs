using System;
using System.Collections.Generic;

namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// <summary>
        /// Makes a copy of the Vector2 with changed x/y values, keeping all undefined values as they were before. Can be
        /// called with named parameters like vector.Change2(y: 5), for example, only changing the y component.
        /// </summary>
        /// <param name="vector">The Vector2 to be copied with changed values.</param>
        /// <param name="x">If this is not null, the x component is set to this value.</param>
        /// <param name="y">If this is not null, the y component is set to this value.</param>
        /// <returns>A copy of the Vector2 with changed values.</returns>
        public static Vector2 Change(this Vector2 vector, float? x = null, float? y = null)
        {
            if (x.HasValue) vector.x = x.Value;
            if (y.HasValue) vector.y = y.Value;
            return vector;
        }

        /// <summary>
        /// Makes a copy of the Vector3 with changed x/y/z values, keeping all undefined values as they were before. Can be
        /// called with named parameters like vector.Change3(x: 5, z: 10), for example, only changing the x and z components.
        /// </summary>
        /// <param name="vector">The Vector3 to be copied with changed values.</param>
        /// <param name="x">If this is not null, the x component is set to this value.</param>
        /// <param name="y">If this is not null, the y component is set to this value.</param>
        /// <param name="z">If this is not null, the z component is set to this value.</param>
        /// <returns>A copy of the Vector3 with changed values.</returns>
        public static Vector3 Change(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            if (x.HasValue) vector.x = x.Value;
            if (y.HasValue) vector.y = y.Value;
            if (z.HasValue) vector.z = z.Value;
            return vector;
        }

        /// <summary>
        /// Makes a copy of the Vector4 with changed x/y/z/w values, keeping all undefined values as they were before. Can be
        /// called with named parameters like vector.Change4(x: 5, z: 10), for example, only changing the x and z components.
        /// </summary>
        /// <param name="vector">The Vector4 to be copied with changed values.</param>
        /// <param name="x">If this is not null, the x component is set to this value.</param>
        /// <param name="y">If this is not null, the y component is set to this value.</param>
        /// <param name="z">If this is not null, the z component is set to this value.</param>
        /// <param name="w">If this is not null, the w component is set to this value.</param>
        /// <returns>A copy of the Vector4 with changed values.</returns>
        public static Vector4 Change(this Vector4 vector, float? x = null, float? y = null, float? z = null, float? w = null)
        {
            if (x.HasValue) vector.x = x.Value;
            if (y.HasValue) vector.y = y.Value;
            if (z.HasValue) vector.z = z.Value;
            if (w.HasValue) vector.w = w.Value;
            return vector;
        }

        /// <summary>
        /// Rotates a Vector2.
        /// </summary>
        /// <param name="v">The Vector2 to rotate.</param>
        /// <param name="angleRad">How far to rotate the Vector2 in radians.</param>
        /// <returns>The rotated Vector2.</returns>
        public static Vector2 RotateRad(this Vector2 v, float angleRad)
        {
            // http://answers.unity3d.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
            var sin = Mathf.Sin(angleRad);
            var cos = Mathf.Cos(angleRad);

            var tx = v.x;
            var ty = v.y;
            v.x = cos * tx - sin * ty;
            v.y = sin * tx + cos * ty;

            return v;
        }

        /// <summary>
        /// Rotates a Vector2.
        /// </summary>
        /// <param name="v">The Vector2 to rotate.</param>
        /// <param name="angleDeg">How far to rotate the Vector2 in degrees.</param>
        /// <returns>The rotated Vector2.</returns>
        public static Vector2 RotateDeg(this Vector2 v, float angleDeg) { return v.RotateRad(angleDeg * Mathf.Deg2Rad); }

        /// <summary>
        /// Creates a Vector2 with a length of 1 pointing towards a certain angle.
        /// </summary>
        /// <param name="angleRad">The angle in radians.</param>
        /// <returns>The Vector2 pointing towards the angle.</returns>
        public static Vector2 CreateVector2AngleRad(this float angleRad) { return new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)); }

        /// <summary>
        /// Creates a Vector2 with a length of 1 pointing towards a certain angle.
        /// </summary>
        /// <param name="angleDeg">The angle in degrees.</param>
        /// <returns>The Vector2 pointing towards the angle.</returns>
        public static Vector2 CreateVector2AngleDeg(this float angleDeg) { return CreateVector2AngleRad(angleDeg * Mathf.Deg2Rad); }

        /// <summary>
        /// Gets the rotation of a Vector2.
        /// </summary>
        /// <param name="vector">The Vector2.</param>
        /// <returns>The rotation of the Vector2 in radians.</returns>
        public static float GetAngleRad(this Vector2 vector) { return Mathf.Atan2(vector.y, vector.x); }

        /// <summary>
        /// Gets the rotation of a Vector2.
        /// </summary>
        /// <param name="vector">The Vector2.</param>
        /// <returns>The rotation of the Vector2 in degrees.</returns>
        public static float GetAngleDeg(this Vector2 vector) { return vector.GetAngleRad() * Mathf.Rad2Deg; }

        /// <summary>
        /// Sets the x/y/z transform.position using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetPosition(x: 5, z: 10), for example, only changing transform.position.x and z.
        /// </summary>
        /// <param name="transform">The transform to set the transform.position at.</param>
        /// <param name="x">If this is not null, transform.position.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.position.y is set to this value.</param>
        /// <param name="z">If this is not null, transform.position.z is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static Transform SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
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
        public static Transform SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
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
        public static Transform SetLocalScale(this Transform transform, float? x = null, float? y = null, float? z = null)
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
        public static Transform SetLossyScale(this Transform transform, float? x = null, float? y = null, float? z = null)
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
        public static Transform SetEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
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
        public static Transform SetLocalEulerAngles(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.localEulerAngles = transform.localEulerAngles.Change(x, y, z);
            return transform;
        }

        /// <summary>
        /// Reset transform setting
        /// Scale to one
        /// Position to zero
        /// Roration to zero
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Transform Reset(this Transform transform)
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(transform, "Transform Reset");
#endif

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            return transform;
        }

        /// <summary>
        /// Destroys a transform's children
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
        
        #region Extend

        #region Convert Methods

        public static Trans ToTrans(this Transform trans) { return Trans.GetGlobal(trans); }

        public static Trans ToRelativeTrans(this Transform trans, Transform relativeTo)
        {
            var m = trans.localToWorldMatrix;
            return Trans.Transform(relativeTo.worldToLocalMatrix * m);
        }

        public static Trans ToLocalTrans(this Transform trans) { return Trans.GetLocal(trans); }

        public static Matrix4x4 GetMatrix(this Transform trans) { return trans.localToWorldMatrix; }

        public static Matrix4x4 GetRelativeMatrix(this Transform trans, Transform relativeTo)
        {
            var m = trans.localToWorldMatrix;
            return relativeTo.worldToLocalMatrix * m;
        }

        public static Matrix4x4 GetLocalMatrix(this Transform trans) { return Matrix4x4.TRS(trans.localPosition, trans.localRotation, trans.localScale); }

        #endregion


        #region Matrix Methods

        public static Vector3 GetTranslation(this Matrix4x4 m)
        {
            var col = m.GetColumn(3);
            return new Vector3(col.x, col.y, col.z);
        }

        public static Quaternion GetRotation(this Matrix4x4 m)
        {
            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            Quaternion q = new Quaternion();
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
            q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
            q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
            q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
            return q;
        }

        public static Vector3 GetScale(this Matrix4x4 m)
        {
            //var xs = m.GetColumn(0);
            //var ys = m.GetColumn(1);
            //var zs = m.GetColumn(2);

            //var sc = new Vector3();
            //sc.x = Vector3.Magnitude(new Vector3(xs.x, xs.y, xs.z));
            //sc.y = Vector3.Magnitude(new Vector3(ys.x, ys.y, ys.z));
            //sc.z = Vector3.Magnitude(new Vector3(zs.x, zs.y, zs.z));

            //return sc;

            return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }

        #endregion

        #region GetAxis

        public static Vector3 GetAxis(CartesianAxis axis)
        {
            switch (axis)
            {
                case CartesianAxis.Xneg:
                    return Vector3.left;
                case CartesianAxis.Yneg:
                    return Vector3.down;
                case CartesianAxis.Zneg:
                    return Vector3.back;
                case CartesianAxis.X:
                    return Vector3.right;
                case CartesianAxis.Y:
                    return Vector3.up;
                case CartesianAxis.Z:
                    return Vector3.forward;
            }

            return Vector3.zero;
        }

        public static Vector3 GetAxis(this Transform trans, CartesianAxis axis)
        {
            if (trans == null) throw new System.ArgumentNullException("trans");

            switch (axis)
            {
                case CartesianAxis.Xneg:
                    return -trans.right;
                case CartesianAxis.Yneg:
                    return -trans.up;
                case CartesianAxis.Zneg:
                    return -trans.forward;
                case CartesianAxis.X:
                    return trans.right;
                case CartesianAxis.Y:
                    return trans.up;
                case CartesianAxis.Z:
                    return trans.forward;
            }

            return Vector3.zero;
        }

        public static Vector3 GetAxis(this Transform trans, CartesianAxis axis, bool inLocalSpace)
        {
            if (trans == null) throw new System.ArgumentNullException("trans");

            Vector3 v = Vector3.zero;
            switch (axis)
            {
                case CartesianAxis.Xneg:
                    v = -trans.right;
                    break;
                case CartesianAxis.Yneg:
                    v = -trans.up;
                    break;
                case CartesianAxis.Zneg:
                    v = -trans.forward;
                    break;
                case CartesianAxis.X:
                    v = trans.right;
                    break;
                case CartesianAxis.Y:
                    v = trans.up;
                    break;
                case CartesianAxis.Z:
                    v = trans.forward;
                    break;
            }

            return (inLocalSpace) ? trans.InverseTransformDirection(v) : v;
        }

        #endregion

        #region Parent

        public static Vector3 ParentTransformPoint(this Transform t, Vector3 pnt)
        {
            if (t.parent == null) return pnt;
            return t.parent.TransformPoint(pnt);
        }

        public static Vector3 ParentInverseTransformPoint(this Transform t, Vector3 pnt)
        {
            if (t.parent == null) return pnt;
            return t.parent.InverseTransformPoint(pnt);
        }

        #endregion

        #region Transform Methods

        public static void ZeroOut(this GameObject go, bool ignoreScale, bool bGlobal = false)
        {
            if (bGlobal)
            {
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                if (!ignoreScale) go.transform.localScale = Vector3.one;
            }
            else
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                if (!ignoreScale) go.transform.localScale = Vector3.one;
            }

            var rb = go.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        public static void ZeroOut(this Transform trans, bool ignoreScale, bool bGlobal = false)
        {
            if (bGlobal)
            {
                trans.position = Vector3.zero;
                trans.rotation = Quaternion.identity;
                if (!ignoreScale) trans.localScale = Vector3.one;
            }
            else
            {
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
                if (!ignoreScale) trans.localScale = Vector3.one;
            }
        }

        public static void ZeroOut(this Rigidbody body)
        {
            if (body.isKinematic) return;

            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        public static void CopyTransform(GameObject src, GameObject dst, bool bSetScale = false)
        {
            if (src == null) throw new System.ArgumentNullException("src");
            if (dst == null) throw new System.ArgumentNullException("dst");
            CopyTransform(src.transform, dst.transform);
        }

        public static void CopyTransform(Transform src, Transform dst, bool bSetScale = false)
        {
            if (src == null) throw new System.ArgumentNullException("src");
            if (dst == null) throw new System.ArgumentNullException("dst");

            Trans.GetGlobal(src).SetToGlobal(dst, bSetScale);

            foreach (Transform child in dst)
            {
                //match the transform by name
                var srcChild = src.Find(child.name);
                if (srcChild != null) CopyTransform(srcChild, child);
            }
        }


        public static Vector3 GetRelativePosition(this Transform trans, Transform relativeTo) { return relativeTo.InverseTransformPoint(trans.position); }

        public static Quaternion GetRelativeRotation(this Transform trans, Transform relativeTo)
        {
            //return trans.rotation * Quaternion.Inverse(relativeTo.rotation);
            return Quaternion.Inverse(relativeTo.rotation) * trans.rotation;
            //return Quaternion.Inverse(trans.rotation) * relativeTo.rotation;
        }

        /// <summary>
        /// Multiply a vector by only the scale part of a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 ScaleVector(this Matrix4x4 m, Vector3 v)
        {
            var sc = m.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Vector3 ScaleVector(this Trans t, Vector3 v) { return Matrix4x4.Scale(t.Scale).MultiplyPoint(v); }

        public static Vector3 ScaleVector(this Transform t, Vector3 v)
        {
            var sc = t.localToWorldMatrix.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        /// <summary>
        /// Inverse multiply a vector by on the scale part of a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 InverseScaleVector(this Matrix4x4 m, Vector3 v)
        {
            var sc = m.inverse.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Vector3 InvserScaleVector(this Trans t, Vector3 v)
        {
            var sc = t.Matrix.inverse.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Vector3 InverseScaleVector(this Transform t, Vector3 v)
        {
            var sc = t.worldToLocalMatrix.GetScale();
            return Matrix4x4.Scale(sc).MultiplyPoint(v);
        }

        public static Quaternion TranformRotation(this Matrix4x4 m, Quaternion rot) { return rot * m.GetRotation(); }

        public static Quaternion TransformRotation(Trans t, Quaternion rot) { return rot * t.Rotation; }

        public static Quaternion TransformRotation(this Transform t, Quaternion rot) { return rot * t.rotation; }

        public static Quaternion InverseTranformRotation(this Matrix4x4 m, Quaternion rot)
        {
            //return rot * Quaternion.Inverse(m.GetRotation());
            return Quaternion.Inverse(m.GetRotation()) * rot;
        }

        public static Quaternion InverseTransformRotation(Trans t, Quaternion rot)
        {
            //return rot * Quaternion.Inverse(t.Rotation);
            return Quaternion.Inverse(t.Rotation) * rot;
        }

        public static Quaternion InverseTransformRotation(this Transform t, Quaternion rot)
        {
            return Quaternion.Inverse(t.rotation) * rot;
        }

        /// <summary>
        /// Apply a transform to a Trans.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Trans TransformTrans(this Matrix4x4 m, Trans t)
        {
            t.Matrix *= m;
            return t;
        }

        public static Trans TransformTrans(this Trans t, Trans t2)
        {
            t2.Matrix *= t.Matrix;
            return t2;
        }

        public static Trans TransformTrans(this Transform t, Trans t2)
        {
            t2.Matrix *= t.localToWorldMatrix;
            return t2;
        }

        public static Trans InverseTransformTrans(this Matrix4x4 m, Trans t)
        {
            t.Matrix *= m.inverse;
            return t;
        }

        public static Trans InverseTransformTrans(this Trans t, Trans t2)
        {
            t2.Matrix *= t.Matrix.inverse;
            return t2;
        }

        public static Trans InverseTransformTrans(this Transform t, Trans t2)
        {
            t2.Matrix *= t.worldToLocalMatrix;
            return t2;
        }

        /// <summary>
        /// Transform a ray by a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Ray TransformRay(this Matrix4x4 m, Ray r) { return new Ray(m.MultiplyPoint(r.origin), m.MultiplyVector(r.direction)); }

        public static Ray TransformRay(this Trans t, Ray r) { return new Ray(t.TransformPoint(r.origin), t.TransformDirection(r.direction)); }

        public static Ray TransformRay(this Transform t, Ray r) { return new Ray(t.TransformPoint(r.origin), t.TransformDirection(r.direction)); }

        /// <summary>
        /// Inverse transform a ray by a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Ray InverseTransformRay(this Matrix4x4 m, Ray r)
        {
            m = m.inverse;
            return new Ray(m.MultiplyPoint(r.origin), m.MultiplyVector(r.direction));
        }

        public static Ray InverseTransformRay(this Trans t, Ray r) { return new Ray(t.InverseTransformPoint(r.origin), t.InverseTransformDirection(r.direction)); }

        public static Ray InverseTransformRay(this Transform t, Ray r) { return new Ray(t.InverseTransformPoint(r.origin), t.InverseTransformDirection(r.direction)); }

        /// <summary>
        /// Transform ray cast info by a transformation
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static RaycastInfo TransformRayCastInfo(this Matrix4x4 m, RaycastInfo r)
        {
            float dist = r.Distance;
            var sc = m.GetScale();
            if (sc.sqrMagnitude != 1.0f)
            {
                var v = r.Direction * r.Distance;
                v = Matrix4x4.Scale(sc).MultiplyPoint(v);
                dist = v.magnitude;
            }

            return new RaycastInfo(m.MultiplyPoint(r.Origin), m.MultiplyVector(r.Direction), dist);
        }

        public static RaycastInfo TransformRayCastInfo(this Trans t, RaycastInfo r)
        {
            float dist = r.Distance;
            var sc = t.Scale;
            if (sc.sqrMagnitude != 1.0f)
            {
                var v = r.Direction * r.Distance;
                v = Matrix4x4.Scale(sc).MultiplyPoint(v);
                dist = v.magnitude;
            }

            return new RaycastInfo(t.TransformPoint(r.Origin), t.TransformDirection(r.Direction), dist);
        }

        public static RaycastInfo TransformRayCastInfo(this Transform t, RaycastInfo r)
        {
            float dist = r.Distance;
            var sc = t.localToWorldMatrix.GetScale();
            if (sc.sqrMagnitude != 1.0f)
            {
                var v = r.Direction * r.Distance;
                v = Matrix4x4.Scale(sc).MultiplyPoint(v);
                dist = v.magnitude;
            }

            return new RaycastInfo(t.TransformPoint(r.Origin), t.TransformDirection(r.Direction), dist);
        }

        #endregion

        #region Transpose Methods

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be in world space.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in world space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundGlobalAnchor(this Transform trans, Vector3 anchor, Vector3 position, Quaternion rotation)
        {
            anchor = trans.InverseTransformPoint(anchor);
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be in world space.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in world space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundGlobalAnchor(this Transform trans, Trans anchor, Vector3 position, Quaternion rotation)
        {
            //anchor.Matrix *= trans.worldToLocalMatrix;
            anchor.Matrix = trans.worldToLocalMatrix * anchor.Matrix;
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in local space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundAnchor(this Transform trans, Vector3 anchor, Vector3 position, Quaternion rotation)
        {
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in local space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundAnchor(this Transform trans, Trans anchor, Vector3 position, Quaternion rotation)
        {
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor, position, rotation);
        }

        /// <summary>
        /// Set the position and rotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose in world space.</param>
        /// <param name="position">The new position in world space.</param>
        /// <param name="rotation">The new rotation in world space.</param>
        public static void TransposeAroundAnchor(this Transform trans, Transform anchor, Vector3 position, Quaternion rotation)
        {
            if (trans.parent != null)
            {
                position = trans.parent.InverseTransformPoint(position);
                rotation = trans.parent.InverseTransformRotation(rotation);
            }

            LocalTransposeAroundAnchor(trans, anchor.ToRelativeTrans(trans), position, rotation);
        }

        /// <summary>
        /// Set the localPosition and localRotation of a Transform as if its origin were that of 'anchor'. 
        /// Anchor should be local to the Transform where <0,0,0> would be the same as its true origin.
        /// </summary>
        /// <param name="trans">The transform to transpose.</param>
        /// <param name="anchor">The point around which to transpose relative to the transform.</param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void LocalTransposeAroundAnchor(this Transform trans, Vector3 anchor, Vector3 position, Quaternion rotation)
        {
            anchor = rotation * Vector3.Scale(anchor, trans.localScale);
            trans.localPosition = position - anchor;
            trans.localRotation = rotation;
        }

        public static void LocalTransposeAroundAnchor(this Transform trans, Trans anchor, Vector3 position, Quaternion rotation)
        {
            var anchorPos = rotation * Vector3.Scale(anchor.Position, trans.localScale);
            trans.localPosition = position - anchorPos;
            trans.localRotation = anchor.Rotation * rotation;
        }

        public static void LocalTransposeAroundAnchor(this Transform trans, Transform anchor, Vector3 position, Quaternion rotation)
        {
            var m = anchor.GetRelativeMatrix(trans);

            var anchorPos = rotation * Vector3.Scale(m.GetTranslation(), trans.localScale);
            trans.localPosition = position - anchorPos;
            trans.localRotation = m.GetRotation() * rotation;
        }

        #endregion

        #region Camera-Like

        public static Ray ViewportPointToRay(Vector2 vp, Matrix4x4 proj, Matrix4x4 trs)
        {
            var worldToCam = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * trs.inverse;
            var m = proj * worldToCam;
            var mInv = m.inverse;
            // near clipping plane point
            Vector4 p = new Vector4(vp.x * 2f - 1f, vp.y * 2f - 1f, -1f, 1f);
            var p0 = mInv * p;
            p0 /= p0.w;
            // far clipping plane point
            p.z = 1;
            var p1 = mInv * p;
            p1 /= p1.w;
            return new Ray(p0, (p1 - p0).normalized);
        }

        #endregion

        #endregion
    }
}