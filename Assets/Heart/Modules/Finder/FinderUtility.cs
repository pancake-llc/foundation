#if PANCAKE_ADDRESSABLE
using UnityEditor.AddressableAssets;
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using System.IO;

namespace PancakeEditor.Finder
{
    public static class FinderUtility
    {
        internal static bool isEditorPlaying;
        internal static bool isEditorUpdating;
        internal static bool isEditorCompiling;
        internal static bool isEditorPlayingOrWillChangePlaymode;

        public static void RefreshEditorStatus()
        {
            isEditorPlaying = EditorApplication.isPlaying;
            isEditorUpdating = EditorApplication.isUpdating;
            isEditorCompiling = EditorApplication.isCompiling;
            isEditorPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static AssetType[] Filters => FiltersLazy.Value;

        private static readonly Lazy<AssetType[]> FiltersLazy = new(() => new AssetType[]
        {
            new("Scene", ".unity"), new("Prefab", ".prefab"), new("Model",
                ".3df",
                ".3dm",
                ".3dmf",
                ".3dv",
                ".3dx",
                ".c5d",
                ".lwo",
                ".lws",
                ".ma",
                ".mb",
                ".mesh",
                ".vrl",
                ".wrl",
                ".wrz",
                ".fbx",
                ".dae",
                ".3ds",
                ".dxf",
                ".obj",
                ".skp",
                ".max",
                ".blend"),
            new("Material", ".mat", ".cubemap", ".physicsmaterial"), new("Texture",
                ".ai",
                ".apng",
                ".png",
                ".bmp",
                ".cdr",
                ".dib",
                ".eps",
                ".exif",
                ".ico",
                ".icon",
                ".j",
                ".j2c",
                ".j2k",
                ".jas",
                ".jiff",
                ".jng",
                ".jp2",
                ".jpc",
                ".jpe",
                ".jpeg",
                ".jpf",
                ".jpg",
                "jpw",
                "jpx",
                "jtf",
                ".mac",
                ".omf",
                ".qif",
                ".qti",
                "qtif",
                ".tex",
                ".tfw",
                ".tga",
                ".tif",
                ".tiff",
                ".wmf",
                ".psd",
                ".exr",
                ".rendertexture"),
            new("Video",
                ".asf",
                ".asx",
                ".avi",
                ".dat",
                ".divx",
                ".dvx",
                ".mlv",
                ".m2l",
                ".m2t",
                ".m2ts",
                ".m2v",
                ".m4e",
                ".m4v",
                "mjp",
                ".mov",
                ".movie",
                ".mp21",
                ".mp4",
                ".mpe",
                ".mpeg",
                ".mpg",
                ".mpv2",
                ".ogm",
                ".qt",
                ".rm",
                ".rmvb",
                ".wmv",
                ".xvid",
                ".flv"),
            new("Audio",
                ".mp3",
                ".wav",
                ".ogg",
                ".aif",
                ".aiff",
                ".mod",
                ".it",
                ".s3m",
                ".xm"),
            new("Script", ".cs", ".js", ".boo"), new("Text",
                ".txt",
                ".json",
                ".xml",
                ".bytes",
                ".sql"),
            new("Shader", ".shader", ".cginc"), new("Animation",
                ".anim",
                ".controller",
                ".overridecontroller",
                ".mask"),
            new("Unity Asset",
                ".asset",
                ".guiskin",
                ".flare",
                ".fontsettings",
                ".prefs"),
            new("Others") //
        });

        public static HashSet<string> selectionAssetGUIDs;

        public static bool StringStartsWith(string source, params string[] prefixes)
        {
            if (string.IsNullOrEmpty(source)) return false;
            for (var i = 0; i < prefixes.Length; i++)
            {
                if (source.StartsWith(prefixes[i])) return true;
            }

            return false;
        }

        public static void SplitPath(string assetPath, out string assetName, out string assetExtension, out string assetFolder)
        {
            assetName = string.Empty;
            assetFolder = string.Empty;
            assetExtension = string.Empty;

            if (string.IsNullOrEmpty(assetPath)) return;

            assetExtension = Path.GetExtension(assetPath);
            assetName = Path.GetFileNameWithoutExtension(assetPath);
            int lastSlash = assetPath.LastIndexOf("/", StringComparison.Ordinal) + 1;
            assetFolder = assetPath.Substring(0, lastSlash);
        }

        public static string[] SelectionAssetGUIDs
        {
            get
            {
                var objs = Selection.objects;

                selectionAssetGUIDs = new HashSet<string>();
                foreach (var item in objs)
                {
                    try
                    {
                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(item, out string guid, out long fileid)) selectionAssetGUIDs.Add(guid + "/" + fileid);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return Selection.assetGUIDs;
            }
        }


        public static Dictionary<int, string> HashClassesNormal => HashClassesNormalLazy.Value;

        private static readonly Lazy<Dictionary<int, string>> HashClassesNormalLazy = new(() => new Dictionary<int, string>
        {
            {1, "UnityEngine.GameObject"},
            {2, "UnityEngine.Component"},
            {4, "UnityEngine.Transform"},
            {8, "UnityEngine.Behaviour"},
            {12, "UnityEngine.ParticleAnimator"},
            {15, "UnityEngine.EllipsoidParticleEmitter"},
            {20, "UnityEngine.Camera"},
            {21, "UnityEngine.Material"},
            {23, "UnityEngine.MeshRenderer"},
            {25, "UnityEngine.Renderer"},
            {26, "UnityEngine.ParticleRenderer"},
            {27, "UnityEngine.Texture"},
            {28, "UnityEngine.Texture2D"},
            {33, "UnityEngine.MeshFilter"},
            {41, "UnityEngine.OcclusionPortal"},
            {43, "UnityEngine.Mesh"},
            {45, "UnityEngine.Skybox"},
            {47, "UnityEngine.QualitySettings"},
            {48, "UnityEngine.Shader"},
            {49, "UnityEngine.TextAsset"},
            {50, "UnityEngine.Rigidbody2D"},
            {53, "UnityEngine.Collider2D"},
            {54, "UnityEngine.Rigidbody"},
            {56, "UnityEngine.Collider"},
            {57, "UnityEngine.Joint"},
            {58, "UnityEngine.CircleCollider2D"},
            {59, "UnityEngine.HingeJoint"},
            {60, "UnityEngine.PolygonCollider2D"},
            {61, "UnityEngine.BoxCollider2D"},
            {62, "UnityEngine.PhysicsMaterial2D"},
            {64, "UnityEngine.MeshCollider"},
            {65, "UnityEngine.BoxCollider"},
            {68, "UnityEngine.EdgeCollider2D"},
            {72, "UnityEngine.ComputeShader"},
            {74, "UnityEngine.AnimationClip"},
            {75, "UnityEngine.ConstantForce"},
            {81, "UnityEngine.AudioListener"},
            {82, "UnityEngine.AudioSource"},
            {83, "UnityEngine.AudioClip"},
            {84, "UnityEngine.RenderTexture"},
            {87, "UnityEngine.MeshParticleEmitter"},
            {88, "UnityEngine.ParticleEmitter"},
            {89, "UnityEngine.Cubemap"},
            {90, "Avatar"},
            {92, "UnityEngine.GUILayer"},
            {93, "UnityEngine.RuntimeAnimatorController"},
            {95, "UnityEngine.Animator"},
            {96, "UnityEngine.TrailRenderer"},
            {102, "UnityEngine.TextMesh"},
            {104, "UnityEngine.RenderSettings"},
            {108, "UnityEngine.Light"},
            {111, "UnityEngine.Animation"},
            {114, "UnityEngine.MonoBehaviour"},
            {115, "UnityEditor.MonoScript"},
            {117, "UnityEngine.Texture3D"},
            {119, "UnityEngine.Projector"},
            {120, "UnityEngine.LineRenderer"},
            {121, "UnityEngine.Flare"},
            {123, "UnityEngine.LensFlare"},
            {124, "UnityEngine.FlareLayer"},
            {128, "UnityEngine.Font"},
            {129, "UnityEditor.PlayerSettings"},
            {131, "UnityEngine.GUITexture"},
            {132, "UnityEngine.GUIText"},
            {133, "UnityEngine.GUIElement"},
            {134, "UnityEngine.PhysicMaterial"},
            {135, "UnityEngine.SphereCollider"},
            {136, "UnityEngine.CapsuleCollider"},
            {137, "UnityEngine.SkinnedMeshRenderer"},
            {138, "UnityEngine.FixedJoint"},
            {142, "UnityEngine.AssetBundle"},
            {143, "UnityEngine.CharacterController"},
            {144, "UnityEngine.CharacterJoint"},
            {145, "UnityEngine.SpringJoint"},
            {146, "UnityEngine.WheelCollider"},
            {152, "UnityEngine.MovieTexture"},
            {153, "UnityEngine.ConfigurableJoint"},
            {154, "UnityEngine.TerrainCollider"},
            {156, "UnityEngine.TerrainData"},
            {157, "UnityEngine.LightmapSettings"},
            {158, "UnityEngine.WebCamTexture"},
            {159, "UnityEditor.EditorSettings"},
            {162, "UnityEditor.EditorUserSettings"},
            {164, "UnityEngine.AudioReverbFilter"},
            {165, "UnityEngine.AudioHighPassFilter"},
            {166, "UnityEngine.AudioChorusFilter"},
            {167, "UnityEngine.AudioReverbZone"},
            {168, "UnityEngine.AudioEchoFilter"},
            {169, "UnityEngine.AudioLowPassFilter"},
            {170, "UnityEngine.AudioDistortionFilter"},
            {171, "UnityEngine.SparseTexture"},
            {180, "UnityEngine.AudioBehaviour"},
            {182, "UnityEngine.WindZone"},
            {183, "UnityEngine.Cloth"},
            {192, "UnityEngine.OcclusionArea"},
            {193, "UnityEngine.Tree"},
            {198, "UnityEngine.ParticleSystem"},
            {199, "UnityEngine.ParticleSystemRenderer"},
            {200, "UnityEngine.ShaderVariantCollection"},
            {205, "UnityEngine.LODGroup"},
            {207, "UnityEngine.Motion"},
            {212, "UnityEngine.SpriteRenderer"},
            {213, "UnityEngine.Sprite"},
            {215, "UnityEngine.ReflectionProbe"},
            {218, "UnityEngine.Terrain"},
            {220, "UnityEngine.LightProbeGroup"},
            {221, "UnityEngine.AnimatorOverrideController"},
            {222, "UnityEngine.CanvasRenderer"},
            {223, "UnityEngine.Canvas"},
            {224, "UnityEngine.RectTransform"},
            {225, "UnityEngine.CanvasGroup"},
            {226, "UnityEngine.BillboardAsset"},
            {227, "UnityEngine.BillboardRenderer"},
            {229, "UnityEngine.AnchoredJoint2D"},
            {230, "UnityEngine.Joint2D"},
            {231, "UnityEngine.SpringJoint2D"},
            {232, "UnityEngine.DistanceJoint2D"},
            {233, "UnityEngine.HingeJoint2D"},
            {234, "UnityEngine.SliderJoint2D"},
            {235, "UnityEngine.WheelJoint2D"},
            {246, "UnityEngine.PhysicsUpdateBehaviour2D"},
            {247, "UnityEngine.ConstantForce2D"},
            {248, "UnityEngine.Effector2D"},
            {249, "UnityEngine.AreaEffector2D"},
            {250, "UnityEngine.PointEffector2D"},
            {251, "UnityEngine.PlatformEffector2D"},
            {252, "UnityEngine.SurfaceEffector2D"},
            {258, "UnityEngine.LightProbes"},
            {290, "UnityEngine.AssetBundleManifest"},
            {1003, "UnityEditor.AssetImporter"},
            {1004, "UnityEditor.AssetDatabase"},
            {1006, "UnityEditor.TextureImporter"},
            {1007, "UnityEditor.ShaderImporter"},
            {1011, "UnityEngine.AvatarMask"},
            {1020, "UnityEditor.AudioImporter"},
            {1029, "UnityEditor.DefaultAsset"},
            {1032, "UnityEditor.SceneAsset"},
            {1035, "UnityEditor.MonoImporter"},
            {1040, "UnityEditor.ModelImporter"},
            {1042, "UnityEditor.TrueTypeFontImporter"},
            {1044, "UnityEditor.MovieImporter"},
            {1045, "UnityEditor.EditorBuildSettings"},
            {1050, "UnityEditor.PluginImporter"},
            {1051, "UnityEditor.EditorUserBuildSettings"},
            {1105, "UnityEditor.HumanTemplate"},
            {1110, "UnityEditor.SpeedTreeImporter"},
            {1113, "UnityEditor.LightmapParameters"}
        });

        public static T LoadAssetAtPath<T>(string path) where T : Object { return AssetDatabase.LoadAssetAtPath<T>(path); }

        public static void SetWindowTitle(EditorWindow window, string title) { window.titleContent = MyGUIContent.FromString(title); }

        public static void GetCompilingPhase(string path, out bool isPlugin, out bool isEditor)
        {
            isPlugin = path.StartsWith("Assets/Plugins/", StringComparison.Ordinal) || path.StartsWith("Assets/Standard Assets/", StringComparison.Ordinal) ||
                       path.StartsWith("Assets/Pro Standard Assets/", StringComparison.Ordinal);

            isEditor = path.Contains("/Editor/");
        }

        public static T LoadAssetWithGuid<T>(string guid) where T : Object
        {
            if (string.IsNullOrEmpty(guid)) return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void UnloadUnusedAssets()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            Resources.UnloadUnusedAssets();
        }

        internal static int Epoch(DateTime time)
        {
            return (int) (time.ToUniversalTime() - new DateTime(1970,
                1,
                1,
                0,
                0,
                0,
                0,
                DateTimeKind.Utc)).TotalSeconds;
        }

        internal static bool DrawToggle(ref bool v, string label)
        {
            bool v1 = GUILayout.Toggle(v, label);
            if (v1 != v)
            {
                v = v1;
                return true;
            }

            return false;
        }

        internal static bool DrawToggleToolbar(ref bool v, string label, float width)
        {
            bool v1 = GUILayout.Toggle(v, label, EditorStyles.toolbarButton, GUILayout.Width(width));
            if (v1 != v)
            {
                v = v1;
                return true;
            }

            return false;
        }

        internal static bool DrawToggleToolbar(ref bool v, GUIContent icon, float width)
        {
            bool v1 = GUILayout.Toggle(v, icon, EditorStyles.toolbarButton, GUILayout.Width(width));
            if (v1 != v)
            {
                v = v1;
                return true;
            }

            return false;
        }

        public static string GetAddressable(string guid)
        {
#if PANCAKE_ADDRESSABLE
            var aaSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            var entry = aaSettings.FindAssetEntry(guid);
            return entry != null ? entry.address : string.Empty;
#else
            return string.Empty;
#endif
        }

        internal static EditorWindow FindEditor(string className)
        {
            var list = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var item in list)
            {
                if (item.GetType().FullName == className) return item;
            }

            return null;
        }

        internal static void RepaintAllEditor(string className)
        {
            var list = Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (var item in list)
            {
                if (item.GetType().FullName != className) continue;

                item.Repaint();
            }
        }

        internal static void RepaintProjectWindows() { RepaintAllEditor("UnityEditor.ProjectBrowser"); }

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null) return type;
            }

            return null;
        }

        public static IEnumerable<Transform> GetAllChild(Transform root)
        {
            yield return root;
            if (root.childCount <= 0) yield break;

            for (var i = 0; i < root.childCount; i++)
            {
                foreach (var item in GetAllChild(root.GetChild(i)))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<GameObject> GetAllObjsInCurScene()
        {
            for (var j = 0; j < SceneManager.sceneCount; j++)
            {
                var scene = SceneManager.GetSceneAt(j);
                foreach (var item in GetGameObjectsInScene(scene))
                {
                    yield return item;
                }
            }

            if (EditorApplication.isPlaying)
            {
                //dont destroy scene
                GameObject temp = null;
                try
                {
                    temp = new GameObject();
                    Object.DontDestroyOnLoad(temp);
                    var dontDestroyOnLoad = temp.scene;
                    Object.DestroyImmediate(temp);
                    temp = null;

                    foreach (var item in GetGameObjectsInScene(dontDestroyOnLoad))
                    {
                        yield return item;
                    }
                }
                finally
                {
                    if (temp != null) Object.DestroyImmediate(temp);
                }
            }
        }

        private static IEnumerable<GameObject> GetGameObjectsInScene(Scene scene)
        {
            var rootObjects = new List<GameObject>();
            if (!scene.isLoaded) yield break;

            scene.GetRootGameObjects(rootObjects);

            // iterate root objects and do something
            for (var i = 0; i < rootObjects.Count; ++i)
            {
                var gameObject = rootObjects[i];

                foreach (var item in GetAllChild(gameObject))
                {
                    yield return item;
                }

                yield return gameObject;
            }
        }

        public static IEnumerable<GameObject> GetAllChild(GameObject target, bool returnMe = false)
        {
            if (returnMe) yield return target;

            if (target.transform.childCount > 0)
                for (var i = 0; i < target.transform.childCount; i++)
                {
                    yield return target.transform.GetChild(i).gameObject;
                    foreach (var item in GetAllChild(target.transform.GetChild(i).gameObject))
                    {
                        yield return item;
                    }
                }
        }

        public static IEnumerable<Object> GetAllRefObjects(GameObject obj)
        {
            var components = obj.GetComponents<Component>();
            foreach (var com in components)
            {
                if (com == null) continue;

                var serialized = new SerializedObject(com);
                var it = serialized.GetIterator().Copy();
                while (it.NextVisible(true))
                {
                    if (it.propertyType != SerializedPropertyType.ObjectReference) continue;

                    if (it.objectReferenceValue == null) continue;

                    yield return it.objectReferenceValue;
                }
            }
        }

        public static int StringMatch(string pattern, string input)
        {
            if (input == pattern) return int.MaxValue;

            if (input.Contains(pattern)) return int.MaxValue - 1;

            var pidx = 0;
            var score = 0;
            var tokenScore = 0;

            for (var i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch == pattern[pidx])
                {
                    tokenScore += tokenScore + 1; //increasing score for continuos token
                    pidx++;
                    if (pidx >= pattern.Length) break;
                }
                else
                    tokenScore = 0;

                score += tokenScore;
            }

            return score;
        }

        public static int GetIndex(string ext)
        {
            for (var i = 0; i < Filters.Length - 1; i++)
            {
                if (Filters[i].extension.Contains(ext)) return i;
            }

            return Filters.Length - 1; //Others
        }

        public static void GuiLine(int iHeight = 1)

        {
            var rect = EditorGUILayout.GetControlRect(false, iHeight);

            rect.height = iHeight;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        public static bool IsInAsset(GameObject obj) { return !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)); }

        public static string GetPrefabParent(Object obj)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
            return AssetDatabase.AssetPathToGUID(prefabPath);
        }

        public static string GetGameObjectPath(GameObject obj, bool includeMe = true)
        {
            if (obj == null) return string.Empty;

            string path = includeMe ? "/" + obj.name : "/";
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }

            path = path.TrimStart('/');
            return path;
        }

        public static bool CheckIsPrefab(GameObject obj) { return PrefabUtility.IsAnyPrefabInstanceRoot(obj); }

        public static TerrainTextureData[] GetTerrainTextureDatas(TerrainData data)
        {
            if (data == null || data.terrainLayers == null) return new TerrainTextureData[] { };

            var arr = new TerrainTextureData[data.terrainLayers.Length];
            for (var i = 0; i < data.terrainLayers.Length; i++)
            {
                var layer = data.terrainLayers[i];
                arr[i] = layer == null ? new TerrainTextureData() : new TerrainTextureData(layer.normalMapTexture, layer.maskMapTexture, layer.diffuseTexture);
            }

            return arr;
        }

        public static int ReplaceTerrainTextureDatas(TerrainData terrain, Texture2D fromObj, Texture2D toObj)
        {
            var found = 0;
            var arr3 = terrain.terrainLayers;
            for (var i = 0; i < arr3.Length; i++)
            {
                if (arr3[i].normalMapTexture == fromObj)
                {
                    found++;
                    arr3[i].normalMapTexture = toObj;
                }

                if (arr3[i].maskMapTexture == fromObj)
                {
                    found++;
                    arr3[i].maskMapTexture = toObj;
                }

                if (arr3[i].diffuseTexture == fromObj)
                {
                    found++;
                    arr3[i].diffuseTexture = toObj;
                }
            }

            terrain.terrainLayers = arr3;
            return found;
        }

        public static void Clear<T1, T2>(ref Dictionary<T1, T2> dict)
        {
            if (dict == null) dict = new Dictionary<T1, T2>();
            else dict.Clear();
        }

        public static void Clear<T>(ref List<T> list)
        {
            if (list == null) list = new List<T>();
            else list.Clear();
        }

        public static SerializedProperty[] XGetSerializedProperties(Object go, bool processArray)
        {
            var so = new SerializedObject(go);
            so.Update();
            var result = new List<SerializedProperty>();

            var iterator = so.GetIterator();
            while (iterator.NextVisible(true))
            {
                var copy = iterator.Copy();

                if (processArray && iterator.isArray) result.AddRange(XGetSoArray(copy));
                else result.Add(copy);
            }

            return result.ToArray();
        }

        public static List<SerializedProperty> XGetSoArray(SerializedProperty prop)
        {
            int size = prop.arraySize;
            var result = new List<SerializedProperty>();

            for (var i = 0; i < size; i++)
            {
                var p = prop.GetArrayElementAtIndex(i);

                if (p.isArray) result.AddRange(XGetSoArray(p.Copy()));
                else result.Add(p.Copy());
            }

            return result;
        }

        public class TerrainTextureData
        {
            public Texture2D[] textures;

            public TerrainTextureData(params Texture2D[] param)
            {
                var count = 0;
                if (param != null) count = param.Length;

                textures = new Texture2D[count];
                for (var i = 0; i < count; i++)
                {
                    textures[i] = param[i];
                }
            }
        }

        internal static void RepaintFinderWindows() { RepaintAllEditor("PancakeEditor.Finder.FinderWindow"); }

        public static void BackupAndDeleteAssets(FindRef[] assets)
        {
            var fileName = DateTime.Now.ToString("yyMMdd_hhmmss");

            var result = new List<string>();
            var selectedList = new List<string>();

            foreach (var item in assets)
            {
                if (item.asset == null) continue;
                string oPath = item.asset.AssetPath.Replace("\\", "/");
                if (!oPath.StartsWith("Assets/")) continue;
                result.Add(item.asset.AssetPath);

                if (item.IsSelected()) selectedList.Add(item.asset.AssetPath);
            }

            if (selectedList.Count != 0) result = selectedList;
            Directory.CreateDirectory("Library/Finder/");
            AssetDatabase.ExportPackage(result.ToArray(), "Library/Finder/bk_" + fileName + ".unitypackage");

            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < result.Count; i++)
                {
                    AssetDatabase.DeleteAsset(result[i]);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            FinderWindowBase.DelayCheck4Changes();
        }

        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            var result = new HashSet<T>();
            if (collection == null) return result;

            foreach (var item in collection)
            {
                result.Add(item);
            }

            return result;
        }
    }
}