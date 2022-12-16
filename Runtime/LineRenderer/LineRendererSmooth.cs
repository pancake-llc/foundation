#if UNITY_EDITOR
using Pancake.Linq;
using UnityEditor;
#endif
using UnityEngine;

namespace Pancake
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineRendererSmooth : MonoBehaviour
    {
        [System.Serializable]
        public class BezierCurve
        {
            public Vector3[] points;

            public BezierCurve() { points = new Vector3[4]; }

            public BezierCurve(Vector3[] points) { this.points = points; }

            public Vector3 StartPosition => points[0];

            public Vector3 EndPosition => points[3];

            public Vector3 GetSegment(float time)
            {
                time = Mathf.Clamp01(time);
                float t = 1 - time;
                return (t * t * t * points[0]) + (3 * t * t * time * points[1]) + (3 * t * time * time * points[2]) + (time * time * time * points[3]);
            }

            public Vector3[] GetSegments(int subdivisions)
            {
                var segments = new Vector3[subdivisions];

                for (int i = 0; i < subdivisions; i++)
                {
                    float time = (float) i / subdivisions;
                    segments[i] = GetSegment(time);
                }

                return segments;
            }
        }

        public LineRenderer line;
        public Vector3[] initState = new Vector3[1];
        public float smoothLength = 0;
        public int smoothSections = 1;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(LineRendererSmooth))]
    public class LineRendererSmoothEditor : UnityEditor.Editor
    {
        private LineRendererSmooth _lineRenderer;

        private SerializedProperty _line;
        private SerializedProperty _initState;
        private SerializedProperty _smoothLength;
        private SerializedProperty _smoothSections;

        private readonly GUIContent _updateInitialStateGUIContent = new GUIContent("Set Initial State");
        private readonly GUIContent _smoothButtonGUIContent = new GUIContent("Smooth Path");
        private readonly GUIContent _restoreDefaultGUIContent = new GUIContent("Restore Default Path");

        private bool _showCurves = true;
        private LineRendererSmooth.BezierCurve[] Curves;

        private void OnEnable()
        {
            _lineRenderer = (LineRendererSmooth) target;

            if (_lineRenderer.line == null)
            {
                _lineRenderer.line = _lineRenderer.GetComponent<LineRenderer>();
            }

            _line = serializedObject.FindProperty("line");
            _initState = serializedObject.FindProperty("initState");
            _smoothLength = serializedObject.FindProperty("smoothLength");
            _smoothSections = serializedObject.FindProperty("smoothSections");

            EnsureCurvesMatchLineRendererPositions();
        }

        public override void OnInspectorGUI()
        {
            if (_lineRenderer == null)
            {
                return;
            }

            EnsureCurvesMatchLineRendererPositions();

            EditorGUILayout.PropertyField(_line);
            _showCurves = EditorGUILayout.Toggle("Show Curves", _showCurves, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(_initState);
            EditorGUILayout.PropertyField(_smoothLength);
            EditorGUILayout.PropertyField(_smoothSections);

            if (GUILayout.Button(_updateInitialStateGUIContent))
            {
                _lineRenderer.initState = new Vector3[_lineRenderer.line.positionCount];
                _lineRenderer.line.GetPositions(_lineRenderer.initState);
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUI.enabled = _lineRenderer.line.positionCount >= 3;
                if (GUILayout.Button(_smoothButtonGUIContent))
                {
                    SmoothPath();
                }

                bool lineRendererPathAndInitialStateAreSame = _lineRenderer.line.positionCount == _lineRenderer.initState.Length;

                if (lineRendererPathAndInitialStateAreSame)
                {
                    Vector3[] positions = new Vector3[_lineRenderer.line.positionCount];
                    _lineRenderer.line.GetPositions(positions);

                    lineRendererPathAndInitialStateAreSame = positions.SequenceEqual(_lineRenderer.initState);
                }

                GUI.enabled = !lineRendererPathAndInitialStateAreSame;
                if (GUILayout.Button(_restoreDefaultGUIContent))
                {
                    _lineRenderer.line.positionCount = _lineRenderer.initState.Length;
                    _lineRenderer.line.SetPositions(_lineRenderer.initState);

                    EnsureCurvesMatchLineRendererPositions();
                }
            }
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(50);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Smooth Line"))
            {
                Smooth();
            }

            if (GUILayout.Button("Generate Collider"))
            {
                GenerateMeshCollider();
            }

            if (GUILayout.Button("Simply Mesh"))
            {
                _lineRenderer.line.Simplify(0.1f);
            }

            EditorGUILayout.EndHorizontal();


            serializedObject.ApplyModifiedProperties();
        }

        private void SmoothPath()
        {
            _lineRenderer.line.positionCount = Curves.Length * _smoothSections.intValue;
            int index = 0;
            for (int i = 0; i < Curves.Length; i++)
            {
                Vector3[] segments = Curves[i].GetSegments(_smoothSections.intValue);
                for (int j = 0; j < segments.Length; j++)
                {
                    _lineRenderer.line.SetPosition(index, segments[j]);
                    index++;
                }
            }

            // Reset values so inspector doesn't freeze if you use lots of smoothing sections
            _smoothSections.intValue = 1;
            _smoothLength.floatValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (_showCurves)
            {
                if (_lineRenderer.line.positionCount < 3)
                {
                    return;
                }

                EnsureCurvesMatchLineRendererPositions();

                for (int i = 0; i < Curves.Length; i++)
                {
                    Vector3 position = _lineRenderer.line.GetPosition(i);
                    Vector3 lastPosition = i == 0 ? _lineRenderer.line.GetPosition(0) : _lineRenderer.line.GetPosition(i - 1);
                    Vector3 nextPosition = _lineRenderer.line.GetPosition(i + 1);

                    Vector3 lastDirection = (position - lastPosition).normalized;
                    Vector3 nextDirection = (nextPosition - position).normalized;

                    Vector3 startTangent = (lastDirection + nextDirection) * _smoothLength.floatValue;
                    Vector3 endTangent = (nextDirection + lastDirection) * -1 * _smoothLength.floatValue;

                    Handles.color = Color.green;
                    Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive),
                        position + startTangent,
                        Quaternion.identity,
                        0.25f,
                        EventType.Repaint);

                    if (i != 0)
                    {
                        Handles.color = Color.blue;
                        Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive),
                            nextPosition + endTangent,
                            Quaternion.identity,
                            0.25f,
                            EventType.Repaint);
                    }

                    Curves[i].points[0] = position; // Start Position (P0)
                    Curves[i].points[1] = position + startTangent; // Start Tangent (P1)
                    Curves[i].points[2] = nextPosition + endTangent; // End Tangent (P2)
                    Curves[i].points[3] = nextPosition; // End Position (P3)
                }

                // Apply look-ahead for first curve and retroactively apply the end tangent
                {
                    Vector3 nextDirection = (Curves[1].EndPosition - Curves[1].StartPosition).normalized;
                    Vector3 lastDirection = (Curves[0].EndPosition - Curves[0].StartPosition).normalized;

                    Curves[0].points[2] = Curves[0].points[3] + (nextDirection + lastDirection) * -1 * _smoothLength.floatValue;

                    Handles.color = Color.blue;
                    Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive),
                        Curves[0].points[2],
                        Quaternion.identity,
                        0.25f,
                        EventType.Repaint);
                }

                DrawSegments();
            }

            if (_lineRenderer.gameObject.TryGetComponent<MeshCollider>(out MeshCollider meshCollider))
            {
                for (int i = 0; i < meshCollider.sharedMesh.vertexCount; i++)
                {
                    Handles.color = Color.black;
                    Handles.Label(meshCollider.sharedMesh.vertices[i], $"{i}");
                }
            }
        }

        private void DrawSegments()
        {
            for (int i = 0; i < Curves.Length; i++)
            {
                Vector3[] segments = Curves[i].GetSegments(_smoothSections.intValue);
                for (int j = 0; j < segments.Length - 1; j++)
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(segments[j], segments[j + 1]);

                    float color = (float) j / segments.Length;
                    Handles.color = new Color(color, color, color);
                    Handles.Label(segments[j], $"C{i} S{j}");
                    Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive),
                        segments[j],
                        Quaternion.identity,
                        0.05f,
                        EventType.Repaint);
                }

                Handles.color = Color.white;
                Handles.Label(segments[segments.Length - 1], $"C{i} S{segments.Length - 1}");
                Handles.DotHandleCap(EditorGUIUtility.GetControlID(FocusType.Passive),
                    segments[segments.Length - 1],
                    Quaternion.identity,
                    0.05f,
                    EventType.Repaint);

                Handles.DrawLine(segments[segments.Length - 1], Curves[i].EndPosition);
            }
        }

        private void EnsureCurvesMatchLineRendererPositions()
        {
            if (Curves == null || Curves.Length != _lineRenderer.line.positionCount - 1)
            {
                Curves = new LineRendererSmooth.BezierCurve[_lineRenderer.line.positionCount - 1];
                for (int i = 0; i < Curves.Length; i++)
                {
                    Curves[i] = new LineRendererSmooth.BezierCurve();
                }
            }
        }

        public void GenerateMeshCollider()
        {
            MeshCollider col = _lineRenderer.GetComponent<MeshCollider>();

            if (col == null) col = _lineRenderer.gameObject.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            _lineRenderer.line.BakeMesh(mesh, true);

            // if you need collisions on both sides of the line, simply duplicate & flip facing the other direction!
            // This can be optimized to improve performance ;)
            int[] meshIndices = mesh.GetIndices(0);
            int[] newIndices = new int[meshIndices.Length * 2];

            int j = meshIndices.Length - 1;
            for (int i = 0; i < meshIndices.Length; i++)
            {
                newIndices[i] = meshIndices[i];
                newIndices[meshIndices.Length + i] = meshIndices[j];
            }

            mesh.SetIndices(newIndices, MeshTopology.Triangles, 0);

            col.sharedMesh = mesh;
        }

        public void Smooth()
        {
            LineRendererSmooth.BezierCurve[] curves = new LineRendererSmooth.BezierCurve[_lineRenderer.line.positionCount - 1];
            for (int i = 0; i < curves.Length; i++)
            {
                curves[i] = new LineRendererSmooth.BezierCurve();
            }

            for (int i = 0; i < curves.Length; i++)
            {
                Vector3 position = _lineRenderer.line.GetPosition(i);
                Vector3 lastPosition = i == 0 ? _lineRenderer.line.GetPosition(0) : _lineRenderer.line.GetPosition(i - 1);
                Vector3 nextPosition = _lineRenderer.line.GetPosition(i + 1);

                Vector3 lastDirection = (position - lastPosition).normalized;
                Vector3 nextDirection = (nextPosition - position).normalized;

                Vector3 startTangent = (lastDirection + nextDirection) * _lineRenderer.smoothLength;
                Vector3 endTangent = (nextDirection + lastDirection) * -1 * _lineRenderer.smoothLength;


                curves[i].points[0] = position; // Start Position (P0)
                curves[i].points[1] = position + startTangent; // Start Tangent (P1)
                curves[i].points[2] = nextPosition + endTangent; // End Tangent (P2)
                curves[i].points[3] = nextPosition; // End Position (P3)
            }

            // Apply look-ahead for first curve and retroactively apply the end tangent
            {
                Vector3 nextDirection = (curves[1].EndPosition - curves[1].StartPosition).normalized;
                Vector3 lastDirection = (curves[0].EndPosition - curves[0].StartPosition).normalized;

                curves[0].points[2] = curves[0].points[3] + (nextDirection + lastDirection) * -1 * _lineRenderer.smoothLength;
            }

            _lineRenderer.line.positionCount = curves.Length * _lineRenderer.smoothSections;
            int index = 0;
            for (int i = 0; i < curves.Length; i++)
            {
                Vector3[] segments = curves[i].GetSegments(_lineRenderer.smoothSections);
                for (int j = 0; j < segments.Length; j++)
                {
                    _lineRenderer.line.SetPosition(index, segments[j]);
                    index++;
                }
            }
        }
    }
#endif
}