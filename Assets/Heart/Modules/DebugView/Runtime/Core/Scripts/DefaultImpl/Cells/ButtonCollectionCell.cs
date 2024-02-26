using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class ButtonCollectionCell : Cell<ButtonCollectionCellModel>
    {
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform contents;
        [SerializeField] private CanvasGroup contentsCanvasGroup;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;

        public CollectionButton buttonPrefab;

        protected override void SetModel(ButtonCollectionCellModel model)
        {
            //_contentsCanvasGroup.alpha = model.Interactable ? 1.0f : 0.3f;

            // Remove all buttons
            while (gridLayoutGroup.transform.childCount > 0)
            {
                var child = gridLayoutGroup.transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            // Buttons
            foreach (var buttonModel in model.Buttons)
            {
                var button = Instantiate(buttonPrefab, gridLayoutGroup.transform);
                button.Setup(buttonModel);
            }

            // Height
            var buttonCount = model.Buttons.Count;
            var rowCount = Mathf.CeilToInt(buttonCount / (float) gridLayoutGroup.constraintCount);
            var height = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;
            height += (int) (rowCount * gridLayoutGroup.cellSize.y);
            height += (int) (gridLayoutGroup.spacing.y * (rowCount - 1));
            height += 1; // Border
            contents.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            layoutElement.preferredHeight = height; // Set the preferred height for the recycler view.
        }
    }

    public sealed class ButtonCollectionCellModel : CellModel
    {
        public List<CollectionButtonModel> Buttons { get; } = new List<CollectionButtonModel>();
    }
}