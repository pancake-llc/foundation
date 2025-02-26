namespace Pancake.Draw
{
#if UNITY_EDITOR
    using System;
    using UnityEngine;
    using UnityEditor;

    public enum GizmoDrawAxis
    {
        X,
        Y,
        Z,
    }

    /// <summary>Style parameters for a gizmo label.</summary>
    [Serializable]
    public struct ImGizmoLabelStyle
    {
        /// <summary>RGBA color.</summary>
        public Color color;

        /// <summary>The center point location of the text bounding rectangle.</summary>
        public LabelPivot pivot;

        /// <summary>Text alignment.</summary>
        public LabelAlignment alignment;

        /// <summary>Font style.</summary>
        public FontStyle fontStyle;

        /// <summary>Font size.</summary>
        public int fontSize;

        /// <summary>Maximum draw distance from viewer.</summary>
        public float maxDrawDistance;

        /// <summary>Specify text should have a drop shadow.</summary>
        public bool shadowed;

        /// <summary>Screen space X position offset.</summary>
        public float offsetX;

        /// <summary>Screen space Y position offset.</summary>
        public float offsetY;
    }

    public static class ImGizmos
    {
        private static readonly Vector2 LabelShadowOffset = new(2f, 2f); // This represents the pixel offset for the shadow thats drawn behind text

        private static readonly Vector3 Vector3Zero = new(0f, 0f, 0f);

        private static GUIStyle sStyle;
        private static GUIContent sGUIContent;

        private static Matrix4x4 sPrevGizmosMatrix;
        private static Color sPrevGizmosColor;
        private static Matrix4x4 sMatrixTemp;

        private static int sColorPropID;
        private static int sZTestPropID;
        private static int sInnerRadiusPropID;
        private static int sDirAnglePropID;
        private static int sSectorAnglePropID;


        private static float[] sSine;
        private static float[] sCosine;

        private static void InitLookupTable()
        {
            if (sSine != null) // Skip if already initialised
                return;

            int step = 15;
            int pcount = (360 / step) + 1;
            int index = 0;
            float anglef;

            sSine = new float[pcount];
            sCosine = new float[pcount];

            for (int angle = 0; angle <= 360; angle += step)
            {
                anglef = (float) (angle - 90) * Mathf.Deg2Rad;
                sSine[index] = Mathf.Sin(anglef);
                sCosine[index] = Mathf.Cos(anglef);
                ++index;
            }
        }

        private static void InitGUI()
        {
            if (sStyle == null)
            {
                sStyle = new GUIStyle();
            }

            if (sGUIContent == null)
            {
                sGUIContent = new GUIContent();
            }
        }

        private static void SaveGizmosState()
        {
            sPrevGizmosMatrix = Gizmos.matrix;
            sPrevGizmosColor = Gizmos.color;
        }

        private static void RestoreGizmosState()
        {
            Gizmos.matrix = sPrevGizmosMatrix;
            Gizmos.color = sPrevGizmosColor;
        }

        private static void SetGizmoMatrix(ref Vector3 position, ref Quaternion rotation)
        {
            ImDrawMatrixHelper.SetTR(ref position, ref rotation, ref sMatrixTemp);
            Gizmos.matrix = sMatrixTemp;
        }

        private static void SetGizmoMatrix(ref Vector3 position, ref Quaternion rotation, GizmoDrawAxis axis)
        {
            ImDrawRotationHelper.SetRotationAxis(ref rotation, (int) axis);
            ImDrawMatrixHelper.SetTR(ref position, ref rotation, ref sMatrixTemp);
            Gizmos.matrix = sMatrixTemp;
        }

        private static void SetGizmoMatrix(ref Vector3 position, ref Quaternion rotation, ref Vector3 scale)
        {
            ImDrawMatrixHelper.SetTRS(ref position, ref rotation, ref scale, ref sMatrixTemp);
            Gizmos.matrix = sMatrixTemp;
        }

        private static void SetGizmoMatrix(ref Vector3 position, ref Quaternion rotation, ref Vector3 scale, GizmoDrawAxis axis)
        {
            ImDrawRotationHelper.SetRotationAxis(ref rotation, (int) axis);
            SetGizmoMatrix(ref position, ref rotation, ref scale);
        }


        #region LINES

        /// <summary>
        /// Draw a 3D line.
        /// </summary>
        /// <param name="from">Starting point of the line.</param>
        /// <param name="to">Ending point of the line.</param>
        /// <param name="color">Line color.</param>
        public static void Line3D(Vector3 from, Vector3 to, Color color)
        {
            SaveGizmosState();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
            RestoreGizmosState();
        }

        /// <summary>
        /// Draw a 3D ray.
        /// </summary>
        /// <param name="ray">Source ray.</param>
        /// <param name="length">Length of line.</param>
        /// <param name="color">Ray color.</param>
        public static void Ray3D(Ray ray, float length, Color color)
        {
            SaveGizmosState();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = color;
            Gizmos.DrawLine(ray.origin, ray.origin + (ray.direction * length));
            RestoreGizmosState();
        }

        /// <summary>
        /// Draw a 3D ray.
        /// </summary>
        /// <param name="origin">Origin of ray.</param>
        /// <param name="direction">Direction of ray (assumed to be normalised).</param>
        /// <param name="length">Length of line.</param>
        /// <param name="color">Ray color.</param>
        public static void Ray3D(Vector3 origin, Vector3 direction, float length, Color color)
        {
            SaveGizmosState();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = color;
            Gizmos.DrawLine(origin, origin + direction * length);
            RestoreGizmosState();
        }

        /// <summary>
        /// Draw a 3D ray.
        /// </summary>
        /// <param name="origin">Origin of ray.</param>
        /// <param name="rotation">Ray orientation.</param>
        /// <param name="length">Ray length.</param>
        /// <param name="color">Ray color.</param>
        public static void Ray3D(Vector3 origin, Quaternion rotation, float length, Color color)
        {
            SaveGizmosState();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = color;
            Gizmos.DrawLine(origin, origin + rotation * new Vector3(0f, 0f, length));
            RestoreGizmosState();
        }

        #endregion // LINES


        private static readonly Vector3[] QuadVert = {new(-0.5f, 0f, -0.5f), new(0.5f, 0f, -0.5f), new(0.5f, 0f, 0.5f), new(-0.5f, 0f, 0.5f),};

        private static void DrawWireQuad()
        {
            Gizmos.DrawLine(QuadVert[0], QuadVert[1]);
            Gizmos.DrawLine(QuadVert[1], QuadVert[2]);
            Gizmos.DrawLine(QuadVert[2], QuadVert[3]);
            Gizmos.DrawLine(QuadVert[3], QuadVert[0]);
        }

        /// <summary>
        /// Draw a wireframe 3D quad.
        /// </summary>
        /// <param name="center">Quad center position.</param>
        /// <param name="rotation">Quad rotation.</param>
        /// <param name="sizeX">Width of quad.</param>
        /// <param name="sizeY">Height of quad.</param>
        /// <param name="axis">Orientation axis of quad.</param>
        /// <param name="color">Quad color.</param>
        public static void WireQuad3D(Vector3 center, Quaternion rotation, float sizeX, float sizeY, GizmoDrawAxis axis, Color color)
        {
            SaveGizmosState();
            //IMDrawRotationHelper.SetRotationAxis(ref rotation, (int)axis);
            Vector3 scale = new Vector3(sizeX, 0f, sizeY);
            SetGizmoMatrix(ref center, ref rotation, ref scale, axis);
            Gizmos.color = color;
            DrawWireQuad();
            RestoreGizmosState();
        }


        /// <summary>
        /// Draw a wireframe 3D box.
        /// </summary>
        /// <param name="center">Box center position.</param>
        /// <param name="rotation">Box orientation.</param>
        /// <param name="size">Box extents.</param>
        /// <param name="color">Box color.</param>
        public static void WireBox3D(Vector3 center, Quaternion rotation, Vector3 size, Color color)
        {
            SaveGizmosState();
            SetGizmoMatrix(ref center, ref rotation);
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3Zero, size);
            RestoreGizmosState();
        }

        private static void DrawWireDisc()
        {
            InitLookupTable();

            int index = 0, lastIndex = sSine.Length - 1;

            Vector3 p0, p1;
            p0.y = p1.y = 0f;

            p0.x = sSine[0];
            p0.z = sCosine[0];

            while (index < lastIndex)
            {
                ++index;

                p1.x = sSine[index];
                p1.z = sCosine[index];

                Gizmos.DrawLine(p0, p1);

                p0 = p1;
            }
        }

        /// <summary>
        /// Draw a wireframe 3D disc.
        /// </summary>
        /// <param name="origin">Disc origin.</param>
        /// <param name="rotation">Disc orientation.</param>
        /// <param name="radius">Disc radius.</param>
        /// <param name="axis">Disc reference axis.</param>
        /// <param name="color">Disc color.</param>
        public static void WireDisc3D(Vector3 origin, Quaternion rotation, float radius, GizmoDrawAxis axis, Color color)
        {
            SaveGizmosState();
            Vector3 scale = new Vector3(radius, 0f, radius);
            SetGizmoMatrix(ref origin, ref rotation, ref scale, axis);
            Gizmos.color = color;
            DrawWireDisc();
            RestoreGizmosState();
        }

        /// <summary>
        /// Draw a wireframe 3D sphere.
        /// </summary>
        /// <param name="center">Sphere center.</param>
        /// <param name="rotation">Sphere radius.</param>
        /// <param name="radius">Sphere radius.</param>
        /// <param name="color">Sphere color.</param>
        public static void WireSphere3D(Vector3 center, Quaternion rotation, float radius, Color color)
        {
            SaveGizmosState();
            SetGizmoMatrix(ref center, ref rotation);
            Gizmos.color = color;
            Gizmos.DrawWireSphere(Vector3Zero, radius);
            RestoreGizmosState();
        }


        /// <summary>
        /// Draw a wireframe 3D ellipsoid.
        /// </summary>
        /// <param name="center">Ellipsoid center.</param>
        /// <param name="rotation">Ellipsoid rotation.</param>
        /// <param name="extents">Ellipsoid size.</param>
        /// <param name="color">Ellipsoid color.</param>
        public static void WireEllipsoid3D(Vector3 center, Quaternion rotation, Vector3 extents, Color color)
        {
            SaveGizmosState();
            SetGizmoMatrix(ref center, ref rotation, ref extents);
            Gizmos.color = color;
            Gizmos.DrawWireSphere(Vector3Zero, 0.5f);
            RestoreGizmosState();
        }


        private static readonly Vector3[] ConeVert = {new(0f, 1f, 0f), new(-1f, 0f, 0f), new(1f, 0f, 0f), new(0f, 0f, -1f), new(0f, 0f, 1f),};


        private static void DrawWireCone3D()
        {
            Gizmos.DrawLine(ConeVert[0], ConeVert[1]);
            Gizmos.DrawLine(ConeVert[0], ConeVert[2]);
            Gizmos.DrawLine(ConeVert[0], ConeVert[3]);
            Gizmos.DrawLine(ConeVert[0], ConeVert[4]);

            DrawWireDisc();
        }

        /// <summary>
        /// Draw a wireframe 3D cone.
        /// </summary>
        /// <param name="position">Cone position (origin is located at the base).</param>
        /// <param name="rotation">Cone rotation.</param>
        /// <param name="height">Cone height.</param>
        /// <param name="width">Cone base width.</param>
        /// <param name="axis">Cone reference axis.</param>
        /// <param name="color">Cone color.</param>
        public static void WireCone3D(Vector3 position, Quaternion rotation, float height, float width, GizmoDrawAxis axis, Color color)
        {
            SaveGizmosState();
            Vector3 scale = new Vector3(width * 0.5f, height, width * 0.5f);
            SetGizmoMatrix(ref position, ref rotation, ref scale, axis);
            Gizmos.color = color;
            DrawWireCone3D();
            RestoreGizmosState();
        }

        private static void DrawWireCapsule3D(float height, float radius)
        {
            InitLookupTable();

            height = height * 0.5f - radius;

            // Draw sides
            Gizmos.DrawLine(new Vector3(-radius, height, 0f), new Vector3(-radius, -height, 0f));
            Gizmos.DrawLine(new Vector3(radius, height, 0f), new Vector3(radius, -height, 0f));
            Gizmos.DrawLine(new Vector3(0f, height, -radius), new Vector3(0f, -height, -radius));
            Gizmos.DrawLine(new Vector3(0f, height, radius), new Vector3(0f, -height, radius));

            // Draw end discs
            int index = 0, lastIndex = sSine.Length - 1;

            Vector3 p0, p1;
            p0.y = p1.y = 0f;

            p0.x = sSine[0] * radius;
            p0.z = sCosine[0] * radius;

            while (index < lastIndex)
            {
                ++index;

                p1.x = sSine[index] * radius;
                p1.z = sCosine[index] * radius;

                p0.y = p1.y = -height;

                Gizmos.DrawLine(p0, p1);

                p0.y = p1.y = height;

                Gizmos.DrawLine(p0, p1);

                p0 = p1;
            }

            // Draw end caps

            index = 0;
            lastIndex = sSine.Length / 2;

            float rx, ry, sx, sy;

            while (index < lastIndex)
            {
                rx = sSine[index] * radius;
                ry = sCosine[index] * radius;

                ++index;

                sx = sSine[index] * radius;
                sy = sCosine[index] * radius;

                p0.x = 0.0f;
                p0.y = ry + height;
                p0.z = rx;
                p1.x = 0.0f;
                p1.y = sy + height;
                p1.z = sx;

                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(-p0, -p1);

                p0.x = rx;
                p0.y = ry + height;
                p0.z = 0.0f;
                p1.x = sx;
                p1.y = sy + height;
                p1.z = 0.0f;

                Gizmos.DrawLine(p0, p1);
                Gizmos.DrawLine(-p0, -p1);
            }
        }

        /// <summary>
        /// Draw a wireframe 3D capsule.
        /// </summary>
        /// <param name="center">Capsule center.</param>
        /// <param name="rotation">Capsule rotation.</param>
        /// <param name="height">Capsule height.</param>
        /// <param name="radius">Capsule radius.</param>
        /// <param name="axis">Capsule reference axis.</param>
        /// <param name="color">Capsule color.</param>
        public static void WireCapsule3D(Vector3 center, Quaternion rotation, float height, float radius, GizmoDrawAxis axis, Color color)
        {
            if (height <= (radius * 2f)) // If the diameter is equal to or less than the height, then we revert to a sphere
            {
                WireSphere3D(center, rotation, radius, color);
                return;
            }

            SaveGizmosState();

            SetGizmoMatrix(ref center, ref rotation, axis);

            Gizmos.color = color;
            DrawWireCapsule3D(height, radius);

            RestoreGizmosState();
        }

        private static void DrawWireCylinder3D()
        {
            InitLookupTable();

            Gizmos.DrawLine(new Vector3(-1f, 0.5f, 0f), new Vector3(-1f, -0.5f, 0f));
            Gizmos.DrawLine(new Vector3(1f, 0.5f, 0f), new Vector3(1f, -0.5f, 0f));
            Gizmos.DrawLine(new Vector3(0f, 0.5f, -1f), new Vector3(0f, -0.5f, -1f));
            Gizmos.DrawLine(new Vector3(0f, 0.5f, 1f), new Vector3(0f, -0.5f, 1f));

            int index = 0, lastIndex = sSine.Length - 1;

            Vector3 p0, p1;
            p0.y = p1.y = 0f;

            p0.x = sSine[0];
            p0.z = sCosine[0];

            while (index < lastIndex)
            {
                ++index;

                p1.x = sSine[index];
                p1.z = sCosine[index];

                p0.y = p1.y = -0.5f;

                Gizmos.DrawLine(p0, p1);

                p0.y = p1.y = 0.5f;

                Gizmos.DrawLine(p0, p1);

                p0 = p1;
            }
        }

        /// <summary>
        /// Draw a wireframe 3D cylinder.
        /// </summary>
        /// <param name="center">Cylinder center.</param>
        /// <param name="rotation">Cylinder rotation.</param>
        /// <param name="height">Cylinder height.</param>
        /// <param name="radius">Cylinder radius.</param>
        /// <param name="axis">Cylinder reference axis.</param>
        /// <param name="color">Cylinder color.</param>
        public static void WireCylinder3D(Vector3 center, Quaternion rotation, float height, float radius, GizmoDrawAxis axis, Color color)
        {
            SaveGizmosState();
            Vector3 scale = new Vector3(radius, height, radius);
            SetGizmoMatrix(ref center, ref rotation, ref scale, axis);
            Gizmos.color = color;
            DrawWireCylinder3D();
            RestoreGizmosState();
        }


        #region SPECIAL PRIMITIVES

        /// <summary>
        /// Draw 3D bounds (an axis aligned box).
        /// </summary>
        /// <param name="bounds">Bounds that specifies a position and extents.</param>
        /// <param name="color">Bounds color.</param>
        public static void Bounds(Bounds bounds, Color color)
        {
            SaveGizmosState();
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = color;
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2f);
            RestoreGizmosState();
        }

        /// <summary>
        /// Draw the 3D bounds for a renderer (note: only draws bounds if renderer is visible).
        /// </summary>
        /// <param name="renderer">Renderer whose bounds will be drawn.</param>
        /// <param name="color">Bounds color.</param>
        public static void Bounds(Renderer renderer, Color color)
        {
            if (renderer != null && renderer.isVisible)
            {
                SaveGizmosState();
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = color;
                Bounds bounds = renderer.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.extents * 2f);
                RestoreGizmosState();
            }
        }

        private static void GetBoxColliderTransform(BoxCollider collider, out Vector3 pos, out Vector3 scale)
        {
            Transform tf = collider.transform;

            pos = tf.TransformPoint(collider.center);

            Vector3 boxScale = collider.size;

            scale = tf.lossyScale;

            scale.x *= boxScale.x;
            scale.y *= boxScale.y;
            scale.z *= boxScale.z;
        }

        private static void GetSphereColliderTransform(SphereCollider collider, out Vector3 pos, out float radius)
        {
            Transform tf = collider.transform;

            pos = tf.TransformPoint(collider.center);

            Vector3 transformScale = tf.lossyScale;

            // Note: sphere collider seems to use absolute value of largest component to scale the radius
            if (transformScale.x < 0f) transformScale.x = -transformScale.x;
            if (transformScale.y < 0f) transformScale.y = -transformScale.y;
            if (transformScale.z < 0f) transformScale.z = -transformScale.z;

            radius = transformScale.x;

            if (transformScale.y > radius)
                radius = transformScale.y;

            if (transformScale.z > radius)
                radius = transformScale.z;

            radius *= collider.radius;
        }

        private static void GetCapsuleColliderTransform(CapsuleCollider collider, out Vector3 pos, out float height, out float radius)
        {
            Transform tf = collider.transform;
            pos = tf.TransformPoint(collider.center);

            Vector3 transformScale = tf.lossyScale;
            if (transformScale.x < 0f) transformScale.x = -transformScale.x;
            if (transformScale.y < 0f) transformScale.y = -transformScale.y;
            if (transformScale.z < 0f) transformScale.z = -transformScale.z;

            height = collider.height;
            radius = collider.radius;

            switch (collider.direction)
            {
                case 0: // X-AXIS
                    height *= transformScale.x;
                    radius *= transformScale.y > transformScale.z ? transformScale.y : transformScale.z;
                    break;

                case 1: // Y-AXIS
                    height *= transformScale.y;
                    radius *= transformScale.x > transformScale.z ? transformScale.x : transformScale.z;
                    break;

                case 2: // Z-AXIS
                    height *= transformScale.z;
                    radius *= transformScale.x > transformScale.y ? transformScale.x : transformScale.y;
                    break;
            }
        }

        /// <summary>
        /// Draw a collider as a wireframe shape. Note: MeshCollider not currently supported.
        /// </summary>
        /// <param name="collider">Collider object.</param>
        /// <param name="wireColor">Wireframe color.</param>
        public static void Collider(Collider collider, Color wireColor)
        {
            if (collider == null)
                return;

            Type type = collider.GetType();

            Transform tf = collider.transform;
            Vector3 pos;

            if (type == typeof(BoxCollider))
            {
                Vector3 scale;
                GetBoxColliderTransform(collider as BoxCollider, out pos, out scale);
                WireBox3D(pos, tf.rotation, scale, wireColor);
            }
            else if (type == typeof(SphereCollider))
            {
                GetSphereColliderTransform(collider as SphereCollider, out pos, out float radius);
                WireSphere3D(pos, tf.rotation, radius, wireColor);
            }
            else if (type == typeof(CapsuleCollider))
            {
                CapsuleCollider capsule = collider as CapsuleCollider;
                float height, radius;
                GetCapsuleColliderTransform(capsule, out pos, out height, out radius);
                WireCapsule3D(pos,
                    tf.rotation,
                    height,
                    radius,
                    (GizmoDrawAxis) capsule.direction,
                    wireColor);
            }
            else if (type == typeof(MeshCollider))
            {
                // Not implemented yet
            }
        }

        #endregion // SPECIAL PRIMITIVES

        #region LABELS

        // Note: changed y offset direction since screen Y is flipped before rather than after.
        private static void SetPivot(ref float x, ref float y, Vector2 size, LabelPivot pivot)
        {
            switch (pivot)
            {
                case LabelPivot.LowerCenter:
                    x -= size.x * 0.5f;
                    y -= size.y;
                    break;
                case LabelPivot.LowerLeft: y -= size.y; break;
                case LabelPivot.LowerRight:
                    x -= size.x;
                    y -= size.y;
                    break;

                case LabelPivot.MiddleCenter:
                    x -= size.x * 0.5f;
                    y -= size.y * 0.5f;
                    break;
                case LabelPivot.MiddleLeft: y -= size.y * 0.5f; break;
                case LabelPivot.MiddleRight:
                    x -= size.x;
                    y -= size.y * 0.5f;
                    break;

                case LabelPivot.UpperCenter: x -= size.x * 0.5f; break;
                //case TextAnchor.UpperLeft: break;
                case LabelPivot.UpperRight: x -= size.x; break;
            }
        }

        private static void SetAlignment(LabelAlignment alignment)
        {
            switch (alignment)
            {
                case LabelAlignment.Left: sStyle.alignment = TextAnchor.UpperLeft; break;
                case LabelAlignment.Center: sStyle.alignment = TextAnchor.UpperCenter; break;
                case LabelAlignment.Right: sStyle.alignment = TextAnchor.UpperRight; break;
            }
        }

        private static Camera GetEditorSceneCamera()
        {
            return SceneView.currentDrawingSceneView != null
                ? // Note: this can be null in rare situations
                SceneView.currentDrawingSceneView.camera
                : null;
        }

        private static float GetPixelsPerPoint() { return EditorGUIUtility.pixelsPerPoint; }

        // Correctly calculate screen position from editor scene view world position taking into account editor UI scaling
        private static Vector3 WorldToScreenPoint(Camera cam, Vector3 position)
        {
            float pixelsPerPoint = GetPixelsPerPoint();
            Vector3
                screenPoint = cam
                    .WorldToViewportPoint(position); // Note: we don't use WorldToSceenPoint because it is broken and doesn't take into account pixels per point
            screenPoint.x *= cam.pixelWidth / pixelsPerPoint;
            screenPoint.y = (cam.pixelHeight / pixelsPerPoint) * (1f - screenPoint.y);
            //screenPoint.y -= 15f; // There seems to be some weird Y offset when using WorldToViewportPoint, so correct it
            return screenPoint;
        }

        // Fix for Unity issue where GUI elements are offset in Y-axis proportional to font size
        private static void DoLabelPositionCorrection(int fontSize, ref float screenY)
        {
            screenY -= (float) fontSize * 0.09f; // This line can be commented out to avoid correction
        }

        /// <summary>
        /// Draw a label in screen space.
        /// </summary>
        /// <param name="x">Screen X position.</param>
        /// <param name="y">Screen Y position.</param>
        /// <param name="color">Label color.</param>
        /// <param name="pivot">Label rectangle pivot.</param>
        /// <param name="alignment">Label text alignment.</param>
        /// <param name="label">Label text.</param>
        /// <param name="fontSize">Label font size.</param>
        public static void Label(float x, float y, Color color, LabelPivot pivot, LabelAlignment alignment, string label, int fontSize = 12)
        {
            InitGUI();

            sStyle.fontSize = fontSize;
            sStyle.normal.textColor = color;
            sStyle.alignment = TextAnchor.UpperLeft;
            SetAlignment(alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            if (x < -size.x || y < -size.y)
                return;

            Camera cam = GetEditorSceneCamera();

            if (cam == null)
                return;

            if (x > cam.pixelWidth || y > cam.pixelHeight)
                return;

            DoLabelPositionCorrection(fontSize, ref y);

            float invPixelsPerPoint = 1f / GetPixelsPerPoint();
            x *= invPixelsPerPoint;
            y *= invPixelsPerPoint;

            SetPivot(ref x, ref y, size, pivot); // Needs to be done after screen position has been scaled

            Handles.BeginGUI();
            GUI.Label(new Rect(x, y, size.x, size.y), sGUIContent, sStyle);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a label (with drop shadow) in screen space.
        /// </summary>
        /// <param name="x">Screen X position.</param>
        /// <param name="y">Screen Y position.</param>
        /// <param name="color">Label color.</param>
        /// <param name="pivot">Label rectangle pivot.</param>
        /// <param name="alignment">Label text alignment.</param>
        /// <param name="label">Label text.</param>
        /// <param name="fontSize">Label font size.</param>
        public static void LabelShadowed(float x, float y, Color color, LabelPivot pivot, LabelAlignment alignment, string label, int fontSize = 12)
        {
            InitGUI();

            sStyle.fontSize = fontSize;
            sStyle.alignment = TextAnchor.UpperLeft;
            SetAlignment(alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            if (x < -size.x || y < -size.y)
                return;

            Camera cam = GetEditorSceneCamera();

            if (cam == null)
                return;

            if (x > cam.pixelWidth || y > cam.pixelHeight)
                return;

            DoLabelPositionCorrection(fontSize, ref y);

            float invPixelsPerPoint = 1f / GetPixelsPerPoint();
            x *= invPixelsPerPoint;
            y *= invPixelsPerPoint;

            SetPivot(ref x, ref y, size, pivot); // Needs to be done after screen position has been scaled

            Handles.BeginGUI();
            sStyle.normal.textColor = new Color(0f, 0f, 0f, color.a);
            GUI.Label(new Rect(x + LabelShadowOffset.x, y + LabelShadowOffset.y, size.x, size.y), sGUIContent, sStyle); // Shadow label

            sStyle.normal.textColor = color;
            GUI.Label(new Rect(x, y, size.x, size.y), sGUIContent, sStyle); // Main label
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a label in 3D space.
        /// </summary>
        /// <param name="position">Label 3D position.</param>
        /// <param name="color">Label color.</param>
        /// <param name="pivot">Label rectangle pivot.</param>
        /// <param name="alignment">Label text alignment.</param>
        /// <param name="label">Label text.</param>
        /// <param name="maxDist">Max draw distance from viewer.</param>
        /// <param name="fontSize">Label font size.</param>
        public static void Label(Vector3 position, Color color, LabelPivot pivot, LabelAlignment alignment, string label, float maxDist, int fontSize = 12)
        {
            InitGUI();

            Camera cam = GetEditorSceneCamera();

            if (cam == null)
                return;

            Vector3 screenPos = WorldToScreenPoint(cam, position);

            if (screenPos.z < 0f || screenPos.z > maxDist)
                return;

            sStyle.fontSize = fontSize;
            sStyle.normal.textColor = color;
            SetAlignment(alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            DoLabelPositionCorrection(fontSize, ref screenPos.y);

            SetPivot(ref screenPos.x, ref screenPos.y, size, pivot);

            Handles.BeginGUI();
            GUI.Label(new Rect((float) Math.Round(screenPos.x), (float) Math.Round(screenPos.y), size.x, size.y), sGUIContent, sStyle);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a label (with drop shadow) in 3D space.
        /// </summary>
        /// <param name="position">Label 3D position.</param>
        /// <param name="color">Label color.</param>
        /// <param name="pivot">Label rectangle pivot.</param>
        /// <param name="alignment">Label text alignment.</param>
        /// <param name="label">Label text.</param>
        /// <param name="maxDist">Max draw distance from viewer.</param>
        /// <param name="fontSize">Label font size.</param>
        public static void LabelShadowed(Vector3 position, Color color, LabelPivot pivot, LabelAlignment alignment, string label, float maxDist, int fontSize = 12)
        {
            InitGUI();

            Camera cam = GetEditorSceneCamera();

            if (cam == null)
                return;

            Vector3 screenPos = WorldToScreenPoint(cam, position);

            if (screenPos.z < 0f || screenPos.z > maxDist)
                return;

            sStyle.fontSize = fontSize;
            SetAlignment(alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            DoLabelPositionCorrection(fontSize, ref screenPos.y);

            SetPivot(ref screenPos.x, ref screenPos.y, size, pivot);
            screenPos.x = (float) Math.Round(screenPos.x);
            screenPos.y = (float) Math.Round(screenPos.y);

            Handles.BeginGUI();
            sStyle.normal.textColor = new Color(0f, 0f, 0f, color.a);
            GUI.Label(new Rect(screenPos.x + LabelShadowOffset.x, screenPos.y + LabelShadowOffset.y, size.x, size.y), sGUIContent, sStyle);

            sStyle.normal.textColor = color;
            GUI.Label(new Rect(screenPos.x, screenPos.y, size.x, size.y), sGUIContent, sStyle);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a label in 3D space.
        /// </summary>
        /// <param name="position">Label 3D position.</param>
        /// <param name="offsetX">Screen space X position offset.</param>
        /// <param name="offsetY">Screen space Y position offset.</param>
        /// <param name="color">Label color.</param>
        /// <param name="pivot">Label rectangle pivot.</param>
        /// <param name="alignment">Label text alignment.</param>
        /// <param name="label">Label text.</param>
        /// <param name="maxDist">Max draw distance from viewer.</param>
        /// <param name="fontSize">Label font size.</param>
        public static void Label(
            Vector3 position,
            float offsetX,
            float offsetY,
            Color color,
            LabelPivot pivot,
            LabelAlignment alignment,
            string label,
            float maxDist,
            int fontSize = 12)
        {
            InitGUI();

            Camera cam = GetEditorSceneCamera();

            if (cam == null) return;

            Vector3 screenPos = WorldToScreenPoint(cam, position);

            if (screenPos.z < 0f || screenPos.z > maxDist) return;

            sStyle.fontSize = fontSize;
            sStyle.normal.textColor = color;
            SetAlignment(alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            DoLabelPositionCorrection(fontSize, ref screenPos.y);

            SetPivot(ref screenPos.x, ref screenPos.y, size, pivot);

            Handles.BeginGUI();
            GUI.Label(new Rect((float) Math.Round(screenPos.x + offsetX), (float) Math.Round(screenPos.y - offsetY), size.x, size.y), sGUIContent, sStyle);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a label (with drop shadow) in 3D space.
        /// </summary>
        /// <param name="position">Label 3D position.</param>
        /// <param name="offsetX">Screen space X position offset.</param>
        /// <param name="offsetY">Screen space Y position offset.</param>
        /// <param name="color">Label color.</param>
        /// <param name="pivot">Label rectangle pivot.</param>
        /// <param name="alignment">Label text alignment.</param>
        /// <param name="label">Label text.</param>
        /// <param name="maxDist">Max draw distance from viewer.</param>
        /// <param name="fontSize">Label font size.</param>
        public static void LabelShadowed(
            Vector3 position,
            float offsetX,
            float offsetY,
            Color color,
            LabelPivot pivot,
            LabelAlignment alignment,
            string label,
            float maxDist,
            int fontSize = 12)
        {
            InitGUI();

            Camera cam = GetEditorSceneCamera();

            if (cam == null)
                return;

            Vector3 screenPos = WorldToScreenPoint(cam, position);

            if (screenPos.z < 0f || screenPos.z > maxDist)
                return;

            sStyle.fontSize = fontSize;
            sStyle.normal.textColor = color;
            SetAlignment(alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            DoLabelPositionCorrection(fontSize, ref screenPos.y);

            SetPivot(ref screenPos.x, ref screenPos.y, size, pivot);
            screenPos.x = (float) Math.Round(screenPos.x + offsetX);
            screenPos.y = (float) Math.Round(screenPos.y - offsetY);

            Handles.BeginGUI();
            sStyle.normal.textColor = new Color(0f, 0f, 0f, color.a);
            GUI.Label(new Rect(screenPos.x + LabelShadowOffset.x, screenPos.y + LabelShadowOffset.y, size.x, size.y), sGUIContent, sStyle);

            sStyle.normal.textColor = color;
            GUI.Label(new Rect(screenPos.x, screenPos.y, size.x, size.y), sGUIContent, sStyle);
            Handles.EndGUI();
        }

        /// <summary>
        /// Draw a label in 3D space.
        /// </summary>
        /// <param name="position">Label 3D position.</param>
        /// <param name="style">Style parameters for how the text should be displayed.</param>
        /// <param name="label">Label text.</param>
        public static void Label(Vector3 position, ImGizmoLabelStyle style, string label) { Label(position, ref style, label); }

        /// <summary>
        /// Draw a label in 3D space.
        /// </summary>
        /// <param name="position">Label 3D position.</param>
        /// <param name="style">Style parameters for how the text should be displayed.</param>
        /// <param name="label">Label text.</param>
        public static void Label(Vector3 position, ref ImGizmoLabelStyle style, string label)
        {
            InitGUI();

            Camera cam = GetEditorSceneCamera();

            if (cam == null)
                return;

            Vector3 screenPos = WorldToScreenPoint(cam, position);

            if (screenPos.z < 0f || screenPos.z > style.maxDrawDistance)
                return;

            sStyle.fontStyle = style.fontStyle;
            sStyle.fontSize = style.fontSize;
            sStyle.normal.textColor = style.color;
            SetAlignment(style.alignment);

            sGUIContent.text = label;
            Vector3 size = sStyle.CalcSize(sGUIContent);

            DoLabelPositionCorrection(style.fontSize, ref screenPos.y);

            SetPivot(ref screenPos.x, ref screenPos.y, size, style.pivot);
            screenPos.x = (float) Math.Round(screenPos.x + style.offsetX);
            screenPos.y = (float) Math.Round(screenPos.y - style.offsetY);

            Handles.BeginGUI();
            if (style.shadowed)
            {
                sStyle.normal.textColor = new Color(0f, 0f, 0f, style.color.a);
                GUI.Label(new Rect(screenPos.x + LabelShadowOffset.x, screenPos.y + LabelShadowOffset.y, size.x, size.y), sGUIContent, sStyle);
            }

            sStyle.normal.textColor = style.color;
            GUI.Label(new Rect(screenPos.x, screenPos.y, size.x, size.y), sGUIContent, sStyle);
            Handles.EndGUI();

            sStyle.fontStyle = FontStyle.Normal;
        }

        #endregion // LABELS
    }

    public static class ImDrawMatrixHelper
    {
        // Note: There is an issue with Matrix4x4.SetTRS where it can spew "Quaternion To Matrix conversion failed because input Quaternion is invalid" errors if the rotation isn't extremely close to normalised.
        // This alternative to Matrix4x4.TRS produces near identical results and performs much faster in both editor and standalone builds.

        /// <summary>Initialise a matrix with translation, rotation and uniform scale.</summary>
        public static void SetTRS(ref Vector3 position, ref Quaternion rotation, float uniformScale, ref Matrix4x4 matrix)
        {
            matrix.m00 = (1.0f - 2.0f * (rotation.y * rotation.y + rotation.z * rotation.z)) * uniformScale;
            matrix.m10 = (rotation.x * rotation.y + rotation.z * rotation.w) * uniformScale * 2.0f;
            matrix.m20 = (rotation.x * rotation.z - rotation.y * rotation.w) * uniformScale * 2.0f;
            matrix.m30 = 0.0f;
            matrix.m01 = (rotation.x * rotation.y - rotation.z * rotation.w) * uniformScale * 2.0f;
            matrix.m11 = (1.0f - 2.0f * (rotation.x * rotation.x + rotation.z * rotation.z)) * uniformScale;
            matrix.m21 = (rotation.y * rotation.z + rotation.x * rotation.w) * uniformScale * 2.0f;
            matrix.m31 = 0.0f;
            matrix.m02 = (rotation.x * rotation.z + rotation.y * rotation.w) * uniformScale * 2.0f;
            matrix.m12 = (rotation.y * rotation.z - rotation.x * rotation.w) * uniformScale * 2.0f;
            matrix.m22 = (1.0f - 2.0f * (rotation.x * rotation.x + rotation.y * rotation.y)) * uniformScale;
            matrix.m32 = 0.0f;
            matrix.m03 = position.x;
            matrix.m13 = position.y;
            matrix.m23 = position.z;
            matrix.m33 = 1.0f;
        }

        /// <summary>Initialise a matrix with translation, rotation and scale.</summary>
        public static void SetTRS(ref Vector3 position, ref Quaternion rotation, ref Vector3 scale, ref Matrix4x4 matrix)
        {
            matrix.m00 = (1.0f - 2.0f * (rotation.y * rotation.y + rotation.z * rotation.z)) * scale.x;
            matrix.m10 = (rotation.x * rotation.y + rotation.z * rotation.w) * scale.x * 2.0f;
            matrix.m20 = (rotation.x * rotation.z - rotation.y * rotation.w) * scale.x * 2.0f;
            matrix.m30 = 0.0f;
            matrix.m01 = (rotation.x * rotation.y - rotation.z * rotation.w) * scale.y * 2.0f;
            matrix.m11 = (1.0f - 2.0f * (rotation.x * rotation.x + rotation.z * rotation.z)) * scale.y;
            matrix.m21 = (rotation.y * rotation.z + rotation.x * rotation.w) * scale.y * 2.0f;
            matrix.m31 = 0.0f;
            matrix.m02 = (rotation.x * rotation.z + rotation.y * rotation.w) * scale.z * 2.0f;
            matrix.m12 = (rotation.y * rotation.z - rotation.x * rotation.w) * scale.z * 2.0f;
            matrix.m22 = (1.0f - 2.0f * (rotation.x * rotation.x + rotation.y * rotation.y)) * scale.z;
            matrix.m32 = 0.0f;
            matrix.m03 = position.x;
            matrix.m13 = position.y;
            matrix.m23 = position.z;
            matrix.m33 = 1.0f;
        }

        /// <summary>Initialise a matrix with translation and rotation.</summary>
        public static void SetTR(ref Vector3 position, ref Quaternion rotation, ref Matrix4x4 matrix)
        {
            matrix.m00 = (1.0f - 2.0f * (rotation.y * rotation.y + rotation.z * rotation.z));
            matrix.m10 = (rotation.x * rotation.y + rotation.z * rotation.w) * 2.0f;
            matrix.m20 = (rotation.x * rotation.z - rotation.y * rotation.w) * 2.0f;
            matrix.m30 = 0.0f;
            matrix.m01 = (rotation.x * rotation.y - rotation.z * rotation.w) * 2.0f;
            matrix.m11 = (1.0f - 2.0f * (rotation.x * rotation.x + rotation.z * rotation.z));
            matrix.m21 = (rotation.y * rotation.z + rotation.x * rotation.w) * 2.0f;
            matrix.m31 = 0.0f;
            matrix.m02 = (rotation.x * rotation.z + rotation.y * rotation.w) * 2.0f;
            matrix.m12 = (rotation.y * rotation.z - rotation.x * rotation.w) * 2.0f;
            matrix.m22 = (1.0f - 2.0f * (rotation.x * rotation.x + rotation.y * rotation.y));
            matrix.m32 = 0.0f;
            matrix.m03 = position.x;
            matrix.m13 = position.y;
            matrix.m23 = position.z;
            matrix.m33 = 1.0f;
        }

        /// <summary>Initialise a matrix with translation and uniform scale.</summary>
        public static void SetTs(ref Vector3 position, float uniformScale, ref Matrix4x4 matrix)
        {
            matrix.m00 = uniformScale;
            matrix.m10 = 0f;
            matrix.m20 = 0f;
            matrix.m30 = 0f;
            matrix.m01 = 0f;
            matrix.m11 = uniformScale;
            matrix.m21 = 0f;
            matrix.m31 = 0f;
            matrix.m02 = 0f;
            matrix.m12 = 0f;
            matrix.m22 = uniformScale;
            matrix.m32 = 0f;
            matrix.m03 = position.x;
            matrix.m13 = position.y;
            matrix.m23 = position.z;
            matrix.m33 = 1f;
        }
    }

    public static class ImDrawRotationHelper
    {
        private static readonly Quaternion AxisXRotation = Quaternion.Euler(90f, 90f, 0f); //Quaternion.Euler(-90f, -90f, 0f); // Note: Old version was wrong?
        private static readonly Quaternion AxisZRotation = Quaternion.Euler(90f, 0f, 0f);

        public static void SetRotationAxis(out Quaternion outRotation, ref Quaternion inRotation, int axis)
        {
            switch (axis)
            {
                case 0: outRotation = inRotation * AxisXRotation; break;
                //case 1: outRotation = inRotation; break;
                case 2: outRotation = inRotation * AxisZRotation; break;
                default: outRotation = inRotation; break;
            }
        }

        public static void SetRotationAxis(ref Quaternion rotation, int axis)
        {
            switch (axis)
            {
                case 0: rotation = rotation * AxisXRotation; break;
                case 2: rotation = rotation * AxisZRotation; break;
            }
        }

        /// <summary>Normalize a quaternion but do not check if it is 0,0,0,0.</summary>
        public static void NormalizeUnchecked(ref Quaternion q)
        {
            float k = (float) (1.0 / Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w));
            q.x *= k;
            q.y *= k;
            q.z *= k;
            q.w *= k;
        }
    }

    public enum LabelPivot
    {
        UpperLeft,
        UpperCenter,
        UpperRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        LowerLeft,
        LowerCenter,
        LowerRight,
    }

    public enum LabelAlignment
    {
        Left,
        Center,
        Right,
    }

#endif
}