using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;

namespace Pancake.Common
{
    [AddComponentMenu(""), ExecuteInEditMode, DefaultExecutionOrder(int.MaxValue)]
    public class DebugDraw : MonoBehaviour
    {
        private const int CAPACITY = 5000;
        private const int TEXT_CAPACITY = 100;
        private const int DIVISIONS = 64;
        private const float TRANSPARENCY = 0.7f;
        private const float LINE_GAP_SIZE = 0.05f;
        private const float OCCLUDED_COLOR = 0.1f;
        private const float POINT_SIZE = 0.5f;
        internal const float ARROW_WIDTH = 0.5f;
        private const float RAY_LENGTH = 1000f;
        private const float DIAMOND_SIZE = 0.5f;
        internal const float ARROW_TIP_SIZE = 0.25f;
        internal const float HIT_RADIUS = 0.1f;
        internal const float HIT_LENGTH = 0.25f;
        internal static readonly Color HitColor = new(1f, 0.94f, 0.54f);
        private static readonly Color LineColor = new(0.45f, 0.45f, 0.45f);
        private static readonly Color ArrowColor = new(0.98f, 0.45f, 0.09f);
        private static readonly Color RayColor = new(0.96f, 0.62f, 0.04f);
        private static readonly Color CircleColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color CubeColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color SphereColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color ArcColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color DiamondColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color ConeColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color BoundsColor = new(0.39f, 0.45f, 0.55f);
        private static readonly Color AxisXColor = Color.red;

        private static DebugDraw instance;

        private static DebugDraw Instance
        {
            get
            {
#if UNITY_EDITOR
                if (instance == null)
                {
                    instance = FindAnyObjectByType<DebugDraw>();
                    if (instance == null)
                    {
                        var go = new GameObject("DebugDraw") {hideFlags = HideFlags.HideAndDontSave};
                        instance = go.AddComponent<DebugDraw>();
                    }
                }
#endif
                return instance;
            }
        }

#if UNITY_EDITOR
        private void Awake()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening += OnSceneOpening;

            if (GraphicsSettings.defaultRenderPipeline == null) Camera.onPostRender += OnDebugRender;
            else RenderPipelineManager.endCameraRendering += OnRendered;
        }


        private void OnRendered(ScriptableRenderContext context, Camera cam) { OnDebugRender(cam); }

        private void OnSceneOpening(string path, UnityEditor.SceneManagement.OpenSceneMode mode) { Clear(); }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange obj) { Clear(); }

        private void OnDestroy()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpening -= OnSceneOpening;

            if (GraphicsSettings.defaultRenderPipeline == null) Camera.onPostRender -= OnDebugRender;
            else RenderPipelineManager.endCameraRendering -= OnRendered;
        }
#endif

        #region primitives

        private static List<Vector3> sphereWire;
        private static List<Vector3> cubeWire;

        private static void CreateSphereWireMesh()
        {
            sphereWire = new List<Vector3>();

            const float step = Math.TWO_PI / DIVISIONS;
            for (var theta = 0.0f; theta < Math.TWO_PI; theta += step)
            {
                float cos0 = Mathf.Cos(theta);
                float cos1 = Mathf.Cos(theta + step);
                float sin0 = Mathf.Sin(theta);
                float sin1 = Mathf.Sin(theta + step);

                sphereWire.Add(0.5f * new Vector3(0.0f, cos0, -sin0));
                sphereWire.Add(0.5f * new Vector3(0.0f, cos1, -sin1));

                sphereWire.Add(0.5f * new Vector3(cos0, 0.0f, -sin0));
                sphereWire.Add(0.5f * new Vector3(cos1, 0.0f, -sin1));

                sphereWire.Add(0.5f * new Vector3(cos0, -sin0, 0.0f));
                sphereWire.Add(0.5f * new Vector3(cos1, -sin1, 0.0f));
            }
        }

        private static void CreateCubeWireMesh()
        {
            cubeWire = new List<Vector3>();

            const float s = 1.0f * 0.5f;
            cubeWire.Add(new Vector3(-s, -s, -s));
            cubeWire.Add(new Vector3(-s, -s, +s));
            cubeWire.Add(new Vector3(-s, -s, +s));
            cubeWire.Add(new Vector3(+s, -s, +s));
            cubeWire.Add(new Vector3(+s, -s, +s));
            cubeWire.Add(new Vector3(+s, -s, -s));
            cubeWire.Add(new Vector3(+s, -s, -s));
            cubeWire.Add(new Vector3(-s, -s, -s));

            cubeWire.Add(new Vector3(-s, +s, -s));
            cubeWire.Add(new Vector3(-s, +s, +s));
            cubeWire.Add(new Vector3(-s, +s, +s));
            cubeWire.Add(new Vector3(+s, +s, +s));
            cubeWire.Add(new Vector3(+s, +s, +s));
            cubeWire.Add(new Vector3(+s, +s, -s));
            cubeWire.Add(new Vector3(+s, +s, -s));
            cubeWire.Add(new Vector3(-s, +s, -s));

            cubeWire.Add(new Vector3(+s, +s, +s));
            cubeWire.Add(new Vector3(+s, -s, +s));
            cubeWire.Add(new Vector3(+s, +s, -s));
            cubeWire.Add(new Vector3(+s, -s, -s));

            cubeWire.Add(new Vector3(-s, +s, +s));
            cubeWire.Add(new Vector3(-s, -s, +s));
            cubeWire.Add(new Vector3(-s, +s, -s));
            cubeWire.Add(new Vector3(-s, -s, -s));
        }

        #endregion

        #region gl

#if UNITY_EDITOR
        private int _currentProgram = -1;

        private void DrawLineGL(Vector3 a, Vector3 b, Color color)
        {
            SetProgramGL(GL.LINES);
            GL.Color(color.ChangeAlpha(TRANSPARENCY * _occludedTransparency));

            GL.Vertex(a);
            GL.Vertex(b);
        }

        private void DrawLineGLDotted(Vector3 a, Vector3 b, Color color)
        {
            SetProgramGL(GL.LINES);
            GL.Color(color.ChangeAlpha(TRANSPARENCY * _occludedTransparency));

            float dashSize = LINE_GAP_SIZE * UnityEditor.EditorGUIUtility.pixelsPerPoint;

            float length = Vector3.Distance(a, b);
            int count = Mathf.CeilToInt(length / dashSize);
            for (var i = 0; i < count; i += 2)
            {
                GL.Vertex(Vector3.Lerp(a, b, i * dashSize / length));
                GL.Vertex(Vector3.Lerp(a, b, (i + 1) * dashSize / length));
            }
        }

        private void DrawTriangleGL(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            SetProgramGL(GL.LINES);
            GL.Color(color.ChangeAlpha(TRANSPARENCY * _occludedTransparency));

            DrawLineGL(a, b, color);
            DrawLineGL(b, c, color);
            DrawLineGL(c, a, color);
        }

        private void DrawTriangleGLSolid(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            SetProgramGL(GL.TRIANGLES);
            GL.Color(color.ChangeAlpha(TRANSPARENCY * _occludedTransparency));

            GL.Vertex(a);
            GL.Vertex(b);
            GL.Vertex(c);
        }

        private void DrawMeshGL(List<Vector3> vertices, Color color, Matrix4x4 matrix)
        {
            GL.PushMatrix();
            GL.MultMatrix(matrix);

            GL.Begin(GL.LINES);
            GL.Color(color.ChangeAlpha(TRANSPARENCY * _occludedTransparency));

            for (var i = 0; i < vertices.Count; ++i)
                GL.Vertex(vertices[i]);

            GL.End();

            GL.PopMatrix();
        }

        private void SetProgramGL(int program)
        {
            if (_currentProgram != program)
            {
                _currentProgram = program;
                GL.End();

                switch (_currentProgram)
                {
                    case 1:
                        GL.Begin(GL.LINES);
                        break;
                    case 4:
                        GL.Begin(GL.TRIANGLES);
                        break;
                    case 5:
                        GL.Begin(GL.TRIANGLE_STRIP);
                        break;
                    case 7:
                        GL.Begin(GL.QUADS);
                        break;
                    default:
                        GL.Begin(GL.LINES);
                        break;
                }
            }
        }

        private void ResetProgramGL()
        {
            _currentProgram = -1;

            GL.End();
        }
#endif

        #endregion

        #region render

        private struct LineJob
        {
            public Vector3 start, end;
            public Color color;
            public bool continuous;

            public int frame;

            public readonly bool Alive => Time.frameCount - frame <= 0;
        }

        private struct TriangleJob
        {
            public Vector3 a, b, c;
            public Color color;
            public bool continuous;

            public int frame;

            public readonly bool Alive => Time.frameCount - frame <= 0;
        }

        private struct MeshJob
        {
            public List<Vector3> vertices;
            public Color color;
            public Matrix4x4 matrix;

            public int frame;

            public readonly bool Alive => Time.frameCount - frame <= 0;
        }

        private struct TextJob
        {
            public Vector3 position;
            public string text;
            public GUIStyle style;

            public int frame;

            public readonly bool Alive => Time.frameCount - frame <= 0;
        }

        private static LineJob[] lineJobs;
        private static TriangleJob[] triangleJobs;
        private static MeshJob[] meshJobs;
        private static TextJob[] textJobs;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int ZTest = Shader.PropertyToID("_ZTest");

#if UNITY_EDITOR
        private Material _materialVisible, _materialOccluded;

        private float _occludedTransparency = 1.0f;

        private void OnDebugRender(Camera camera)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Profiler.BeginSample(nameof(DebugDraw));

            CheckMaterials();
            CheckGeometry();

            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);

            _occludedTransparency = OCCLUDED_COLOR;
            _materialOccluded.SetPass(0);
            SubmitLines();
            SubmitTriangles();
            ResetProgramGL();
            SubmitMeshes();

            _occludedTransparency = 1.0f;
            _materialVisible.SetPass(0);
            SubmitLines();
            SubmitTriangles();
            ResetProgramGL();
            SubmitMeshes();

            GL.PopMatrix();

            SubmitTexts();

            Profiler.EndSample();
        }

        private void CheckMaterials()
        {
            if (_materialVisible == null || _materialOccluded)
            {
                var shader = Shader.Find("Hidden/Internal-Colored");
                _materialVisible = new Material(shader) {hideFlags = HideFlags.HideAndDontSave, shader = {hideFlags = HideFlags.HideAndDontSave}};
                _materialVisible.SetInt(SrcBlend, (int) BlendMode.SrcAlpha);
                _materialVisible.SetInt(DstBlend, (int) BlendMode.OneMinusSrcAlpha);
                _materialVisible.SetInt(Cull, (int) CullMode.Off);
                _materialVisible.SetInt(ZWrite, 1);
                _materialVisible.SetInt(ZTest, (int) CompareFunction.Less);

                _materialOccluded = new Material(shader) {hideFlags = HideFlags.HideAndDontSave};
                _materialOccluded.shader.hideFlags = HideFlags.HideAndDontSave;
                _materialOccluded.SetInt(SrcBlend, (int) BlendMode.SrcAlpha);
                _materialOccluded.SetInt(DstBlend, (int) BlendMode.OneMinusSrcAlpha);
                _materialOccluded.SetInt(Cull, (int) CullMode.Off);
                _materialOccluded.SetInt(ZWrite, 1);
                _materialOccluded.SetInt(ZTest, (int) CompareFunction.GreaterEqual);
            }
        }

        private void CheckGeometry()
        {
            lineJobs ??= new LineJob[CAPACITY];
            triangleJobs ??= new TriangleJob[CAPACITY];
            meshJobs ??= new MeshJob[CAPACITY];
            textJobs ??= new TextJob[TEXT_CAPACITY];
        }

        private void SubmitLines()
        {
            for (var i = 0; i < CAPACITY; ++i)
            {
                if (lineJobs[i].Alive)
                {
                    if (lineJobs[i].continuous) DrawLineGL(lineJobs[i].start, lineJobs[i].end, lineJobs[i].color);
                    else DrawLineGLDotted(lineJobs[i].start, lineJobs[i].end, lineJobs[i].color);
                }
            }
        }

        private void SubmitTriangles()
        {
            for (var i = 0; i < CAPACITY; ++i)
            {
                if (triangleJobs[i].Alive)
                {
                    if (triangleJobs[i].continuous)
                    {
                        DrawTriangleGLSolid(triangleJobs[i].a, triangleJobs[i].b, triangleJobs[i].c, triangleJobs[i].color);
                    }
                    else
                    {
                        DrawTriangleGL(triangleJobs[i].a, triangleJobs[i].b, triangleJobs[i].c, triangleJobs[i].color);
                    }
                }
            }
        }

        private void SubmitMeshes()
        {
            for (var i = 0; i < CAPACITY; ++i)
            {
                if (meshJobs[i].Alive) DrawMeshGL(meshJobs[i].vertices, meshJobs[i].color, meshJobs[i].matrix);
            }
        }

        private void SubmitTexts()
        {
            UnityEditor.Handles.BeginGUI();

            for (var i = 0; i < TEXT_CAPACITY; ++i)
            {
                if (textJobs[i].Alive)
                {
                    var position = UnityEditor.HandleUtility.WorldToGUIPointWithDepth(textJobs[i].position);
                    if (position.z >= 0.0f)
                    {
                        var content = UnityEditor.EditorGUIUtility.TrTempContent(textJobs[i].text);
                        GUI.Label(UnityEditor.HandleUtility.WorldPointToSizedRect(textJobs[i].position, content, textJobs[i].style ?? GUI.skin.label),
                            content,
                            textJobs[i].style ?? GUI.skin.label);
                    }
                }
            }

            UnityEditor.Handles.EndGUI();
        }

        private int GetLineIndex()
        {
            int length = lineJobs?.Length ?? 0;
            for (var i = 0; i < length; ++i)
            {
                if (lineJobs[i].Alive == false) return i;
            }

            return -1;
        }

        private int GetTriangleIndex()
        {
            int length = triangleJobs?.Length ?? 0;
            for (var i = 0; i < length; ++i)
            {
                if (triangleJobs[i].Alive == false) return i;
            }

            return -1;
        }

        private int GetMeshIndex()
        {
            int length = meshJobs?.Length ?? 0;
            for (var i = 0; i < length; ++i)
            {
                if (meshJobs[i].Alive == false)
                    return i;
            }

            return -1;
        }

        private int GetTextIndex()
        {
            int length = textJobs?.Length ?? 0;
            for (var i = 0; i < length; ++i)
            {
                if (textJobs[i].Alive == false) return i;
            }

            return -1;
        }

        private void Clear()
        {
            for (var i = 0; lineJobs != null && i < lineJobs.Length; ++i)
                lineJobs[i].frame = -1;

            for (var i = 0; triangleJobs != null && i < triangleJobs.Length; ++i)
                triangleJobs[i].frame = -1;

            for (var i = 0; meshJobs != null && i < meshJobs.Length; ++i)
                meshJobs[i].frame = -1;

            for (var i = 0; textJobs != null && i < textJobs.Length; ++i)
                textJobs[i].frame = -1;
        }
#else
    private int GetLineIndex() => 0;

    private int GetTriangleIndex() => 0;

    private int GetMeshIndex() => 0;

    private int GetTextIndex() => 0;
#endif

        #endregion

        #region api

        /// <summary> Draw a three-axis cross. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Axis(Vector3 position, float size, Quaternion rotation)
        {
            float halfSize = size * 0.5f;

            Line(position + rotation * Vector3.right * halfSize, position - rotation * Vector3.right * halfSize, null, Color.red);
            Line(position + rotation * Vector3.up * halfSize, position - rotation * Vector3.up * halfSize, null, Color.green);
            Line(position + rotation * Vector3.forward * halfSize, position - rotation * Vector3.forward * halfSize, null, Color.blue);
        }

        /// <summary> Draw a three-axis cross. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Axis(Vector3 position, float? size = null, Quaternion? rotation = null) => Axis(position, size ?? POINT_SIZE, rotation ?? Quaternion.identity);

        /// <summary> Draw a point with a three-axis cross. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Point(Vector3 position, float size, Quaternion rotation, Color color, bool continuous)
        {
            float halfSize = size * 0.5f;

            Line(position + rotation * Vector3.right * halfSize,
                position - rotation * Vector3.right * halfSize,
                null,
                color,
                continuous);
            Line(position + rotation * Vector3.up * halfSize,
                position - rotation * Vector3.up * halfSize,
                null,
                color,
                continuous);
            Line(position + rotation * Vector3.forward * halfSize,
                position - rotation * Vector3.forward * halfSize,
                null,
                color,
                continuous);
        }

        /// <summary> Draw a point with a three-axis cross. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Point(Vector3 position, float? size = null, Quaternion? rotation = null, Color? color = null, bool continuous = true) =>
            Point(position,
                size ?? POINT_SIZE,
                rotation ?? Quaternion.identity,
                color ?? AxisXColor,
                continuous);

        /// <summary> Draw an array of points using three-axis crosshairs. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Points(Vector3[] points, float size, Quaternion rotation, Color color, bool continuous)
        {
            for (var i = 0; i < points.Length; ++i)
                Point(points[i],
                    size,
                    rotation,
                    color,
                    continuous);
        }

        /// <summary> Draw an array of points using three-axis crosshairs. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Points(Vector3[] points, float? size = null, Quaternion? rotation = null, Color? color = null, bool continuous = true) =>
            Points(points,
                size ?? POINT_SIZE,
                rotation ?? Quaternion.identity,
                color ?? AxisXColor,
                continuous);

        /// <summary> Draw a line. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Line(Vector3 a, Vector3 b, Quaternion? rotation = null, Color? color = null, bool continuous = true)
        {
            int index = Instance.GetLineIndex();
            if (index != -1)
            {
                lineJobs[index].start = rotation != null ? (Quaternion) rotation * a : a;
                lineJobs[index].end = rotation != null ? (Quaternion) rotation * b : b;
                lineJobs[index].color = color ?? LineColor;
                lineJobs[index].continuous = continuous;
                lineJobs[index].frame = Time.frameCount;
            }
        }

        /// <summary> Draw an array of lines. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Lines(Vector3[] lines, Quaternion? rotation = null, Color? color = null, bool continuous = true)
        {
            for (var i = 0; i < lines.Length - 1; ++i)
                Line(lines[i],
                    lines[i + 1],
                    rotation,
                    color,
                    continuous);
        }

        /// <summary> Draw a triangle. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Triangle(Vector3 a, Vector3 b, Vector3 c, Quaternion? rotation = null, Color? color = null, bool continuous = true)
        {
            int index = Instance.GetTriangleIndex();
            if (index != -1)
            {
                triangleJobs[index].a = rotation != null ? (Quaternion) rotation * a : a;
                triangleJobs[index].b = rotation != null ? (Quaternion) rotation * b : b;
                triangleJobs[index].c = rotation != null ? (Quaternion) rotation * c : c;
                triangleJobs[index].color = color ?? LineColor;
                triangleJobs[index].continuous = continuous;
                triangleJobs[index].frame = Time.frameCount;
            }
        }

        /// <summary> Draw a line using an arrow. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Arrow(Vector3 position, Vector3 direction, float length = 1.0f, float? size = null, float? width = null, Color? color = null)
        {
            float sideLen = length - length * (size ?? ARROW_TIP_SIZE);
            var widthOffset = Vector3.Cross(direction, Vector3.up) * (width ?? ARROW_WIDTH);
            var tip = position + direction * length;
            var upCornerInRight = position - widthOffset * 0.3f + direction * sideLen;
            var upCornerInLeft = position + widthOffset * 0.3f + direction * sideLen;
            var upCornerOutRight = position - widthOffset * 0.5f + direction * sideLen;
            var upCornerOutLeft = position + widthOffset * 0.5f + direction * sideLen;

            Line(position, upCornerInRight, Quaternion.identity, color ?? ArrowColor);
            Line(upCornerInRight, upCornerOutRight, Quaternion.identity, color ?? ArrowColor);
            Line(upCornerOutRight, tip, Quaternion.identity, color ?? ArrowColor);
            Line(tip, upCornerOutLeft, Quaternion.identity, color ?? ArrowColor);
            Line(upCornerOutLeft, upCornerInLeft, Quaternion.identity, color ?? ArrowColor);
            Line(upCornerInLeft, position, Quaternion.identity, color ?? ArrowColor);
        }

        /// <summary> Draw a line using an arrow. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Arrow(Vector3 position, Quaternion rotation, float length = 1.0f, float? size = null, float? width = null, Color? color = null) =>
            Arrow(position,
                rotation * Vector3.forward,
                length,
                size,
                width,
                color);

        /// <summary> Draw a ray. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Ray(Vector3 position, Quaternion rotation, Color? color = null) =>
            Line(position, rotation * Vector3.forward * RAY_LENGTH, Quaternion.identity, color ?? RayColor);

        /// <summary> Draw a ray. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Ray(Vector3 position, Vector3 direction, Color? color = null) =>
            Line(position, position + (direction * direction.magnitude), Quaternion.identity, color ?? RayColor);

        /// <summary> Draw a wire circle. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Circle(Vector3 center, float radius, Quaternion rotation, Color color, bool continuous)
        {
            var forward = (rotation * Vector3.forward).normalized;
            var right = (rotation * Vector3.right).normalized;

            var b = center + (forward * radius);
            float angleStep = Mathf.PI * 2.0f / DIVISIONS;

            for (var i = 0; i < DIVISIONS; ++i)
            {
                float angle = (i == DIVISIONS - 1) ? 0.0f : (i + 1) * angleStep;

                var next = new Vector3(Mathf.Sin(angle), 0.0f, Mathf.Cos(angle)) * radius;
                var a = center + (right * next.x) + (forward * next.z);

                Line(a,
                    b,
                    Quaternion.identity,
                    color,
                    continuous);

                b = a;
            }
        }

        /// <summary> Draw a wire circle. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Circle(Vector3 center, float radius, Quaternion? rotation = null, Color? color = null, bool continuous = true) =>
            Circle(center,
                radius,
                rotation ?? Quaternion.identity,
                color ?? CircleColor,
                continuous);

        /// <summary> Draw a solid circle. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void CircleColid(Vector3 center, float radius, Quaternion rotation, Color color)
        {
            var normal = rotation == null ? Vector3.up : rotation * Vector3.up;
            var forward = Vector3.Cross(normal, normal.x < normal.z ? Vector3.right : Vector3.forward).normalized;
            var right = Vector3.Cross(forward, normal).normalized;

            var b = center + (forward * radius);
            float angleStep = Mathf.PI * 2.0f / DIVISIONS;

            for (var i = 0; i < DIVISIONS; ++i)
            {
                float angle = (i == DIVISIONS - 1) ? 0.0f : (i + 1) * angleStep;

                var next = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
                var a = center + (right * next.x) + (forward * next.z);

                Triangle(a,
                    b,
                    center,
                    null,
                    color);

                b = a;
            }
        }

        /// <summary> Draw a solid circle. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void CircleSolid(Vector3 center, float radius, Quaternion? rotation = null, Color? color = null) =>
            CircleColid(center, radius, rotation ?? Quaternion.identity, color ?? CircleColor);

        /// <summary> Draw a wire sphere. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Sphere(Vector3 center, float radius, Quaternion? rotation = null, Color? color = null)
        {
            if (sphereWire == null)
                CreateSphereWireMesh();

            int index = Instance.GetMeshIndex();
            if (index != -1)
            {
                meshJobs[index].vertices = sphereWire;
                meshJobs[index].color = color ?? SphereColor;
                meshJobs[index].matrix = Matrix4x4.TRS(center, rotation ?? Quaternion.identity, Vector3.one * radius);
                meshJobs[index].frame = Time.frameCount;
            }
        }

        /// <summary> Draw an arc centered on the forward vector. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Arc(Vector3 center, float radius, float angle, Quaternion rotation, Color color, bool continuous)
        {
            var from = rotation * Quaternion.Euler(0.0f, -angle * 0.5f, 0.0f) * Vector3.forward;
            from.Normalize();
            var rot = Quaternion.AngleAxis(angle / (DIVISIONS / 4 - 1), rotation * Vector3.up);
            var vector = from;

            Line(center, center + vector * radius, Quaternion.identity, color);

            int num = DIVISIONS / 4;
            for (var i = 1; i < num; ++i)
            {
                var a = center + vector * radius;
                vector = rot * vector;
                var b = center + vector * radius;

                Line(a, b, Quaternion.identity, color);
            }

            Line(center, center + vector * radius, Quaternion.identity, color);
        }

        /// <summary> Draw an arc centered on the forward vector. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Arc(Vector3 center, float radius, float angle, Quaternion? rotation = null, Color? color = null, bool continuous = true) =>
            Arc(center,
                radius,
                angle,
                rotation ?? Quaternion.identity,
                color ?? ArcColor,
                continuous);

        /// <summary> Draw a solid arc centered on the forward vector. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void ArcSolid(Vector3 center, float radius, float angle, Quaternion rotation, Color color)
        {
            var from = rotation * Quaternion.Euler(0.0f, -angle * 0.5f, 0.0f) * Vector3.forward;
            from.Normalize();
            var rot = Quaternion.AngleAxis(angle / (DIVISIONS / 4f - 1), rotation * Vector3.up);
            var vector = from;

            int num = DIVISIONS / 4;
            for (var i = 1; i < num; ++i)
            {
                var b = center + vector * radius;
                vector = rot * vector;
                var c = center + vector * radius;

                Triangle(center,
                    b,
                    c,
                    null,
                    color);
            }
        }

        /// <summary> Draw a solid arc centered on the forward vector. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void ArcSolid(Vector3 center, float radius, float angle, Quaternion? rotation = null, Color? color = null) =>
            ArcSolid(center,
                radius,
                angle,
                rotation ?? Quaternion.identity,
                color ?? ArcColor);

        /// <summary> Draw a wire cube. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Cube(Vector3 center, Vector3 size, Quaternion? rotation = null, Color? color = null)
        {
            if (cubeWire == null)
                CreateCubeWireMesh();

            int index = Instance.GetMeshIndex();
            if (index != -1)
            {
                meshJobs[index].vertices = cubeWire;
                meshJobs[index].color = color ?? CubeColor;
                meshJobs[index].matrix = Matrix4x4.TRS(center, rotation ?? Quaternion.identity, size);
                meshJobs[index].frame = Time.frameCount;
            }
        }

        /// <summary> Draw a wire cube. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Cube(Vector3 center, float size, Quaternion? rotation = null, Color? color = null) =>
            Cube(center, Vector3.one * size, rotation, color ?? CubeColor);

        /// <summary> Draw a wire diamond. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Diamond(Vector3 center, float size, Quaternion rotation, Color color, bool continuous)
        {
            var u = center + Vector3.up * size;
            var d = center + Vector3.down * size;
            var r = center + Vector3.right * size;
            var l = center + Vector3.left * size;
            var f = center + Vector3.forward * size;
            var b = center + Vector3.back * size;

            Lines(new[] {u, r, f, u, f, l, u, l, b, u, b, r}, rotation, color, continuous);

            Lines(new[] {d, f, r, d, r, b, d, b, l, d, l, f}, rotation, color, continuous);
        }

        /// <summary> Draw a wire diamond. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Diamond(Vector3 center, float? size = null, Quaternion? rotation = null, Color? color = null, bool continuous = true) =>
            Diamond(center,
                size ?? DIAMOND_SIZE,
                rotation ?? Quaternion.identity,
                color ?? DiamondColor,
                continuous);

        /// <summary> Draw a wire cone. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Cone(Vector3 position, float angle, float length, Quaternion rotation, Color color, bool continuous)
        {
            var direction = rotation * Vector3.forward;

            var forward = direction;
            var up = Vector3.Slerp(forward, -forward, 0.5f);
            var right = Vector3.Cross(forward, up).normalized * length;

            direction = direction.normalized;

            var slerpedVector = Vector3.Slerp(forward, up, angle / 90.0f);

            Plane farPlane = new(-direction, position + forward);
            Ray distRay = new(position, slerpedVector);

            farPlane.Raycast(distRay, out float dist);

            Ray(position, slerpedVector.normalized * dist, color);
            Ray(position, Vector3.Slerp(forward, -up, angle / 90.0f).normalized * dist, color);
            Ray(position, Vector3.Slerp(forward, right, angle / 90.0f).normalized * dist, color);
            Ray(position, Vector3.Slerp(forward, -right, angle / 90.0f).normalized * dist, color);

            Circle(position + forward,
                (forward - (slerpedVector.normalized * dist)).magnitude,
                rotation * Quaternion.Euler(90.0f, 0.0f, 0.0f),
                color,
                continuous);
        }

        /// <summary> Draw a wire cone. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Cone(Vector3 position, float angle, float length, Quaternion? rotation = null, Color? color = null, bool continuous = true) =>
            Cone(position,
                angle,
                length,
                rotation ?? Quaternion.identity,
                color ?? ConeColor,
                continuous);

        /// <summary> Draw bounds. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Bounds(Bounds b, Color? color = null)
        {
            var boundsColor = color ?? BoundsColor;

            Vector3 lbf = new(b.min.x, b.min.y, b.max.z);
            Vector3 ltb = new(b.min.x, b.max.y, b.min.z);
            Vector3 rbb = new(b.max.x, b.min.y, b.min.z);
            Line(b.min, lbf, Quaternion.identity, boundsColor);
            Line(b.min, ltb, Quaternion.identity, boundsColor);
            Line(b.min, rbb, Quaternion.identity, boundsColor);

            Vector3 rtb = new(b.max.x, b.max.y, b.min.z);
            Vector3 rbf = new(b.max.x, b.min.y, b.max.z);
            Vector3 ltf = new(b.min.x, b.max.y, b.max.z);
            Line(b.max, rtb, Quaternion.identity, boundsColor);
            Line(b.max, rbf, Quaternion.identity, boundsColor);
            Line(b.max, ltf, Quaternion.identity, boundsColor);

            Line(rbb, rbf, Quaternion.identity, boundsColor);
            Line(rbb, rtb, Quaternion.identity, boundsColor);

            Line(lbf, rbf, Quaternion.identity, boundsColor);
            Line(lbf, ltf, Quaternion.identity, boundsColor);

            Line(ltb, rtb, Quaternion.identity, boundsColor);
            Line(ltb, ltf, Quaternion.identity, boundsColor);
        }

        /// <summary> Draw bounds. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Bounds(BoundsInt b, Color color) => Bounds(new Bounds(b.center, b.size), color);

        /// <summary> Draw text. </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Text(Vector3 position, string text, GUIStyle style = null)
        {
            int index = Instance.GetTextIndex();
            if (index != -1)
            {
                textJobs[index].text = text;
                textJobs[index].position = position;
                textJobs[index].style = style;
                textJobs[index].frame = Time.frameCount;
            }
        }

        #endregion
    }
}