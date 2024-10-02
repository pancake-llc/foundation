using System.Collections.Generic;
using Sirenix.OdinInspector;
using DebugUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.DebugView
{
    public class DebugView : MonoBehaviour
    {
        [SerializeField] protected UIDocument uiDocument;

        [SerializeField, OnValueChanged(nameof(OnResolutionChanged))]
        private Vector2Int referenceResolution = new(1080, 1920);

        protected readonly List<DebugPageBase> pages = new();
        protected readonly DebugUIBuilder builder = new();

        private void Start()
        {
            uiDocument.panelSettings.referenceResolution = referenceResolution;
            builder.ConfigureWindowOptions(options =>
            {
                options.Title = "Debug";
                options.Draggable = true;
            });
            Configure();
            foreach (var page in pages)
            {
                page.Configure(builder);
            }

            builder.AddSpace(1);
            //builder.AddButton("Close", HideDebug);
            builder.BuildWith(uiDocument);
            if (!HeartSettings.DebugView) HideDebug();

            return;
        }

        private void OnResolutionChanged(Vector2Int value) { uiDocument.panelSettings.referenceResolution = value; }

        private void HideDebug() { SetDisplayStatusUiDocument(); }

        private void ShowDebug() { SetDisplayStatusUiDocument(DisplayStyle.Flex); }

        private void SetDisplayStatusUiDocument(DisplayStyle style = DisplayStyle.None)
        {
            for (var i = 0; i < uiDocument.rootVisualElement.hierarchy.childCount; i++)
            {
                uiDocument.rootVisualElement.hierarchy.ElementAt(i).style.display = style;
            }
        }

        protected virtual void Configure() { pages.Add(new DefaultPage()); }

        private void OnDestroy()
        {
            foreach (var page in pages)
            {
                page.Dispose();
            }
        }
    }
}