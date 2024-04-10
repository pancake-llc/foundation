using System;
using Pancake.Common;
using Pancake.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class CountryElementCell : Cell<CountryElementCellModel>
    {
        [SerializeField] private Image imageIcon;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private GameObject objectHightLight;
        [SerializeField] private Button buttonSelect;

        private Tween _tween;

        protected override void SetModel(CountryElementCellModel model)
        {
            imageIcon.sprite = model.CountryData.icon;
            textName.text = model.CountryData.name;

            buttonSelect.onClick.RemoveAllListeners();
            buttonSelect.onClick.AddListener(() =>
            {
                model.OnClickedAction.Invoke(model.CountryData);
                ActiveTween(model.OnHideSelectCountryAction);
            });

            ActiveTween(() => { });
            return;

            void ActiveTween(Action action)
            {
                _tween.Stop();
                objectHightLight.SetActive(model.IsSelected.Invoke(model.CountryData.code.ToString()));
                if (objectHightLight.activeInHierarchy)
                {
                    objectHightLight.transform.SetScaleX(0);
                    _tween = Tween.ScaleX(objectHightLight.transform, 1f, 0.25f).OnComplete(action.Invoke);
                }
            }
        }
    }

    public sealed class CountryElementCellModel : CellModel
    {
        public CountryData CountryData { get; set; }
        public Action<CountryData> OnClickedAction { get; set; }
        public Func<string, bool> IsSelected { get; set; }
        public Action OnHideSelectCountryAction { get; set; }
    }
}