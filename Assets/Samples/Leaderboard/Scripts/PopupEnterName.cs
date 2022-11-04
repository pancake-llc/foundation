using System;
using Pancake.Tween;
using Pancake.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.GameService
{
    public class PopupEnterName : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private CountryCode countryCode;
        [SerializeField] private CountryView elementPrefab;
        [SerializeField] private Sprite btnSpriteLocked;
        [SerializeField] private TMP_InputField ipfEnterName;
        [SerializeField] private UIButton btnCountry;
        [SerializeField] private UIButton btnOk;
        [SerializeField] private EnhancedScroller scroller;
        [SerializeField] private TextMeshProUGUI txtWarning;
        [SerializeField] private Image imgCurrentCountryIcon;
        [SerializeField] private TextMeshProUGUI txtCurrentCountryName;
        [SerializeField] private Transform block;
        [SerializeField] private RectTransform selectCountryPopup;
        [SerializeField] private RectTransform container;

        private SmallList<CountryData> _data;
        private bool _firstTime;
        private ITween _tween;
        private string _selectedCountry;
        private string _userName;
        private ISequence _sequenceTxtWarning;
        private Sprite _defaultSprite;
        private Func<int> _valueExpression;
        private Action _actionShowPopupLeaderboard;
        private string _nameTable;

        private void Start()
        {
            scroller.Delegate = this;
            ipfEnterName.characterLimit = 17;
            ipfEnterName.onValueChanged.AddListener(OnInputNameCallback);
            ipfEnterName.text = "";
            ipfEnterName.ActivateInputField();
            ipfEnterName.Select();
            btnCountry.onClick.RemoveListener(OnButtonShowPopupCountryClicked);
            btnCountry.onClick.AddListener(OnButtonShowPopupCountryClicked);
            btnOk.onClick.RemoveListener(OnButtonOkClicked);
            btnOk.onClick.AddListener(OnButtonOkClicked);
            txtWarning.gameObject.SetActive(false);
            var countryData = countryCode.Get(LoginResultModel.countryCode);
            imgCurrentCountryIcon.sprite = countryData.icon;
            imgCurrentCountryIcon.color = Color.white;
            txtCurrentCountryName.text = countryData.name;
            _defaultSprite = btnOk.image.sprite;
            block.gameObject.SetActive(false);
            _selectedCountry = LoginResultModel.countryCode;
        }

        /// <summary>
        /// use for init expression get value
        /// </summary>
        /// <param name="nameTable"></param>
        /// <param name="valueExpression"></param>
        /// <param name="actionShowPopupLeaderboard">must be include close current popup and show popup leaderboard</param>
        public void Init(string nameTable, Func<int> valueExpression, Action actionShowPopupLeaderboard)
        {
            _nameTable = nameTable;
            _valueExpression = valueExpression;
            _actionShowPopupLeaderboard = actionShowPopupLeaderboard;
        }

        protected virtual void OnEnable()
        {
            AuthService.OnUpdateUserTitleDisplayNameSuccess += OnUpdateNameCallbackCompleted;
            AuthService.OnUpdateUserTitleDisplayNameError += OnUpdateNameCallbackError;
        }

        protected virtual void OnDisable()
        {
            AuthService.OnUpdateUserTitleDisplayNameSuccess -= OnUpdateNameCallbackCompleted;
            AuthService.OnUpdateUserTitleDisplayNameError -= OnUpdateNameCallbackError;
        }

        protected virtual void OnUpdateNameCallbackCompleted(UpdateUserTitleDisplayNameResult success)
        {
            AuthService.UpdateUserData(ServiceSettings.INTERNAL_CONFIG_KEY,
                ServiceSettings.Get(_selectedCountry),
                UserDataPermission.Public,
                OnUpdateInternalConfigCompleted,
                OnUpdateInternalConfigError);
            LoginResultModel.playerDisplayName = success.DisplayName;
        }

        protected virtual void OnUpdateNameCallbackError(PlayFabError error)
        {
            block.gameObject.SetActive(false);
            DisplayWarning(error.ErrorMessage);

            if (error.Error == PlayFabErrorCode.NameNotAvailable) ipfEnterName.Select();
        }

        protected virtual void OnUpdateInternalConfigCompleted(UpdateUserDataResult success)
        {
            AuthService.Instance.IsCompleteSetupName = true;
            LoginResultModel.countryCode = _selectedCountry;
            AuthService.OnUpdatePlayerStatisticsSuccess += AuthServiceOnUpdatePlayerStatisticsSuccess;
            if (_valueExpression != null) AuthService.UpdatePlayerStatistics(_nameTable, _valueExpression.Invoke());
            else
            {
                AuthService.OnUpdatePlayerStatisticsSuccess -= AuthServiceOnUpdatePlayerStatisticsSuccess;
                block.gameObject.SetActive(false);
                //Popup.Close(); // close current popup enter name
                //Popup.Show<PopupLeaderboard>();
                _actionShowPopupLeaderboard?.Invoke();
            }
        }

        protected virtual void AuthServiceOnUpdatePlayerStatisticsSuccess(UpdatePlayerStatisticsResult success)
        {
            AuthService.OnUpdatePlayerStatisticsSuccess -= AuthServiceOnUpdatePlayerStatisticsSuccess;
            block.gameObject.SetActive(false);
            //Popup.Close(); // close current popup enter name
            //Popup.Show<PopupLeaderboard>();
            _actionShowPopupLeaderboard?.Invoke();
        }

        private void OnUpdateInternalConfigError(PlayFabError error)
        {
            block.gameObject.SetActive(false);
            DisplayWarning(error.ErrorMessage);
        }

        protected virtual void OnButtonOkClicked()
        {
            if (string.IsNullOrEmpty(ipfEnterName.text))
            {
                DisplayWarning("Name cannot be blank!");
                block.gameObject.SetActive(false);
                ipfEnterName.Select();
                return;
            }

            btnOk.interactable = false;
            block.gameObject.SetActive(true);
            txtWarning.gameObject.SetActive(false);

            string str = ipfEnterName.text.Trim();
            AuthService.UpdateUserTitleDisplayName(str);
        }

        protected virtual void OnInputNameCallback(string value)
        {
            if (value.Length >= 16)
            {
                btnOk.interactable = false;
                btnOk.image.sprite = btnSpriteLocked;
                if (!txtWarning.gameObject.activeSelf) DisplayWarning("Name length cannot be longer than 16 characters!");
            }
            else
            {
                txtWarning.gameObject.SetActive(false);
                btnOk.interactable = true;
                btnOk.image.sprite = _defaultSprite;
            }
        }

        private void DisplayWarning(string message)
        {
            txtWarning.gameObject.SetActive(true);
            txtWarning.text = message;
            _sequenceTxtWarning?.Kill();
            _sequenceTxtWarning = TweenManager.Sequence();
            _sequenceTxtWarning.Append(txtWarning.transform.TweenLocalScale(new Vector3(1.1f, 1.1f, 1.1f), 0.12f).SetEase(Ease.Parabolic));
            _sequenceTxtWarning.Append(txtWarning.transform.TweenLocalScale(new Vector3(1f, 1f, 1f), 0.12f).SetEase(Ease.Linear));
            _sequenceTxtWarning.Play();
        }

        private void OnButtonShowPopupCountryClicked()
        {
            if (btnCountry.AffectObject.localEulerAngles.z.Equals(0))
            {
                btnCountry.AffectObject.TweenLocalRotationZ(90, 0.3f, RotationMode.Beyond360).Play();
                InternalShowSelectCountry();
                if (!_firstTime)
                {
                    _firstTime = true;
                    InitData();
                }
            }
            else
            {
                btnCountry.AffectObject.TweenLocalRotationZ(0, 0.3f, RotationMode.Beyond360).Play();
                InternalHideSelectCountry();
            }
        }

        private void InitData()
        {
            _data = new SmallList<CountryData>();
            for (var i = 0; i < countryCode.countryCodeDatas.Length; i++)
            {
                _data.Add(new CountryData() {id = i});
            }

            scroller.ReloadData();
        }

        private void InternalShowSelectCountry()
        {
            txtWarning.gameObject.SetActive(false);
            container.SetPivot(new Vector2(0.5f, 1f));
            selectCountryPopup.gameObject.SetActive(true);
            btnOk.interactable = false;
            selectCountryPopup.sizeDelta = selectCountryPopup.sizeDelta.Change(y: 103);
            _tween?.Kill();
            _tween = selectCountryPopup.TweenSizeDeltaY(666, 0.5f);
            _tween.SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    scroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Always;
                    btnOk.interactable = true;
                    container.SetPivot(new Vector2(0.5f, 0.5f));
                })
                .Play();
            container.TweenSizeDeltaY(1206f, 0.5f).Play();
        }

        private void InternalHideSelectCountry()
        {
            container.SetPivot(new Vector2(0.5f, 1f));
            scroller.ScrollbarVisibility = EnhancedScroller.ScrollbarVisibilityEnum.Never;
            btnOk.interactable = false;
            _tween?.Kill();
            _tween = selectCountryPopup.TweenSizeDeltaY(103f, 0.5f);
            _tween.SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    selectCountryPopup.gameObject.SetActive(false);
                    btnOk.interactable = true;
                    container.SetPivot(new Vector2(0.5f, 0.5f));
                })
                .Play();
            container.TweenSizeDeltaY(940f, 0.5f).Play();
            btnCountry.AffectObject.TweenLocalRotationZ(0, 0.3f, RotationMode.Beyond360).Play();
        }

        #region implement

        public int GetNumberOfCells(EnhancedScroller scroller) { return _data.Count; }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) { return 120; }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var element = scroller.GetCellView(elementPrefab) as CountryView;
            if (element != null)
            {
                var code = (ECountryCode) dataIndex;
                element.name = "Country_" + code;
                element.Init(_data[dataIndex],
                    OnButtonElementCountryClicked,
                    IsElementSelected,
                    countryCode.Get,
                    InternalHideSelectCountry);
                return element;
            }

            return null;
        }

        private void OnButtonElementCountryClicked(CountryView view)
        {
            _selectedCountry = view.Data.code.ToString();
            _userName = ipfEnterName.text;
            scroller.RefreshActiveCellViews();
            imgCurrentCountryIcon.sprite = view.Data.icon;
            imgCurrentCountryIcon.color = Color.white;
            txtCurrentCountryName.text = view.Data.name;
        }

        private bool IsElementSelected(string code) { return _selectedCountry.Equals(code); }

        #endregion
    }
}