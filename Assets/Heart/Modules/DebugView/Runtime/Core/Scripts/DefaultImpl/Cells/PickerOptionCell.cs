﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class PickerOptionCell : Cell<PickerOptionCellModel>
    {
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform contents;
        [SerializeField] private CanvasGroup contentsCanvasGroup;

        public CellIcon icon;
        public CellTexts cellTexts;
        public Toggle toggle;

        protected override void SetModel(PickerOptionCellModel model)
        {
            contentsCanvasGroup.alpha = model.Interactable ? 1.0f : 0.3f;

            // Cleanup
            toggle.onValueChanged.RemoveAllListeners();

            // Icon
            icon.Setup(model.Icon);
            icon.gameObject.SetActive(model.Icon.Sprite != null);

            // Texts
            cellTexts.Setup(model.CellTexts);

            // Toggle
            toggle.interactable = model.Interactable;
            toggle.SetIsOnWithoutNotify(model.IsOn);
            toggle.onValueChanged.AddListener(x =>
            {
                model.IsOn = x;
                model.InvokeToggled(x);
            });

            // Height
            var height = model.UseSubTextOrIcon ? 68 : 42; // Texts
            height += 36; // Padding
            height += 1; // Border
            contents.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            layoutElement.preferredHeight = height; // Set the preferred height for the recycler view.
        }
    }

    public sealed class PickerOptionCellModel : CellModel
    {
        public PickerOptionCellModel(bool useSubTextOrIcon) { UseSubTextOrIcon = useSubTextOrIcon; }

        public CellIconModel Icon { get; } = new CellIconModel();

        public bool UseSubTextOrIcon { get; }

        public bool IsOn { get; set; }

        public bool Interactable { get; set; } = true;

        public CellTextsModel CellTexts { get; } = new CellTextsModel();

        public event Action<bool> Toggled;

        internal void InvokeToggled(bool isOn) { Toggled?.Invoke(isOn); }
    }
}