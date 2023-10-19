using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Coffee.UIEffects;
using Newtonsoft.Json;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using PrimeTween;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Pancake.UI
{
    public sealed class RenameView : View, IEnhancedScrollerDelegate
    {
        public class NameList
        {
            public string[] names;
        }

        [SerializeField] private TextAsset namesAsset;
        [SerializeField] private CountryCollection countryCollection;
        [SerializeField] private CountryElementView countryElementPrefab;
        [SerializeField] private EnhancedScroller countryScroller;
        [SerializeField] private RectTransform countryPopup;
        [SerializeField] private TMP_InputField inputFieldName;
        [SerializeField] private UIButton buttonSelectCountry;
        [SerializeField] private Image imageIconCountrySelected;
        [SerializeField] private TextMeshProUGUI textNameCountrySelected;
        [SerializeField] private TextMeshProUGUI textMessage;
        [SerializeField] private GameObject objectStatusOk;
        [SerializeField] private GameObject objectBlock;
        [SerializeField] private Button buttonOk;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonDice;

        private SmallList<int> _datas;
        private bool _isVerifySuccess;
        private UIEffect _buttonVerifyEffect;
        private string _selectedCountry;
        private bool _firstTimeActiveCountry;
        private string _userPickName;
        private RectTransform _countryScrollerRT;
        private Action _onCloseCallback;
        private NameList _nameList;

        public void SetCallbackClose(Action onCloseCallback) { _onCloseCallback = onCloseCallback; }

        protected override UniTask Initialize()
        {
            _nameList = JsonConvert.DeserializeObject<NameList>(namesAsset.text);
            _buttonVerifyEffect = buttonOk.GetComponent<UIEffect>();
            _countryScrollerRT = countryScroller.GetComponent<RectTransform>();
            countryScroller.Delegate = this;
            inputFieldName.characterLimit = 13;
            inputFieldName.onValueChanged.AddListener(OnInputNameValueChanged);
            inputFieldName.onSelect.AddListener(OnInputNameSelected);
            inputFieldName.text = "";
            inputFieldName.ActivateInputField();
            inputFieldName.Select();

            textMessage.gameObject.SetActive(false);
            var currentCountryData = countryCollection.Get(RegionInfo.CurrentRegion.TwoLetterISORegionName);
            imageIconCountrySelected.sprite = currentCountryData.icon;
            imageIconCountrySelected.color = Color.white;
            textNameCountrySelected.text = currentCountryData.name;
            _selectedCountry = currentCountryData.code.ToString();

            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonOk.onClick.AddListener(OnButtonOkPressed);
            buttonSelectCountry.onClick.AddListener(OnButtonSelectCountryPressed);
            buttonDice.onClick.AddListener(OnButtonDicePressed);

            return UniTask.CompletedTask;
        }

        private void OnInputNameSelected(string value)
        {
            if (countryPopup.gameObject.activeInHierarchy) InternalHideSelectCountry();
        }

        private void OnButtonDicePressed()
        {
            string randomName = _nameList.names.PickRandom();
            int number = Random.Range(1, 99);
            inputFieldName.text = $"{randomName}{number}";
        }

        private void OnButtonSelectCountryPressed()
        {
            if (buttonSelectCountry.AffectObject.localEulerAngles.z.Equals(0))
            {
                Tween.LocalRotation(buttonSelectCountry.AffectObject.transform, Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 90), 0.3f);
                InternalShowSelectCountry();

                if (!_firstTimeActiveCountry)
                {
                    _firstTimeActiveCountry = true;
                    InitCountryData();
                }
            }
            else
            {
                Tween.LocalRotation(buttonSelectCountry.AffectObject.transform, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f);
                InternalHideSelectCountry();
            }
        }

        private void InitCountryData()
        {
            _datas = new SmallList<int>();
            for (int i = 0; i < countryCollection.Collection.Length; i++)
            {
                _datas.Add(i);
            }

            countryScroller.ReloadData();
        }

        private void InternalShowSelectCountry()
        {
            textMessage.gameObject.SetActive(false);
            _countryScrollerRT.pivot = new Vector2(0.5f, 1f);
            countryPopup.gameObject.SetActive(true);
            buttonOk.interactable = false;
            countryPopup.SetSizeDeltaY(103);
            Tween.UISizeDelta(countryPopup, new Vector2(countryPopup.sizeDelta.x, 666), 0.5f)
                .OnComplete(() =>
                {
                    countryScroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Always;
                    buttonOk.interactable = true;
                    _countryScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                });
        }

        private void InternalHideSelectCountry()
        {
            textMessage.gameObject.SetActive(false);
            _countryScrollerRT.pivot = new Vector2(0.5f, 1f);
            buttonOk.interactable = false;
            countryScroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Never;

            Tween.UISizeDelta(countryPopup, new Vector2(countryPopup.sizeDelta.x, 103f), 0.5f)
                .OnComplete(() =>
                {
                    countryPopup.gameObject.SetActive(false);
                    bool state = inputFieldName.text.Length < 13 || inputFieldName.text.Length >= 3;
                    countryScroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Always;
                    buttonOk.interactable = state;
                    textMessage.gameObject.SetActive(!state);
                    _countryScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                });
            Tween.LocalRotation(buttonSelectCountry.AffectObject.transform, Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f);
        }

        private async void OnButtonOkPressed()
        {
            objectBlock.SetActive(true);
            _userPickName = inputFieldName.text;
            _userPickName += $"#{_selectedCountry}";
            await AuthenticationService.Instance.UpdatePlayerNameAsync(_userPickName);
            objectBlock.SetActive(false);
            objectStatusOk.SetActive(false);
            OnButtonClosePressed();
        }

        private void OnButtonClosePressed()
        {
            _onCloseCallback?.Invoke();
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void OnInputNameValueChanged(string value)
        {
            if (value.Length >= 13)
            {
                buttonOk.interactable = false;
                _buttonVerifyEffect.effectMode = EffectMode.Grayscale;
                DisplayWarning("Name length cannot be longer than 12 characters!");
            }
            else
            {
                if (value.Length < 3)
                {
                    buttonOk.interactable = false;
                    _buttonVerifyEffect.effectMode = EffectMode.Grayscale;
                }
                else
                {
                    textMessage.gameObject.SetActive(false);
                    buttonOk.interactable = true;
                    _buttonVerifyEffect.effectMode = EffectMode.None;
                }
            }
        }

        private void DisplayWarning(string message)
        {
            textMessage.gameObject.SetActive(true);
            textMessage.text = message;
            Tween.PunchScale(textMessage.transform, new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 5);
        }

        public int GetNumberOfCells(EnhancedScroller scroller) { return _datas.Count; }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) { return 120f; }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var element = scroller.GetCellView(countryElementPrefab) as CountryElementView;
            if (element != null)
            {
                var code = (ECountryCode) dataIndex;
                element.name = "Country_" + code;
                element.Init(_datas[dataIndex],
                    OnButtonElementClicked,
                    IsElementSelected,
                    countryCollection.Get,
                    InternalHideSelectCountry);
                return element;
            }

            return null;
        }

        private void OnButtonElementClicked(CountryElementView view)
        {
            _selectedCountry = view.CountryData.code.ToString();
            _userPickName = inputFieldName.text;
            countryScroller.RefreshActiveCellViews();
            imageIconCountrySelected.sprite = view.CountryData.icon;
            imageIconCountrySelected.color = Color.white;
            textNameCountrySelected.text = view.CountryData.name;
        }

        private bool IsElementSelected(string code) { return _selectedCountry.Equals(code); }
    }
}