using System;
using Pancake.Linq;
using Pancake.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    public static class MenuItemCreator
    {
        [MenuItem("GameObject/Pancake/Self Filling", false, 1)]
        private static void AnchorFillinSelectedUIObjects()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform) rectTransform.SelfFilling();
            }
        }

        [MenuItem("GameObject/Pancake/Self Filling", true, 1)]
        private static bool AnchorFillinSelectedUIObjectsValidate()
        {
            bool flag = false;
            foreach (var obj in Selection.gameObjects)
            {
                var rectTransform = obj.GetComponent<RectTransform>();
                flag = rectTransform != null;
            }

            return flag;
        }

        [MenuItem("GameObject/Pancake/UIButton", false, 1000)]
        public static void CreateUniButtonEmpty()
        {
            var button = CreateObject<UIButton>("Button");
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton");
            SetupUIButton(button);
            Selection.activeTransform = button;
        }

        private static void SetupUIButton(RectTransform button)
        {
            var uiButton = button.GetComponent<UIButton>();
            button.sizeDelta = new Vector2(160, 60);
            uiButton.IsMotion = true;
        }

        [MenuItem("GameObject/Pancake/UIButton (TMP)", false, 1000)]
        private static void AddUniButtonTMP()
        {
            var button = CreateObject<UIButtonTMP>("Button");
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton TMP");
            SetupUIButton(button);
            Selection.activeTransform = button;
        }

        [MenuItem("GameObject/Pancake/UIPopup", false, 1100)]
        public static void CreateUIPopupEmpty()
        {
            var popup = CreateUIPopupObject();
            Undo.RegisterCreatedObjectUndo(popup.gameObject, "Create UniPopup");
            Selection.activeTransform = popup;
        }

        [MenuItem("GameObject/Pancake/Fetch", false, 1100)]
        private static void CreateFetch()
        {
            var obj = CreateObject<Image>("Fetch");
            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Create Fetch");
            SetupFetch(obj);
            Selection.activeTransform = obj;
        }

        [MenuItem("Assets/Create/Pancake/Loading Prefab")]
        private static void CreateLoaderPrefab()
        {
            var loadingCommon = InEditor.FindAssetWithPath<GameObject>("LoadingPrefabBase.prefab", "Runtime/UGUI/Loader/Prefabs");
            if (loadingCommon != null)
            {
                GameObject instanceRoot = (GameObject) PrefabUtility.InstantiatePrefab(loadingCommon);
                GameObject p = PrefabUtility.SaveAsPrefabAsset(instanceRoot, AssetDatabase.GetAssetPath(Selection.activeObject) + "/LoadingPrefab-Copy.prefab");
                Object.DestroyImmediate(instanceRoot);
            }
        }

        private static void SetupFetch(RectTransform fetch)
        {
            Sprite first = (Sprite) AssetDatabase.LoadAssetAtPath("Packages/com.pancake.heart/Runtime/UGUI/Fetch/Sprites/01.png", typeof(Sprite));
            if (first == null) first = (Sprite) AssetDatabase.LoadAssetAtPath("Assets/_Root/Runtime/UGUI/Fetch/Sprites/01.png", typeof(Sprite));
            var img = fetch.GetComponent<Image>();
            img.sprite = first;
            img.raycastTarget = false;
            var animator = fetch.gameObject.AddComponent<Animator>();

            animator.runtimeAnimatorController =
                (AnimatorController) AssetDatabase.LoadAssetAtPath("Packages/com.pancake.heart/Runtime/UGUI/Fetch/Animation/FetchAnimator.controller",
                    typeof(AnimatorController));

            if (animator.runtimeAnimatorController == null)
            {
                animator.runtimeAnimatorController =
                    (AnimatorController) AssetDatabase.LoadAssetAtPath("Assets/_Root/Runtime/UGUI/Fetch/Animation/FetchAnimator.controller", typeof(AnimatorController));
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Can not found FetchAnimator!");
            }
        }

        private static RectTransform CreateUIPopupObject()
        {
            // find canvas in scene
            var allCanvases = (Canvas[]) Object.FindObjectsOfType(typeof(Canvas));
            if (allCanvases.Length > 0)
            {
                if (Selection.activeTransform == null) return CreateUniPopup(allCanvases[0].transform);

                for (int i = 0; i < allCanvases.Length; i++)
                {
                    if (!Selection.activeTransform.IsChildOf(allCanvases[i].transform)) continue;
                    return CreateUniPopup(Selection.activeTransform);
                }

                return CreateUniPopup(allCanvases[0].transform);
            }

            var canvas = CreateCanvas();
            return CreateUniPopup(canvas.transform);
        }

        private static RectTransform CreateUniPopup(Transform parent)
        {
            var popup = CreateEmptyRectTransformObject(parent, "Popup");
            popup.gameObject.AddComponent<Canvas>().overrideSorting = true;
            popup.gameObject.AddComponent<GraphicRaycaster>();
            popup.FullScreen();

            var background = CreateEmptyRectTransformObject(popup.transform, "Background");
            background.FullScreen();
            background.gameObject.AddComponent<CanvasGroup>();
            var img = background.gameObject.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.78f);

            var container = CreateEmptyRectTransformObject(popup.transform, "Container");
            container.gameObject.AddComponent<CanvasGroup>();
            container.gameObject.AddComponent<Image>().GetComponent<RectTransform>().sizeDelta = new Vector2(800, 500);

            var button = CreateObjectWithComponent<UIButton>(container.transform, "BtnClose");
            button.gameObject.layer = LayerMask.NameToLayer("UI");
            button.sizeDelta = new Vector2(80, 80);
            button.anchorMax = Vector2.one;
            button.anchorMin = Vector2.one;
            button.anchoredPosition = Vector2.zero;
            return popup;
        }

        private static RectTransform CreateEmptyRectTransformObject(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.gameObject.layer = LayerMask.NameToLayer("UI");
            var rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        private static Canvas CreateCanvas()
        {
            var canvas = new GameObject("Canvas").AddComponent<Canvas>();
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            var eventSystem = (EventSystem) Object.FindObjectOfType(typeof(EventSystem));
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            return canvas;
        }

        private static RectTransform CreateObjectWithComponent<T>(Transform parent, string name) where T : Component
        {
            var r = CreateEmptyRectTransformObject(parent, name);
            r.gameObject.AddComponent<T>();
            return r;
        }

        private static RectTransform CreateObject<T>(string name) where T : Component
        {
            // find canvas in scene
            var allCanvases = (Canvas[]) Object.FindObjectsOfType(typeof(Canvas));
            if (allCanvases.Length > 0)
            {
                if (Selection.activeTransform == null) return CreateObjectWithComponent<T>(allCanvases[0].transform, name);

                for (int i = 0; i < allCanvases.Length; i++)
                {
                    if (!Selection.activeTransform.IsChildOf(allCanvases[i].transform)) continue;
                    return CreateObjectWithComponent<T>(Selection.activeTransform, name);
                }

                return CreateObjectWithComponent<T>(allCanvases[0].transform, name);
            }

            var canvas = CreateCanvas();
            return CreateObjectWithComponent<T>(canvas.transform, name);
        }
    }
}