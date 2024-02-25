using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class ButtonCell : Cell<ButtonCellModel>
    {
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform contents;
        [SerializeField] private CanvasGroup contentsCanvasGroup;

        public CellIcon icon;
        public CellTexts cellTexts;
        public Image arrow;
        public Button button;

        protected override void SetModel(ButtonCellModel model)
        {
            contentsCanvasGroup.alpha = model.Interactable ? 1.0f : 0.3f;

            // Cleanup
            button.onClick.RemoveAllListeners();

            // Icon
            icon.Setup(model.Icon);
            icon.gameObject.SetActive(model.Icon.Sprite != null);

            //Texts
            cellTexts.Setup(model.CellTexts);

            // Arrow
            arrow.gameObject.SetActive(model.ShowArrow);

            // Button
            button.interactable = model.Interactable;
            button.onClick.AddListener(model.InvokeClicked);

            // Height
            var height = model.UseSubTextOrIcon ? 68 : 42; // Texts
            height += 36; // Padding
            height += 1; // Border
            contents.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            layoutElement.preferredHeight = height; // Set the preferred height for the recycler view.
        }
    }

    public sealed class ButtonCellModel : CellModel
    {
        public ButtonCellModel(bool useSubTextOrIcon) { UseSubTextOrIcon = useSubTextOrIcon; }

        public CellIconModel Icon { get; } = new CellIconModel();

        public bool UseSubTextOrIcon { get; }

        public CellTextsModel CellTexts { get; } = new CellTextsModel();

        public bool ShowArrow { get; set; }

        public bool Interactable { get; set; } = true;

        public event Action Clicked;

        internal void InvokeClicked() { Clicked?.Invoke(); }
    }
}