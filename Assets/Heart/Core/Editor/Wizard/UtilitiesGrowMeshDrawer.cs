using System;
using Pancake.GrowMeshEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesGrowMeshDrawer
    {
        public static void OnInspectorGUI(ref GameObject target, ref int level, EditorWindow window)
        {
            EditorGUILayout.HelpBox("SubMesh is not supported.", MessageType.Warning);
            EditorGUILayout.HelpBox("Smooth the mesh by increasing the number of vertices. Should be used for polygon models and should use level 1", MessageType.Info);
            EditorGUILayout.HelpBox("If you want to export it into an FBX file, use the FBX Exporter package from unity's manger package", MessageType.Info);
            
            target = EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true) as GameObject;
            level = EditorGUILayout.IntSlider("Level", level, 1, 3);

            if (GUILayout.Button("Grow", GUILayout.Height(30f)))
            {
                try
                {
                    if (target == null)
                    {
                        window.ShowNotification(new GUIContent("Target can not be null!"));
                        return;
                    }

                    var importer = (ModelImporter) AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(target));
                    if (importer != null)
                    {
                        importer.isReadable = true;
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                        AssetDatabase.Refresh();
                    }

                    var allMeshFilters = target.GetComponentsInChildren<MeshFilter>(true);
                    var skinMeshs = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    bool isSkinMesh = skinMeshs.Length > 0;

                    if (allMeshFilters.Length == 0 && skinMeshs.Length == 0)
                    {
                        window.ShowNotification(new GUIContent("Target gameObject must contain SkinnedMeshRenderer or MeshRenerder"));
                        return;
                    }

                    for (var i = 0; i < skinMeshs.Length; i++)
                    {
                        if (skinMeshs[i].sharedMesh.triangles.Length > 30000)
                        {
                            window.ShowNotification(new GUIContent("Mesh cannot have a number of triangles greater than 30k"));
                            return;
                        }
                    }

                    for (var i = 0; i < allMeshFilters.Length; i++)
                    {
                        if (allMeshFilters[i].sharedMesh.triangles.Length > 30000)
                        {
                            window.ShowNotification(new GUIContent("Mesh cannot have a number of triangles greater than 30k"));
                            return;
                        }
                    }

                    var isGen = true;

                    EditorApplication.update = delegate()
                    {
                        bool isCancel = EditorUtility.DisplayCancelableProgressBar("In execution. . .",
                            "Too many vertices will take a long time, please be patient.",
                            0.5f);
                        if (isGen == false)
                        {
                            EditorUtility.ClearProgressBar();
                            EditorApplication.update = null;
                        }
                    };

                    var newFitlerMeshs = new Mesh[allMeshFilters.Length];
                    var newSkineMeshs = new Mesh[skinMeshs.Length];
                    for (var i = 0; i < allMeshFilters.Length; i++)
                    {
                        newFitlerMeshs[i] = Generate(allMeshFilters[i].sharedMesh, level, isSkinMesh);
                    }

                    for (var i = 0; i < skinMeshs.Length; i++)
                    {
                        newSkineMeshs[i] = Generate(skinMeshs[i].sharedMesh, level, isSkinMesh);
                    }

                    var newObj = GameObject.Instantiate(target);
                    newObj.transform.parent = target.transform.parent;
                    target.gameObject.SetActive(false);
                    newObj.gameObject.SetActive(true);

                    var newAllMeshFilters = newObj.GetComponentsInChildren<MeshFilter>(true);
                    var NewSkinMeshs = newObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                    for (var i = 0; i < newAllMeshFilters.Length; i++)
                    {
                        newAllMeshFilters[i].sharedMesh = newFitlerMeshs[i];
                    }

                    for (var i = 0; i < NewSkinMeshs.Length; i++)
                    {
                        NewSkinMeshs[i].sharedMesh = newSkineMeshs[i];
                    }
                    //newObj.GetComponentInChildren<MeshFilter>().sharedMesh = newMesh;

                    isGen = false;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        private static Mesh Generate(Mesh mesh, int level, bool isSkinMesh)
        {
            var newMesh = new Mesh();

            Mesh shareMesh = null;

            newMesh.vertices = mesh.vertices.Clone() as Vector3[];
            newMesh.normals = mesh.normals.Clone() as Vector3[];
            newMesh.uv = mesh.uv.Clone() as Vector2[];
            if (mesh.uv2.Length > 0)
            {
                newMesh.uv2 = mesh.uv2.Clone() as Vector2[];
            }

            newMesh.triangles = mesh.triangles.Clone() as int[];
            newMesh.boneWeights = mesh.boneWeights.Clone() as BoneWeight[];
            newMesh.bindposes = mesh.bindposes.Clone() as Matrix4x4[];
            newMesh.name = mesh.name;
            newMesh.colors = mesh.colors.Clone() as Color[];
            newMesh.bounds = mesh.bounds;
            newMesh.tangents = mesh.tangents.Clone() as Vector4[];
            shareMesh = mesh;

            for (var m = 0; m < level; m++)
            {
                GrowMesh.Gen(newMesh);
            }

            if (isSkinMesh)
            {
                var _mesh = newMesh;
                var _beforMesh = shareMesh;

                var weights = new BoneWeight[_mesh.boneWeights.Length];

                var beforVertices = _beforMesh.vertices;
                var _meshVertices = _mesh.vertices;
                var beforBoneWeights = _beforMesh.boneWeights;
                var meshBoneWeights = _mesh.boneWeights;

                for (var i = 0; i < meshBoneWeights.Length; i++)
                {
                    var disIndex = 0;
                    var distance = float.MaxValue;

                    for (var j = 0; j < beforVertices.Length; j++)
                    {
                        var dis = Vector3.Distance(_meshVertices[i], beforVertices[j]);
                        if (distance > dis)
                        {
                            disIndex = j;
                            distance = dis;
                        }
                    }

                    weights[i] = beforBoneWeights[disIndex];
                }

                newMesh.boneWeights = weights;
            }

            return newMesh;
        }
    }
}