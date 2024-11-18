using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pancake.Common;
using Pancake.Localization;
using Pancake.SignIn;
using Pancake.UI;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public sealed class ChoosePlatformLoginView : View
    {
        [Space] [SerializeField] private string bucket = "masterdata";
        [Space] [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonGpgs;
        [SerializeField] private Button buttonApple;
        [SerializeField] private GameObject block;

        [SerializeField, PopupPickup] private string popupNotification;
        [SerializeField, PopupPickup] private string popupQuestion;
        [SerializeField] private LocaleText localeLoginGpgsFail;
        [SerializeField] private LocaleText localeLoginAppleFail;
        [SerializeField] private LocaleText localeBackupSuccess;
        [SerializeField] private LocaleText localeRestoreSuccess;

        private bool _isBackup;

        protected override UniTask Initialize()
        {
#if UNITY_ANDROID && PANCAKE_GPGS
            buttonGpgs.onClick.AddListener(OnButtonGpgsPressed);
            buttonGpgs.gameObject.SetActive(true);
            buttonApple.gameObject.SetActive(false);
#elif UNITY_IOS
            buttonApple.onClick.AddListener(OnButtonApplePressed);
            buttonGpgs.gameObject.SetActive(false);
            buttonApple.gameObject.SetActive(true);
#endif

            buttonClose.onClick.AddListener(OnButtonClosePressed);

            return UniTask.CompletedTask;
        }

        public void Setup(bool backup) { _isBackup = backup; }

        #region gpgs

        private async void OnButtonGpgsPressed()
        {
            block.SetActive(true);
            if (_isBackup)
            {
                await GpgsBackup();
                return;
            }

            await GpgsRestore();
        }

        private async UniTask GpgsRestore()
        {
            if (!AuthenticationGooglePlayGames.IsSignIn())
            {
                SignInEvent.status = false;
                SignInEvent.Login();
                await UniTask.WaitUntil(() => SignInEvent.status);

                if (string.IsNullOrEmpty(SignInEvent.ServerCode))
                {
                    await MainUIContainer.In.GetMain<PopupContainer>()
                        .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeLoginGpgsFail, OnButtonClosePressed));
                    return;
                }
            }
            else
            {
                SignInEvent.status = false;
                SignInEvent.GetNewServerCode();
                await UniTask.WaitUntil(() => SignInEvent.status);
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                // signin cached
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(SignInEvent.ServerCode);
            }

            await FetchData();
            return;

            async Task FetchData()
            {
                // save process
                byte[] inputBytes = await BackupDataHelper.LoadFileBytes(bucket);
                Data.Restore(inputBytes);

                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeRestoreSuccess, ActionOk));
                return;

                async void ActionOk()
                {
                    TurnOffBlock();
                    PlaySoundClose();
                    await PopupHelper.Close(transform);
                    ReloadMenu();
                }
            }
        }

        private async UniTask GpgsBackup()
        {
            if (!AuthenticationGooglePlayGames.IsSignIn())
            {
                SignInEvent.status = false;
                SignInEvent.Login();
                await UniTask.WaitUntil(() => SignInEvent.status);

                if (string.IsNullOrEmpty(SignInEvent.ServerCode))
                {
                    await MainUIContainer.In.GetMain<PopupContainer>()
                        .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeLoginGpgsFail, OnButtonClosePressed));
                    return;
                }
            }
            else
            {
                SignInEvent.status = false;
                SignInEvent.GetNewServerCode();
                await UniTask.WaitUntil(() => SignInEvent.status);
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // signin cached
            }
            else
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(SignInEvent.ServerCode);
            }

            await PushData();
            return;

            async Task PushData()
            {
                // save process
                byte[] inputBytes = Data.Backup();
                await BackupDataHelper.SaveFileBytes(bucket, inputBytes);

                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeBackupSuccess, TurnOffBlock));
            }
        }

        #endregion

        #region apple

        private async void OnButtonApplePressed()
        {
            block.SetActive(true);
            if (_isBackup)
            {
                await AppleBackup();
                return;
            }

            await AppleRestore();
        }

        private async UniTask AppleRestore()
        {
            SignInEvent.status = false;
            SignInEvent.Login();
            await UniTask.WaitUntil(() => SignInEvent.status);

            if (string.IsNullOrEmpty(SignInEvent.ServerCode))
            {
                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeLoginGpgsFail, OnButtonClosePressed));
                return;
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // signin cached
            }
            else
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(SignInEvent.ServerCode);
            }

            await FetchData();
            return;

            async Task FetchData()
            {
                // load process
                byte[] inputBytes = await BackupDataHelper.LoadFileBytes(bucket);
                Data.Restore(inputBytes);

                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeRestoreSuccess, ActionOk));
                return;

                async void ActionOk()
                {
                    TurnOffBlock();
                    PlaySoundClose();
                    await PopupHelper.Close(transform);
                    ReloadMenu();
                }
            }
        }

        private async UniTask AppleBackup()
        {
            SignInEvent.status = false;
            SignInEvent.Login();
            await UniTask.WaitUntil(() => SignInEvent.status);

            if (string.IsNullOrEmpty(SignInEvent.ServerCode))
            {
                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeLoginAppleFail, OnButtonClosePressed));
                return;
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // signin cached
            }
            else
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(SignInEvent.ServerCode);
            }

            await PushData();
            return;

            async Task PushData()
            {
                // save process
                byte[] inputBytes = Data.Backup();
                await BackupDataHelper.SaveFileBytes(bucket, inputBytes);

                await MainUIContainer.In.GetMain<PopupContainer>()
                    .PushAsync<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.Setup(localeBackupSuccess, TurnOffBlock));
            }
        }

        #endregion

        private async void ReloadMenu()
        {
            await PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER)
                .PushAsync<TransitionPopup>(nameof(TransitionPopup), false, onLoad: t => { t.popup.view.Setup(); }); // show transition
            await SceneLoader.LoadScene(Constant.Scene.MENU);
        }

        private void TurnOffBlock() { block.SetActive(false); }


        private void OnButtonClosePressed()
        {
            TurnOffBlock();
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}