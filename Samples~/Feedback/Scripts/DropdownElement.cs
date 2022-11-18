using System;
using Pancake.Tween;
using TMPro;
using UnityEngine.UI;

namespace Pancake.Feedback
{
    using UnityEngine;

    public class DropdownElement : MonoBehaviour
    {
        [field: SerializeField] public int Index { get; private set; }
        [field: SerializeField] public TextMeshProUGUI Label { get; private set; }
        [field: SerializeField] public GameObject Selected { get; private set; }
        [SerializeField] private Button btnSelect;

        private ITween _tweenSelect;
        private Func<int, bool> _isSelected;

        private Action<DropdownElement> _onClicked;

        public void Init(Action<DropdownElement> onClicked, Func<int, bool> isSelected)
        {
            _isSelected = isSelected;
            _onClicked = onClicked;

            btnSelect.onClick.RemoveAllListeners();
            btnSelect.onClick.AddListener(() => { _onClicked?.Invoke(this); });

            RefreshView();
        }

        public void RefreshView() { Selected.SetActive(_isSelected.Invoke(Index)); }
    }
}