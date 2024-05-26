using System.Collections.Generic;
using DebugUI;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.DebugView
{
    public class DebugView : MonoBehaviour
    {
        [SerializeField] protected UIDocument uiDocument;
        [SerializeField] private SerializableReactiveProperty<Vector2Int> referenceResolution = new(new Vector2Int(800, 600));
        protected readonly List<DebugPageBase> pages = new();
        protected readonly DebugUIBuilder builder = new();

        private void Start()
        {
            referenceResolution.Subscribe(OnResolutionChanged);
            builder.ConfigureWindowOptions(options =>
            {
                options.Title = "Debug View";
                options.Draggable = true;
            });
            Configure();
            foreach (var page in pages)
            {
                page.Configure(builder);
            }

            builder.BuildWith(uiDocument);
            return;
            
            void OnResolutionChanged(Vector2Int value) { uiDocument.panelSettings.referenceResolution = value; }
        }

        protected virtual void Configure() { pages.Add(new DefaultPage()); }
    }
}