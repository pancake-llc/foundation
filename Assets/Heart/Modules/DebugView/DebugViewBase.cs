using System.Collections.Generic;
using Sirenix.OdinInspector;
#if PANCAKE_DEBUG_UI
using DebugUI;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.DebugView
{
    public abstract class DebugViewBase : MonoBehaviour
    {
        [SerializeField] protected UIDocument uiDocument;

        [SerializeField, OnValueChanged(nameof(OnResolutionChanged))]
        private Vector2Int referenceResolution = new(1080, 1920);

        protected readonly List<DebugPageBase> pages = new();
#if PANCAKE_DEBUG_UI
        protected readonly DebugUIBuilder builder = new();
#endif

        private void Start()
        {
            if (!HeartSettings.DebugView) return;
#if PANCAKE_DEBUG_UI
            uiDocument.panelSettings.referenceResolution = referenceResolution;
            builder.ConfigureWindowOptions(options =>
            {
                options.Title = "Debug";
                options.Draggable = true;
                options.Foldout = true;
            });
            Configure();
            foreach (var page in pages)
            {
                page.Configure(builder);
            }

            builder.AddSpace(1);
            builder.AddButton("Close", HideDebug);
            builder.BuildWith(uiDocument);

            HideDebug();
#endif
        }

        private void OnResolutionChanged(Vector2Int value) { uiDocument.panelSettings.referenceResolution = value; }

        protected void HideDebug()
        {
            if (!HeartSettings.DebugView) return;
            SetDisplayStatusUiDocument();
        }

        protected void ShowDebug()
        {
            if (!HeartSettings.DebugView) return;
            SetDisplayStatusUiDocument(DisplayStyle.Flex);
        }

        private void SetDisplayStatusUiDocument(DisplayStyle style = DisplayStyle.None)
        {
            for (var i = 0; i < uiDocument.rootVisualElement.hierarchy.childCount; i++)
            {
                uiDocument.rootVisualElement.hierarchy.ElementAt(i).style.display = style;
            }
        }

        protected virtual void Configure() { pages.Add(new DefaultDebugPage()); }

        private void OnDestroy()
        {
            foreach (var page in pages)
            {
                page.Dispose();
            }
        }
    }
}