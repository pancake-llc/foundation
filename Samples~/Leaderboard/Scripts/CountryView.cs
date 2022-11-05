using System;
using Pancake.Tween;
using Pancake.UI;
using TMPro;
using UnityEngine.UI;

namespace Pancake.GameService
{
    using UnityEngine;

    public class CountryView : EnhancedScrollerCellView
    {
        [SerializeField] private Image countryIcon;
        [SerializeField] private TextMeshProUGUI countryName;
        [SerializeField] private GameObject selectedHightLight;
        [SerializeField] private Button btnSelect;

        private CountryCodeData _data;
        private ITween _tweenSelect;

        public Image CountryIcon => countryIcon;
        public TextMeshProUGUI CountryName => countryName;
        public CountryCodeData Data => _data;
        private Func<string, bool> _isSelected;

        private Action _onClickedAdvance;
        private Action<CountryView> _onClicked;

        public void Init(CountryData data, Action<CountryView> onClicked, Func<string, bool> isSelected, Func<string, CountryCodeData> get, Action onClickedAdvance)
        {
            _isSelected = isSelected;
            _onClicked = onClicked;
            _onClickedAdvance = onClickedAdvance;
            _data = get?.Invoke(((ECountryCode) data.id).ToString());
            if (Data == null)
            {
                Debug.Log("Can not get country code data with id = " + data.id);
                return;
            }

            countryIcon.sprite = Data.icon;
            countryName.text = Data.name;

            btnSelect.onClick.RemoveAllListeners();
            btnSelect.onClick.AddListener(() => { _onClicked?.Invoke(this); });

            selectedHightLight.SetActive(_isSelected.Invoke(Data.code.ToString()));
        }

        public override void RefreshCellView()
        {
            _tweenSelect?.Kill();
            selectedHightLight.SetActive(_isSelected.Invoke(Data.code.ToString()));
            if (selectedHightLight.activeInHierarchy)
            {
                selectedHightLight.transform.SetLocalScale(x: 0);
                _tweenSelect = selectedHightLight.transform.TweenLocalScaleX(1, 0.25f).OnComplete(() => _onClickedAdvance?.Invoke());
                _tweenSelect.Play();
            }
        }
    }
}