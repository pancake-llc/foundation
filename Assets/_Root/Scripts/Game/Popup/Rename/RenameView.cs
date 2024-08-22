using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Newtonsoft.Json;
using Pancake.Common;
using Pancake.Linq;
using Pancake.Pools;
using Pancake.UI;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Pancake.Game.UI
{
    public sealed class RenameView : View, IRecyclerViewCellProvider, IRecyclerViewDataProvider
    {
        public class NameList
        {
            // ReSharper disable once UnassignedField.Global
            public string[] names;
        }

        [SerializeField] private GameObject countryElementPrefab;
        [SerializeField] private TextAsset namesAsset;
        [SerializeField] private CountryIconAsset countryIconAsset;
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

        private bool _isVerifySuccess;
        private string _selectedCountry;
        private bool _firstTimeActiveCountry;
        private string _userPickName;
        private RectTransform _countryScrollerRT;
        private Action<bool> _onCloseCallback;
        private NameList _nameList;
        private RecyclerView _recyclerView;
        private readonly List<CountryElementCellModel> _datas = new();

        private RecyclerView RecyclerView
        {
            get
            {
                if (_recyclerView == null)
                {
                    _recyclerView = countryPopup.GetComponentInChildren<RecyclerView>();
                    _recyclerView.DataCount = 0;
                    _recyclerView.CellProvider = this;
                    _recyclerView.DataProvider = this;
                }

                return _recyclerView;
            }
        }

        public void SetCallbackClose(Action<bool> onCloseCallback) { _onCloseCallback = onCloseCallback; }

        protected override UniTask Initialize()
        {
            // Add padding for the safe area.
            float canvasScaleFactor = GetComponentInParent<Canvas>().scaleFactor;
            RecyclerView.AfterPadding += (int) (Screen.safeArea.y / canvasScaleFactor);

            _nameList = JsonConvert.DeserializeObject<NameList>(namesAsset.text);
            _countryScrollerRT = RecyclerView.GetComponent<RectTransform>();
            inputFieldName.characterLimit = 13;
            inputFieldName.onValueChanged.AddListener(OnInputNameValueChanged);
            inputFieldName.onSelect.AddListener(OnInputNameSelected);
            inputFieldName.text = "";
            inputFieldName.ActivateInputField();
            inputFieldName.Select();

            textMessage.gameObject.SetActive(false);
            var currentCountryData = countryIconAsset.Get(RegionInfo.CurrentRegion.TwoLetterISORegionName);
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
                LMotion.Create(Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 90), 0.3f).BindToLocalRotation(buttonSelectCountry.AffectObject);
                InternalShowSelectCountry();

                if (!_firstTimeActiveCountry)
                {
                    _firstTimeActiveCountry = true;
                    InitCountryData();
                }
            }
            else
            {
                LMotion.Create(Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f).BindToLocalRotation(buttonSelectCountry.AffectObject);
                InternalHideSelectCountry();
            }
        }

        private void InitCountryData()
        {
            _datas.Clear();
            for (var i = 0; i < countryIconAsset.Countries.Length; i++)
            {
                _datas.Add(new CountryElementCellModel
                {
                    CountryData = countryIconAsset.Countries[i],
                    IsSelected = IsElementSelected,
                    OnClickedAction = OnButtonElementClicked,
                    OnHideSelectCountryAction = InternalHideSelectCountry
                });
                RecyclerView.DataCount++;
            }

            RecyclerView.RefreshData();
        }

        private void InternalShowSelectCountry()
        {
            textMessage.gameObject.SetActive(false);
            _countryScrollerRT.pivot = new Vector2(0.5f, 1f);
            countryPopup.gameObject.SetActive(true);
            buttonOk.interactable = false;
            countryPopup.SetSizeDeltaY(103);
            RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(false);
            LMotion.Create(countryPopup.sizeDelta, new Vector2(countryPopup.sizeDelta.x, 666), 0.5f)
                .WithOnComplete(() =>
                {
                    RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(true);
                    buttonOk.interactable = true;
                    _countryScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                })
                .BindToSizeDelta(countryPopup);
        }

        private void InternalHideSelectCountry()
        {
            textMessage.gameObject.SetActive(false);
            _countryScrollerRT.pivot = new Vector2(0.5f, 1f);
            buttonOk.interactable = false;
            RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(false);
            LMotion.Create(countryPopup.sizeDelta, new Vector2(countryPopup.sizeDelta.x, 103f), 0.5f)
                .WithOnComplete(() =>
                {
                    countryPopup.gameObject.SetActive(false);
                    bool state = inputFieldName.text.Length < 13 || inputFieldName.text.Length >= 3;
                    RecyclerView.ScrollRect.verticalScrollbar.handleRect.gameObject.SetActive(true);
                    buttonOk.interactable = state;
                    textMessage.gameObject.SetActive(!state);
                    _countryScrollerRT.pivot = new Vector2(0.5f, 0.5f);
                })
                .BindToSizeDelta(countryPopup);
            LMotion.Create(Quaternion.Euler(0, 0, 90), Quaternion.Euler(0, 0, 0), 0.3f).BindToLocalRotation(buttonSelectCountry.AffectObject);
        }

        private async void OnButtonOkPressed()
        {
            objectBlock.SetActive(true);
            _userPickName = inputFieldName.text;
            _userPickName += $"#{_selectedCountry}";
            await AuthenticationService.Instance.UpdatePlayerNameAsync(_userPickName);
            objectBlock.SetActive(false);
            objectStatusOk.SetActive(false);
            PlaySoundClose();
            await PopupHelper.Close(transform);
            _onCloseCallback?.Invoke(false);
        }

        private async void OnButtonClosePressed()
        {
            PlaySoundClose();
            await PopupHelper.Close(transform);
            _onCloseCallback?.Invoke(true);
        }

        private void OnInputNameValueChanged(string value)
        {
            if (value.Length >= 13)
            {
                buttonOk.interactable = false;
                DisplayWarning("Name length cannot be longer than 12 characters!");
            }
            else
            {
                if (value.Length < 3) buttonOk.interactable = false;
                else
                {
                    textMessage.gameObject.SetActive(false);
                    buttonOk.interactable = true;
                }
            }
        }

        private void DisplayWarning(string message)
        {
            textMessage.gameObject.SetActive(true);
            textMessage.text = message;
            LMotion.Punch.Create(textMessage.transform.localScale, new Vector3(0.1f, 0.1f, 0.1f), 0.2f).BindToLocalScale(textMessage.transform);
        }

        private void OnButtonElementClicked(CountryData countryData)
        {
            _selectedCountry = countryData.code.ToString();
            _userPickName = inputFieldName.text;
            imageIconCountrySelected.sprite = countryData.icon;
            imageIconCountrySelected.color = Color.white;
            textNameCountrySelected.text = countryData.name;
        }

        private bool IsElementSelected(string code) { return _selectedCountry.Equals(code); }

        public GameObject GetCell(int dataIndex) { return countryElementPrefab.Request(); }

        public void ReleaseCell(GameObject cell) { cell.Return(); }

        public void SetupCell(int dataIndex, GameObject cell) { cell.GetComponent<ICell>().Setup(_datas[dataIndex]); }
    }
}