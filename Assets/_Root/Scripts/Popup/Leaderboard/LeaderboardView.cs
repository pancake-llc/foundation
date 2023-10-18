using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class LeaderboardView : View
    {
        [SerializeField] private string tableId;
        [SerializeField] private IntVariable currentLevel;
        [SerializeField] private Button buttonClose;
        [SerializeField] private GameObject waitLoginObject;
        [SerializeField, PopupPickup] private string popupRename;

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            InternalInit();
            return UniTask.CompletedTask;
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private async void InternalInit()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                waitLoginObject.SetActive(true);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await LeaderboardsService.Instance.AddPlayerScoreAsync(tableId, currentLevel.Value);
                waitLoginObject.SetActive(false);
                Debug.Log("Player Id:" + AuthenticationService.Instance.PlayerId);

                if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName)) ShowPopupRename();
                else
                {
                    Debug.Log(AuthenticationService.Instance.PlayerName);
                }
            }
            else
            {
                // todo
            }
        }

        private void ShowPopupRename() { PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).Push<RenamePopup>(popupRename, true); }
    }
}