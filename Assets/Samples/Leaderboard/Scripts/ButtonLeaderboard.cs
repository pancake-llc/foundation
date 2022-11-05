using System;
using Pancake.Linq;
using Pancake.UI;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.GameService
{
    [RequireComponent(typeof(UIButton))]
    public class ButtonLeaderboard : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private PopupLeaderboard popupLeaderboard;
        [SerializeField] private PopupEnterName popupEnterName;
        [SerializeField] private PopupNoInternet popupNoInternet;
        [SerializeField] private PopupNotification popupNotification;
        
        private UIButton _button;
        [SerializeField] private GameObject block;
        [SerializeField] private string nameTable;
        public UnityEvent<LoginResult> onLoginSuccess;
        public Func<int> valueExpression;

        public GameObject Block { get => block; set => block = value; }

        private void Awake() { _button = GetComponent<UIButton>(); }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
            AuthService.Instance.infoRequestParams = ServiceSettings.InfoRequestParams;
            AuthService.OnLoginSuccess += AuthServiceOnLoginSuccess;
            AuthService.OnLoginError += AuthServiceOnError;
        }

        private void AuthServiceOnError(PlayFabError error)
        {
            #region replace your code show popup notification

            popupNotification.Message($"Login failed.\nError code: {error.Error}\nPlease try again later!");
            popupNotification.gameObject.SetActive(false);

            #endregion

        }

        private void AuthServiceOnLoginSuccess(LoginResult result)
        {
            var r = result.InfoResultPayload.PlayerProfile;
            if (r == null && result.NewlyCreated) // new account need once more login
            {
                AuthService.Instance.Authenticate(EAuthType.Silent);
                return;
            }

            Block.SetActive(false);
            var countryCode = "";
            // ReSharper disable once PossibleNullReferenceException
            foreach (var location in r.Locations)
            {
                countryCode = location.CountryCode.ToString();
            }

            bool condition = result.NewlyCreated || !AuthService.Instance.IsCompleteSetupName;
            if (!condition)
            {
                foreach (var userData in result.InfoResultPayload.UserData)
                {
                    if (userData.Key.Equals(ServiceSettings.INTERNAL_CONFIG_KEY))
                    {
                        var config = PlayFabSimpleJson.DeserializeObject<InternalConfig>(userData.Value.Value);
                        if (config != null) countryCode = config.countryCode;
                    }
                }
            }


            LoginResultModel.Init(r.PlayerId, r.DisplayName, countryCode, r.LinkedAccounts.Any(_ => _.Platform == LoginIdentityProvider.Facebook));
            if (result.NewlyCreated || !AuthService.Instance.IsCompleteSetupName)
            {
                #region replace your code show popup enter name

                popupEnterName.gameObject.SetActive(true);
                popupEnterName.Init(nameTable,
                    valueExpression,
                    () =>
                    {
                        popupEnterName.gameObject.SetActive(false);
                        popupLeaderboard.gameObject.SetActive(true);
                        popupLeaderboard.Init(canvas);
                    });

                #endregion
            }
            else
            {
                // todo display show popup leaderboard

                #region replace your code to show poup in here

                popupLeaderboard.gameObject.SetActive(true);
                popupLeaderboard.Init(canvas);

                #endregion
            }
        }

        private void OnButtonClicked()
        {
            _button.interactable = false;
            C.CheckConnection(_ =>
            {
                _button.interactable = true;
                if (_ == ENetworkStatus.Connected)
                {
                    Block.SetActive(true);
                    AuthService.Instance.Authenticate(EAuthType.Silent);
                }
                else
                {
                    #region replace you code show popup no internet

                    popupNoInternet.gameObject.SetActive(true);

                    #endregion
                }
            });
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            AuthService.OnLoginSuccess -= AuthServiceOnLoginSuccess;
            AuthService.OnLoginError -= AuthServiceOnError;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ButtonLeaderboard))]
    public class ButtonLeaderboardEdior : UnityEditor.Editor
    {
        private ButtonLeaderboard _buttonLeaderboard;

        private void OnEnable() { _buttonLeaderboard = target as ButtonLeaderboard; }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Setup Fetch")) SetupFetch();

            if (_buttonLeaderboard.Block != null) _buttonLeaderboard.Block.transform.GetChild(0).transform.position = Vector3.zero;

            void SetupFetch()
            {
                if (_buttonLeaderboard.Block == null)
                {
                    var obj = new GameObject("Block") {gameObject = {layer = LayerMask.NameToLayer("UI")}};
                    obj.transform.SetParent(_buttonLeaderboard.transform, false);
                    _buttonLeaderboard.Block = obj;

                    var r = obj.AddComponent<RectTransform>();

                    r.FullScreen();
                    r.sizeDelta = new Vector2(10000, 10000);
                    var img = obj.AddComponent<Image>();
                    img.color = new Color(0f, 0f, 0f, 0.78f);
                    var canvas = obj.AddComponent<Canvas>();
                    canvas.overrideSorting = true;

                    var fetch = new GameObject("Fetch") {gameObject = {layer = LayerMask.NameToLayer("UI")}};
                    fetch.transform.SetParent(obj.transform, false);
                    var fetchRectransform = fetch.AddComponent<RectTransform>();
                    fetchRectransform.sizeDelta = new Vector2(150, 150);
                    fetch.AddComponent<Image>();
                    SetupFetch2(fetchRectransform);
                    obj.SetActive(false);
                }
            }

            void SetupFetch2(RectTransform fetch)
            {
                Sprite first = (Sprite) AssetDatabase.LoadAssetAtPath("Packages/com.pancake.ui/Runtime/Fetch/Sprites/01.png", typeof(Sprite));
                if (first == null) first = (Sprite) AssetDatabase.LoadAssetAtPath("Assets/_Root/Runtime/Fetch/Sprites/01.png", typeof(Sprite));
                var img = fetch.GetComponent<Image>();
                img.sprite = first;
                img.raycastTarget = false;
                var animator = fetch.gameObject.AddComponent<Animator>();

                animator.runtimeAnimatorController = (UnityEditor.Animations.AnimatorController) AssetDatabase.LoadAssetAtPath(
                    "Packages/com.pancake.ui/Runtime/Fetch/Animation/FetchAnimator.controller",
                    typeof(UnityEditor.Animations.AnimatorController));

                if (animator.runtimeAnimatorController == null)
                {
                    animator.runtimeAnimatorController =
                        (UnityEditor.Animations.AnimatorController) AssetDatabase.LoadAssetAtPath("Assets/_Root/Runtime/Fetch/Animation/FetchAnimator.controller",
                            typeof(UnityEditor.Animations.AnimatorController));
                }

                if (animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("Can not found FetchAnimator!");
                }
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
#endif
}