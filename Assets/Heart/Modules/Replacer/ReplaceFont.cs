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

            var currentScene = EditorSceneManager.GetActiveScene();

            Undo.SetCurrentGroupName("Replace all legacy text fonts");
            var countFontReplaceInScene = 0;
            foreach (GameObject go in currentScene.GetRootGameObjects())
            {
                var textComponents = go.GetComponentsInChildren<Text>(true).ToList();
                foreach (var t in textComponents)
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(t)) textComponents.Remove(t);
                }

                countFontReplaceInScene += textComponents.Count;
                foreach (var component in textComponents)
                {
                    if (component.gameObject.scene != currentScene) continue;

                    Undo.RecordObject(component, "");
                    component.font = font;
                }
            }

            if (countFontReplaceInScene > 0) Debug.Log($"[Scene] Replaced font of {countFontReplaceInScene} component Text in: {currentScene.name}");
            else Debug.LogWarning("Can't find any text components on scene :" + currentScene.name);

            EditorSceneManager.SaveOpenScenes();
            Undo.IncrementCurrentGroup();
        }

        public static void ReplaceFontInScene(TMP_FontAsset font)
        {
            if (font == null)
            {
                EditorUtility.DisplayDialog("Replace font Result", "Font is null", "Ok");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            var countFontReplaceInScene = 0;
            Undo.SetCurrentGroupName("Replace all TMP fonts");
            foreach (GameObject go in currentScene.GetRootGameObjects())
            {
                var textComponents = go.GetComponentsInChildren<TextMeshProUGUI>(true).ToList();
                foreach (var t in textComponents.ToList())
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(t)) textComponents.Remove(t);
                }

                countFontReplaceInScene += textComponents.Count;
                foreach (var component in textComponents)
                {
                    if (component.gameObject.scene != currentScene) continue;

                    Undo.RecordObject(component, "");
                    component.font = font;
                }
            }

            if (countFontReplaceInScene > 0) Debug.Log($"[Scene] Replaced font of {countFontReplaceInScene} component Text in: {currentScene.name}");
            else Debug.LogWarning("Can't find any text components on scene :" + currentScene.name);

            EditorSceneManager.SaveOpenScenes();
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

            int count = 0;
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
                        count++;
                    }

                    if (prefabTexts.Length > 0) Debug.Log($"[Prefab] Replaced font of {prefabTexts.Length} component Text in: {prefab.name}", prefab);
                }
            }

            if (count == 0) Debug.LogWarning("Can't find any text components in prefabs");

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

            int count = 0;
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
                        count++;
                    }

                    if (prefabTexts.Length > 0) Debug.Log($"[Prefab] Replaced font of {prefabTexts.Length} component Text in: {prefab.name}", prefab);
                }
            }

            if (count == 0) Debug.LogWarning("Can't find any TMP components in prefabs");

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
    }
}