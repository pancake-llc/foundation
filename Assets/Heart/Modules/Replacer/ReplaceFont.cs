using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using TMPro;
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Pancake.ReplacerEditor
{
    public static class ReplaceFont
    {
        [MenuItem("Tools/Pancake/Replacer Font")]
        private static void ReplaceFontMenuItem() { ReplaceFontEditorWindow.ShowWindow(); }

        public static void ReplaceFontInScene(Font font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            List<Component> components = null;
            var currentScene = EditorSceneManager.GetActiveScene();

            var textComponents = Resources.FindObjectsOfTypeAll<Text>();

            Undo.SetCurrentGroupName("Replace all legacy text fonts");

            foreach (var component in textComponents)
            {
                if (component.gameObject.scene != currentScene) continue;

                Undo.RecordObject(component, "");
                component.font = font;
                components ??= new List<Component>();
                components.Add(component);
                Debug.Log($"Replaced: {component.name}", component);
            }

            if (components == null) Debug.LogError("Can't find any text components on current scene");

            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontInScene(TMP_FontAsset font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            List<Component> components = null;
            var currentScene = EditorSceneManager.GetActiveScene();

            var textComponents = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            Undo.SetCurrentGroupName("Replace all TMP fonts");

            foreach (var component in textComponents)
            {
                if (component.gameObject.scene != currentScene) continue;

                Undo.RecordObject(component, "");

                component.font = font;
                components ??= new List<Component>();
                components.Add(component);
                Debug.Log($"Replaced: {component.name}", component);
            }

            if (components == null) Debug.LogError("Can't find any TMP components on current scene");

            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontPrefab(Font font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            string[] prefabsPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();

            List<Component> components = null;

            Undo.SetCurrentGroupName("Replace all legacy text fonts");
            foreach (string path in prefabsPaths)
            {
                if (path.Contains("Packages")) continue;

                using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var prefab = prefabScope.prefabContentsRoot;

                    var prefabTexts = prefab.GetComponentsInChildren<Text>(true);
                    foreach (Text component in prefabTexts)
                    {
                        Undo.RecordObject(component, "");
                        component.font = font;
                        components ??= new List<Component>();
                        components.Add(component);
                    }

                    Debug.Log($"Replaced: {prefab.name}", prefab);
                }
            }

            if (components == null) Debug.LogError("Can't find any text components in prefabs");

            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontPrefab(TMP_FontAsset font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            string[] prefabsPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();

            List<Component> components = null;
            Undo.SetCurrentGroupName("Replace all TMP fonts");

            foreach (string path in prefabsPaths)
            {
                if (path.Contains("Packages")) continue;

                using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var prefab = prefabScope.prefabContentsRoot;

                    var prefabTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (TextMeshProUGUI component in prefabTexts)
                    {
                        Undo.RecordObject(component, "");
                        component.font = font;
                        components ??= new List<Component>();
                        components.Add(component);
                    }

                    Debug.Log($"Replaced: {prefab.name}", prefab);
                }
            }

            if (components == null)
                Debug.LogError("Can't find any TMP components in prefabs");

            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontSpecified(TMP_FontAsset font, List<TextMeshProUGUI> components)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            if (components.Count == 0)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Components List is null", "Ok");
                return;
            }

            Undo.SetCurrentGroupName("Replace specified TMP fonts");

            bool oneComponentIsEmpty = false;

            foreach (TextMeshProUGUI component in components)
            {
                if (component == null)
                {
                    oneComponentIsEmpty = true;
                    continue;
                }

                Undo.RecordObject(component, "");
                component.font = font;
                Debug.Log($"Replaced: {component.gameObject.name}", component);
            }

            if (oneComponentIsEmpty)
                EditorUtility.DisplayDialog("Replace font Result",
                    "One of the components of the list is empty. Please manually check the list of text objects you have set. All other components have been replaced successfully.",
                    "Ok");

            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontSpecified(Font font, List<Text> components)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            if (components.Count == 0)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Components List is null", "Ok");
                return;
            }

            Undo.SetCurrentGroupName("Replace specified legacy text fonts");

            bool oneComponentIsEmpty = false;

            foreach (Text component in components)
            {
                if (component == null)
                {
                    oneComponentIsEmpty = true;
                    continue;
                }

                Undo.RecordObject(component, "");
                component.font = font;
                Debug.Log($"Replaced: {component.gameObject.name}", component);
            }

            if (oneComponentIsEmpty)
                EditorUtility.DisplayDialog("Replace font Result",
                    "One of the components of the list is empty. Please manually check the list of text objects you have set. All other components have been replaced successfully.",
                    "Ok");

            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontInProject(Font font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            string[] scenesPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase)).ToArray();
            string[] prefabsPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();

            List<Component> components = null;

            Undo.SetCurrentGroupName("Replace all legacy text fonts");
            foreach (string path in prefabsPaths)
            {
                if (path.Contains("Packages"))
                    continue;

                using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var prefab = prefabScope.prefabContentsRoot;

                    var prefabTexts = prefab.GetComponentsInChildren<Text>(true);
                    foreach (Text component in prefabTexts)
                    {
                        Undo.RecordObject(component, "");
                        component.font = font;
                        if (components == null)
                            components = new();
                        components.Add(component);
                    }

                    Debug.Log($"Replaced: {prefab.name}", prefab);
                }
            }

            var currentScene = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            var textComponents = Component.FindObjectsOfType<Text>();

            foreach (string scene in scenesPaths)
            {
                EditorSceneManager.OpenScene(scene);
                foreach (var component in textComponents)
                {
                    Undo.RecordObject(component, "");
                    component.font = font;
                    components ??= new List<Component>();
                    components.Add(component);
                    Debug.Log($"Replaced: {component.name}", component);
                }

                EditorSceneManager.SaveOpenScenes();
            }

            EditorSceneManager.OpenScene(currentScene);

            if (components == null) Debug.LogError("Can't find any text components on all scenes");
            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontInProject(TMP_FontAsset font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            string[] scenesPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase)).ToArray();
            string[] prefabsPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();

            List<Component> components = null;

            Undo.SetCurrentGroupName("Replace all TMP fonts");
            foreach (string path in prefabsPaths)
            {
                if (path.Contains("Packages")) continue;

                using (var prefabScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var prefab = prefabScope.prefabContentsRoot;

                    var prefabTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (TextMeshProUGUI component in prefabTexts)
                    {
                        component.font = font;
                        components ??= new List<Component>();
                        components.Add(component);
                    }

                    Debug.Log($"Replaced: {prefab.name}", prefab);
                }
            }
            
            var currentScene = EditorSceneManager.GetActiveScene().path;
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            foreach (string scene in scenesPaths)
            {
                EditorSceneManager.OpenScene(scene);

                var textComponents = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

                foreach (var component in textComponents)
                {
                    component.font = font;
                    components ??= new List<Component>();
                    components.Add(component);
                    Debug.Log($"Replaced: {component.name}", component);
                }

                EditorSceneManager.SaveOpenScenes();
            }

            EditorSceneManager.OpenScene(currentScene);

            if (components == null) Debug.LogError("Can't find any TMP components on all scenes");
            Undo.IncrementCurrentGroup();
        }
    }
}