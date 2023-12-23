using System;
using Pancake.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class CountryElementView : EnhancedScrollerCellView
    {
        [SerializeField] private Image imageIcon;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private GameObject objectHightLight;
        [SerializeField] private Button buttonSelect;

        private Func<string, bool> _funcSelected;
        private Action<CountryElementView> _onClicked;
        private Action _onClickedAdvance;
        private Tween _tween;

        public CountryData CountryData { get; private set; }

        public void Init(
            int countryCode,
            Action<CountryElementView> onClicked,
            Func<string, bool> funcSelected,
            Func<string, CountryData> getCountryData,
            Action onClickedAdvance)
        {
            _funcSelected = funcSelected;
            _onClicked = onClicked;
            _onClickedAdvance = onClickedAdvance;
            var code = ((ECountryCode) countryCode).ToString();
            CountryData = getCountryData?.Invoke(code);
            if (CountryData == null)
            {
                Debug.Log("Can not get country data with country code = " + countryCode);
                return;
            }

            imageIcon.sprite = CountryData.icon;
            textName.text = CountryData.name;
            buttonSelect.onClick.RemoveListener(OnButtonSelectPressed);
            buttonSelect.onClick.AddListener(OnButtonSelectPressed);
            objectHightLight.SetActive(_funcSelected.Invoke(code));
        }

        private void OnButtonSelectPressed() { _onClicked?.Invoke(this); }

        public override void RefreshCellView()
        {
            _tween.Stop();
            objectHightLight.SetActive(_funcSelected.Invoke(CountryData.code.ToString()));
            if (objectHightLight.activeInHierarchy)
            {
                objectHightLight.transform.SetScaleX(0);
                _tween = Tween.ScaleX(objectHightLight.transform, 1f, 0.25f).OnComplete(() => _onClickedAdvance?.Invoke());
            }
        }
    }
}