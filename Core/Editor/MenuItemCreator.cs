using Pancake.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public static class MenuItemCreator
    {
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