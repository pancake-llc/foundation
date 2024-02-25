using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class LabelCell : Cell<LabelCellModel>
    {
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform contents;

        public CellIcon icon;
        public CellTexts cellTexts;

        protected override void SetModel(LabelCellModel model)
        {
            // Icon
            icon.Setup(model.Icon);
            icon.gameObject.SetActive(model.Icon.Sprite != null);

            //Texts
            cellTexts.Setup(model.CellTexts);

            // Height
            var height = model.UseSubTextOrIcon ? 68 : 42; // Texts
            height += 36; // Padding
            height += 1; // Border
            contents.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            layoutElement.preferredHeight = height; // Set the preferred height for the recycler view.
        }
    }

    public sealed class LabelCellModel : CellModel
    {
        public LabelCellModel(bool useSubTextOrIcon) { UseSubTextOrIcon = useSubTextOrIcon; }

        public CellIconModel Icon { get; } = new CellIconModel();

        public bool UseSubTextOrIcon { get; }

        public CellTextsModel CellTexts { get; } = new CellTextsModel();
    }
}