using System.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
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
        [SerializeField] private string bucket = "masterdata";
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonGpgs;
        [SerializeField] private GameObject block;

        [SerializeField, PopupPickup] private string popupNotification;
        [SerializeField, PopupPickup] private string popupQuestion;
        [SerializeField] private LocaleText localeLoginFail;
        [SerializeField] private LocaleText localeLinkedGooglePlayGameFail;
        [SerializeField] private LocaleText localeBackupSuccess;
        [SerializeField] private LocaleText localeRestoreSuccess;

        private string _serverCode;
        private bool _isError;
        private bool _isBackup;
        private PopupContainer _popupContainer;

        protected override UniTask Initialize()
        {
            _isError = false;
            _serverCode = "";
            _popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
            PlayGamesPlatform.Activate();

            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonGpgs.onClick.AddListener(OnButtonGpgsPressed);
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

        private async UniTask GpgsRestore() { }

        private async UniTask GpgsBackup()
        {
            LoginGooglePlayGames();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await UniTask.WaitUntil(() => !string.IsNullOrEmpty(_serverCode) || _isError);

            if (_isError)
            {
                await _popupContainer.Push<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.SetMessage(localeLoginFail));
                return;
            }

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
                            AuthenticationService.Instance.SignOut();
                            await SignInWithGooglePlayGamesAsync(_serverCode);
                        }
                    });
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

            // save process
            byte[] inputBytes = Data.Backup();
            await SaveFileBytes(bucket, inputBytes);

            await _popupContainer.Push<NotificationPopup>(popupNotification, true, onLoad: tuple => tuple.popup.view.SetMessage(localeBackupSuccess));
        }

        private async void OnButtonApplePressed() { }

        private void LoginGooglePlayGames()
        {
            PlayGamesPlatform.Instance.Authenticate((success) =>
            {
                if (success == SignInStatus.Success)
                {
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => _serverCode = code);
                }
                else
                {
                    _isError = true;
                }
            });
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