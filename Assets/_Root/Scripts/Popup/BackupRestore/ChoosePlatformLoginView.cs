using System.Threading.Tasks;
using Pancake.SignIn;
using Pancake.Localization;
using Pancake.Scriptable;
using Cysharp.Threading.Tasks;
using Pancake.UI;
using Pancake.Common;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
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

        [Header("Authen")] [SerializeField] private StringVariable serverCode;
        [SerializeField] private BoolVariable status;
        [SerializeField] private ScriptableEventNoParam loginEvent;
        [SerializeField] private ScriptableEventNoParam gpgsGetNewServerCode;
        [SerializeField] private ScriptableEventString changeSceneEvent;

        private bool _isBackup;
        private PopupContainer _popupContainer;

        protected override UniTask Initialize()
        {
            _popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
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
                status.Value = false;
                loginEvent.Raise();
                await UniTask.WaitUntil(() => status.Value);

                if (string.IsNullOrEmpty(serverCode.Value))
                {
                    await _popupContainer.Push<NotificationPopup>(popupNotification,
                        true,
                        onLoad: tuple =>
                        {
                            tuple.popup.view.SetMessage(localeLoginGpgsFail);
                            tuple.popup.view.SetAction(OnButtonClosePressed);
                        });
                    return;
                }
            }
            else
            {
                status.Value = false;
                gpgsGetNewServerCode.Raise();
                await UniTask.WaitUntil(() => status.Value);
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                // signin cached
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(serverCode.Value);
            }

            await FetchData();
            return;

            async Task FetchData()
            {
                // save process
                byte[] inputBytes = await LoadFileBytes(bucket);
                Data.Restore(inputBytes);

                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeRestoreSuccess);
                        tuple.popup.view.SetAction(ActionOk);
                        return;

                        async void ActionOk()
                        {
                            TurnOffBlock();
                            PlaySoundClose();
                            await PopupHelper.Close(transform);
                            ReloadMenu();
                        }
                    });
            }
        }

        private async UniTask GpgsBackup()
        {
            if (!AuthenticationGooglePlayGames.IsSignIn())
            {
                status.Value = false;
                loginEvent.Raise();
                await UniTask.WaitUntil(() => status.Value);

                if (string.IsNullOrEmpty(serverCode.Value))
                {
                    await _popupContainer.Push<NotificationPopup>(popupNotification,
                        true,
                        onLoad: tuple =>
                        {
                            tuple.popup.view.SetMessage(localeLoginGpgsFail);
                            tuple.popup.view.SetAction(OnButtonClosePressed);
                        });
                    return;
                }
            }
            else
            {
                status.Value = false;
                gpgsGetNewServerCode.Raise();
                await UniTask.WaitUntil(() => status.Value);
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                // signin cached
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(serverCode.Value);
            }

            await PushData();
            return;

            async Task PushData()
            {
                // save process
                byte[] inputBytes = Data.Backup();
                await SaveFileBytes(bucket, inputBytes);

                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeBackupSuccess);
                        tuple.popup.view.SetAction(TurnOffBlock);
                    });
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
            status.Value = false;
            loginEvent.Raise();
            await UniTask.WaitUntil(() => status.Value);

            if (string.IsNullOrEmpty(serverCode.Value))
            {
                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeLoginGpgsFail);
                        tuple.popup.view.SetAction(OnButtonClosePressed);
                    });
                return;
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                // signin cached
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(serverCode.Value);
            }

            await FetchData();
            return;

            async Task FetchData()
            {
                // save process
                byte[] inputBytes = await LoadFileBytes(bucket);
                Data.Restore(inputBytes);

                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeRestoreSuccess);
                        tuple.popup.view.SetAction(ActionOk);
                        return;

                        async void ActionOk()
                        {
                            TurnOffBlock();
                            PlaySoundClose();
                            await PopupHelper.Close(transform);
                            ReloadMenu();
                        }
                    });
            }
        }

        private async UniTask AppleBackup()
        {
            status.Value = false;
            loginEvent.Raise();
            await UniTask.WaitUntil(() => status.Value);

            if (string.IsNullOrEmpty(serverCode.Value))
            {
                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeLoginAppleFail);
                        tuple.popup.view.SetAction(OnButtonClosePressed);
                    });
                return;
            }

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                // signin cached
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                await AuthenticationService.Instance.SignInWithAppleAsync(serverCode.Value);
            }

            await PushData();
            return;

            async Task PushData()
            {
                // save process
                byte[] inputBytes = Data.Backup();
                await SaveFileBytes(bucket, inputBytes);

                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeBackupSuccess);
                        tuple.popup.view.SetAction(TurnOffBlock);
                    });
            }
        }

        #endregion

        private async void ReloadMenu()
        {
            await PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER)
                .Push<SceneTransitionPopup>(nameof(SceneTransitionPopup),
                    false,
                    onLoad: t => { t.popup.view.Setup(); },
                    popupId: nameof(SceneTransitionPopup)); // show transition
            changeSceneEvent.Raise(Constant.MENU_SCENE);
        }

        private void TurnOffBlock() { block.SetActive(false); }

        private async Task SaveFileBytes(string key, byte[] bytes)
        {
            try
            {
                await CloudSaveService.Instance.Files.Player.SaveAsync(key, bytes);
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveRateLimitedException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        private async Task<byte[]> LoadFileBytes(string key)
        {
            try
            {
                byte[] results = await CloudSaveService.Instance.Files.Player.LoadBytesAsync(key);
                return results;
            }
            catch (CloudSaveValidationException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveRateLimitedException e)
            {
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        private void OnButtonClosePressed()
        {
            TurnOffBlock();
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}