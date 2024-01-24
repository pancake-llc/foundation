using System;
using System.Threading.Tasks;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using Pancake.Localization;
using Pancake.Threading.Tasks;
using Pancake.UI;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
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
        [SerializeField] private LocaleText localeLoginFail;
        [SerializeField] private LocaleText localeLinkedGooglePlayGameFail;
        [SerializeField] private LocaleText localeBackupSuccess;
        [SerializeField] private LocaleText localeRestoreSuccess;

        private string _serverCode;
        private bool _isBackup;
        private PopupContainer _popupContainer;

        protected override UniTask Initialize()
        {
            _serverCode = "";
            _popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
#if UNITY_ANDROID
            buttonGpgs.onClick.AddListener(OnButtonGpgsPressed);
            buttonGpgs.gameObject.SetActive(true);
            buttonApple.gameObject.SetActive(false);
            PlayGamesPlatform.Activate();
#elif UNITY_IOS
            buttonGpgs.gameObject.SetActive(false);
            buttonApple.onClick.AddListener(OnButtonApplePressed);
            buttonApple.gameObject.SetActive(true);
#endif

            buttonClose.onClick.AddListener(OnButtonClosePressed);

            return UniTask.CompletedTask;
        }

        public void Setup(bool backup) { _isBackup = backup; }

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

        private async UniTask GpgsRestore() { LoginGooglePlayGames(); }

        private async UniTask GpgsBackup()
        {
            _serverCode = await LoginGooglePlayGames();
            
            if (string.IsNullOrEmpty(_serverCode))
            {
                await _popupContainer.Push<NotificationPopup>(popupNotification,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeLoginFail);
                        tuple.popup.view.SetAction(TurnOffBlock);
                    });
                return;
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            try
            {
                await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(_serverCode);
                // Link is successful.
            }
            catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                await _popupContainer.Push<QuestionPopup>(popupQuestion,
                    true,
                    onLoad: tuple =>
                    {
                        tuple.popup.view.SetMessage(localeLinkedGooglePlayGameFail);
                        tuple.popup.view.SetAction(Ok, null);
                        return;

                        async void Ok()
                        {
                            AuthenticationService.Instance.SignOut(true);
                            await SignInWithGooglePlayGamesAsync(_serverCode);
                            if (AuthenticationService.Instance.SessionTokenExists) await AuthenticationService.Instance.SignInAnonymouslyAsync(); 
                            await PushData();
                        }
                    });
                return;
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }

            await PushData();
            return;

            async Task PushData()
            {
                Debug.Log(AuthenticationService.Instance.PlayerId);
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

        private void TurnOffBlock() { block.SetActive(false); }

        private async void OnButtonApplePressed() { }

        private Task<string> LoginGooglePlayGames()
        {
            var taskSource = new TaskCompletionSource<string>();
            PlayGamesPlatform.Instance.Authenticate((success) =>
            {
                if (success == SignInStatus.Success)
                {
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => taskSource.SetResult(code));
                }
                else
                {
                    PlayGamesPlatform.Instance.ManuallyAuthenticate(success =>
                    {
                        if (success == SignInStatus.Success)
                        {
                            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => taskSource.SetResult(code));
                        }
                        else
                        {
                            taskSource.SetResult(null);
                        }
                    });
                }
            });
            return taskSource.Task;
        }

        private async Task SignInWithGooglePlayGamesAsync(string authCode)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
                // SignIn is successful.
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

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
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}