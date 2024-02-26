using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.DebugView
{
    public sealed class SearchFieldCell : Cell<SearchFieldCellModel>
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Text placeholderText;

        protected override void SetModel(SearchFieldCellModel model)
        {
            // Cleanup
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onEndEdit.RemoveAllListeners();

            inputField.interactable = model.Interactable;
            inputField.onValueChanged.AddListener(model.InvokeValueChanged);
            inputField.onEndEdit.AddListener(model.InvokeSubmitted);

            placeholderText.text = model.Placeholder;
        }
    }

    public sealed class SearchFieldCellModel : CellModel
    {
        public bool Interactable { get; set; } = true;

        public string Placeholder { get; set; } = "Search";

        public event Action<string> ValueChanged;

        public event Action<string> Submitted;

        internal void InvokeValueChanged(string searchText) { ValueChanged?.Invoke(searchText); }

        internal void InvokeSubmitted(string searchText) { Submitted?.Invoke(searchText); }
    }
}