using PancakeEditor.Common;
using Pancake.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public static class MenuItemCreator
    {
        private static RectTransform CreateUIPopupObject()
        {
            var prefabRoot = PrefabDatabase.GetPrefabRoot();
            if (prefabRoot != null)
            {
                return CreateUniPopup(Selection.activeTransform == null ? prefabRoot.transform : Selection.activeTransform);
            }

            // find canvas in scene
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
            var popup = CreateEmptyRectTransformObject(parent, "popup_basic");
            popup.anchorMin = Vector2.zero;
            popup.anchorMax = Vector2.one;
            popup.pivot = new Vector2(0.5f, 0.5f);
            popup.sizeDelta = Vector2.zero;
            popup.anchoredPosition3D = Vector3.zero;
            popup.localPosition = Vector3.zero;

            var container = CreateEmptyRectTransformObject(popup.transform, "container");
            container.gameObject.AddComponent<Image>().GetComponent<RectTransform>().sizeDelta = new Vector2(710, 730);

            var button = CreateObjectWithComponent<UIButton>(container.transform, "button_close");
            button.gameObject.layer = LayerMask.NameToLayer("UI");
            button.sizeDelta = new Vector2(125, 125);
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

            var eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
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
            // detect in prefab mode first

            var prefabRoot = PrefabDatabase.GetPrefabRoot();
            if (prefabRoot != null)
            {
                return CreateObjectWithComponent<T>(Selection.activeTransform == null ? prefabRoot.transform : Selection.activeTransform, name);
            }

            // find canvas in scene
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

        [MenuItem("GameObject/Pancake/UIButton", false, 1000)]
        public static void CreateUIButtonEmpty()
        {
            var t = CreateObject<Image>("Button");
            var button = t.gameObject.AddComponent<UIButton>();
            button.targetGraphic = t.GetComponent<Image>();
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton");
            SetupUIButton(t);
            Selection.activeTransform = t;
        }

        private static void SetupUIButton(RectTransform button)
        {
            var uiButton = button.GetComponent<UIButton>();
            button.sizeDelta = new Vector2(160, 60);
            uiButton.IsMotion = true;
        }

        [MenuItem("GameObject/Pancake/UIButton - TextMeshPro", false, 1000)]
        private static void CreateUIButtonText()
        {
            var t = CreateObject<Image>("Button");
            var button = t.gameObject.AddComponent<UIButtonText>();
            button.targetGraphic = t.GetComponent<Image>();
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton TMP");
            SetupUIButton(t);
            Selection.activeTransform = t;
        }

        [MenuItem("GameObject/Pancake/UIButton - Text Only", false, 1000)]
        private static void CreateUIButtonTextOnly()
        {
            var t = CreateObject<TextMeshProUGUI>("Button");
            var button = t.gameObject.AddComponent<UIButtonText>();
            button.targetGraphic = t.GetComponent<TextMeshProUGUI>();
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Create UIButton Only Text");
            SetupUIButton(t);
            Selection.activeTransform = t;
        }

        [MenuItem("GameObject/Pancake/UIPopup", false, 1000)]
        public static void CreateUIPopupEmpty()
        {
            var popup = CreateUIPopupObject();
            Undo.RegisterCreatedObjectUndo(popup.gameObject, "Create UIPopup");
            Selection.activeTransform = popup;
        }

        [MenuItem("GameObject/Pancake/UI Set Native Size + Pivot", false, 1000)]
        private static void SetSizeAndPivot()
        {
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                // process all selected game objects which have a RectTransform + Image
                RectTransform transform = gameObject.GetComponent<RectTransform>();
                Image image = gameObject.GetComponent<Image>();

                if (transform && image && image.sprite)
                {
                    // set size as it is defined by source image sprite
                    image.SetNativeSize();

#if UNITY_2018_1_OR_NEWER
                    // use mesh defined by source sprite to render UI image
                    image.useSpriteMesh = true;
#endif

                    // set pivot point as defined by source sprite
                    Vector2 size = transform.sizeDelta * image.pixelsPerUnit;
                    Vector2 pixelPivot = image.sprite.pivot;
                    // sprite pivot point is defined in pixel, RectTransform pivot point is normalized
                    transform.pivot = new Vector2(pixelPivot.x / size.x, pixelPivot.y / size.y);
                }
            }
        }

        [MenuItem("Assets/Create/Value Providers/Single Instance", false, 1000)]
        private static void ValueProviderHeader() { }

        [MenuItem("Assets/Create/Value Providers/Single Instance", true, 1000)]
        private static bool ValueProviderHeaderValidate() { return false; }
    }
}