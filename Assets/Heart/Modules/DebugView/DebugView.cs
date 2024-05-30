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
        [SerializeField] private InputBasedFlickEvent flickEvent;
        [SerializeField] private FlickToOpenMode flickToOpen = FlickToOpenMode.Edge;
        [SerializeField] private SerializableReactiveProperty<Vector2Int> referenceResolution = new(new Vector2Int(800, 600));
        protected readonly List<DebugPageBase> pages = new();
        protected readonly DebugUIBuilder builder = new();
        private float _dpi;

        private const float THRESHOLD_INCH = 0.24f;

        private void Start()
        {
            float dpi = Screen.dpi;
            if (dpi == 0) dpi = 326;
            _dpi = dpi;

            flickEvent.flicked.AddListener(OnFlicked);
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

            builder.AddSpace(1);
            builder.AddButton("Close", HideDebug);
            builder.BuildWith(uiDocument);
            HideDebug();
            return;

            void OnResolutionChanged(Vector2Int value) { uiDocument.panelSettings.referenceResolution = value; }
        }

        private void HideDebug() { SetDisplayStatusUiDocument(); }

        private void ShowDebug() { SetDisplayStatusUiDocument(DisplayStyle.Flex); }

        private void SetDisplayStatusUiDocument(DisplayStyle style = DisplayStyle.None)
        {
            for (var i = 0; i < uiDocument.rootVisualElement.hierarchy.childCount; i++)
            {
                uiDocument.rootVisualElement.hierarchy.ElementAt(i).style.display = style;
            }
        }

        private void OnFlicked(Flick flick)
        {
            if (flickToOpen == FlickToOpenMode.Off) return;

            // If it is horizontal flick, ignore it.
            bool isVertical = Mathf.Abs(flick.DeltaInchPosition.y) > Mathf.Abs(flick.DeltaInchPosition.x);
            if (!isVertical) return;

            // Determines whether flicking is valid or not by the global control mode.
            float startPosXInch = flick.TouchStartScreenPosition.x / _dpi;
            float totalInch = Screen.width / _dpi;
            float leftSafeAreaInch = Screen.safeArea.xMin / _dpi;
            bool isLeftEdge = startPosXInch <= THRESHOLD_INCH + leftSafeAreaInch;
            float rightSafeAreaInch = (Screen.width - Screen.safeArea.xMax) / _dpi;
            bool isRightEdge = startPosXInch >= totalInch - (THRESHOLD_INCH + rightSafeAreaInch);
            var isValid = false;
            switch (flickToOpen)
            {
                case FlickToOpenMode.Edge:
                    isValid = isLeftEdge || isRightEdge;
                    break;
                case FlickToOpenMode.LeftEdge:
                    isValid = isLeftEdge;
                    break;
                case FlickToOpenMode.RightEdge:
                    isValid = isRightEdge;
                    break;
                case FlickToOpenMode.Off:
                    break;
            }

            if (!isValid) return;

            // Apply the flick.
            bool isUp = flick.DeltaInchPosition.y >= 0;
            if (isUp) ShowDebug();
        }

        protected virtual void Configure() { pages.Add(new DefaultPage()); }
    }
}