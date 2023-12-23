using System;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryMeshFillScattererModule : GreeneryScattererModule
    {
        public int pointCount = 100;

        private const string MESH_FILL_SETTINGS_KEY = "GREENERY_MESH_FILL_SETTINGS";

        private float[] _sizes;
        private float[] _cumulativeSizes;
        private float _total;

        public override void Initialize(GreeneryScatteringModule scatteringModule)
        {
            base.Initialize(scatteringModule);
            if (EditorPrefs.HasKey(MESH_FILL_SETTINGS_KEY))
            {
                pointCount = EditorPrefs.GetInt(MESH_FILL_SETTINGS_KEY);
            }

            Undo.undoRedoPerformed += SaveSettings;
        }

        public override void Release() { Undo.undoRedoPerformed -= SaveSettings; }

        public override void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            int count = Mathf.Max(1, EditorGUILayout.IntField("Point count", pointCount));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(scatteringModule.toolEditor, "Changed mesh fill point count");
                pointCount = count;

                SaveSettings();
            }

            if (GUILayout.Button("Mesh fill"))
            {
                GreeneryManager greeneryManager = GreeneryEditorUtilities.GetActiveManager();
                if (Selection.activeGameObject != null)
                {
                    GameObject gameObject = Selection.activeGameObject;
                    MeshFilter meshFilter = gameObject.GetComponentInChildren<MeshFilter>();
                    if (meshFilter != null)
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
                        CalcAreas(mesh);
                        Undo.RegisterCompleteObjectUndo(greeneryManager, "Added spawn points");
                        for (int i = 0; i < count; i++)
                        {
                            var point = GetRandomPointOnMesh(mesh);
                            Vector3 pos = gameObject.transform.TransformPoint(point.Item1);
                            Vector3 normal = gameObject.transform.TransformDirection(point.Item2);
                            Vector2 uv = point.Item3;
                            GreeneryScatteringUtilities.AddSpawnPoint(greeneryManager,
                                scatteringModule.toolEditor.GetSelectedItems(),
                                pos,
                                normal,
                                scatteringModule.scatteringModuleSettings.getSurfaceColor ? GreenerySurfaceColorSampling.GetMeshColor(renderer, uv) : Color.clear,
                                scatteringModule.scatteringModuleSettings.sizeFactor,
                                scatteringModule.scatteringModuleSettings.colorGradient);
                        }

                        EditorUtility.SetDirty(greeneryManager);
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        public override void ToolHandles(Rect guiRect)
        {
            if (guiRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.Layout)
                {
                    HandleUtility.AddDefaultControl(0);
                }
            }
        }

        public override float GetHeight() { return EditorGUIUtility.singleLineHeight * 3; }

        public override GUIContent GetIcon() { return EditorGUIUtility.IconContent("d_Grid.FillTool"); }

        protected override void SaveSettings() { EditorPrefs.SetInt(MESH_FILL_SETTINGS_KEY, pointCount); }

        #region Functionality

        //Based on https://gist.github.com/danieldownes/b1c9bab09cce013cc30a4198bfeda0aa

        public void CalcAreas(Mesh mesh)
        {
            _sizes = GetTriSizes(mesh.triangles, mesh.vertices);
            _cumulativeSizes = new float[_sizes.Length];
            _total = 0;

            for (int i = 0; i < _sizes.Length; i++)
            {
                _total += _sizes[i];
                _cumulativeSizes[i] = _total;
            }
        }

        public (Vector3, Vector3, Vector2) GetRandomPointOnMesh(Mesh mesh)
        {
            float randomsample = UnityEngine.Random.value * _total;
            int triIndex = -1;

            for (int i = 0; i < _sizes.Length; i++)
            {
                if (randomsample <= _cumulativeSizes[i])
                {
                    triIndex = i;
                    break;
                }
            }

            if (triIndex == -1)
                Debug.LogError("triIndex should never be -1");

            Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
            Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
            Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

            Vector3 na = mesh.normals[mesh.triangles[triIndex * 3]];
            Vector3 nb = mesh.normals[mesh.triangles[triIndex * 3 + 1]];
            Vector3 nc = mesh.normals[mesh.triangles[triIndex * 3 + 2]];

            Vector2 uva = mesh.uv[mesh.triangles[triIndex * 3]];
            Vector2 uvb = mesh.uv[mesh.triangles[triIndex * 3 + 1]];
            Vector2 uvc = mesh.uv[mesh.triangles[triIndex * 3 + 2]];

            // Generate random barycentric coordinates
            float r = UnityEngine.Random.value;
            float s = UnityEngine.Random.value;

            if (r + s >= 1)
            {
                r = 1 - r;
                s = 1 - s;
            }

            // Turn point back to a Vector3
            Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
            Vector3 normal = (na + r * (nb - na) + s * (nc - na)).normalized;
            Vector2 uv = uva + r * (uvb - uva) + s * (uvc - uva);
            return (pointOnMesh, normal, uv);
        }

        public float[] GetTriSizes(int[] tris, Vector3[] verts)
        {
            int triCount = tris.Length / 3;
            float[] sizes = new float[triCount];
            for (int i = 0; i < triCount; i++)
            {
                sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
            }

            return sizes;
        }

        #endregion
    }
}