using Pancake.Linq;
using PlayFab.ClientModels;
using UnityEngine;

namespace Pancake.GameService
{
    [RequireComponent(typeof(PlayfabSilentLogin))]
    [DisallowMultipleComponent]
    public class SilentLogin : MonoBehaviour
    {
        private PlayfabSilentLogin _playfabSilentLogin;

        private void Awake()
        {
            _playfabSilentLogin = GetComponent<PlayfabSilentLogin>();
            _playfabSilentLogin.onLoginSuccess.RemoveAllListeners();
            _playfabSilentLogin.onLoginSuccess.AddListener(OnLoginSuccess);
        }


        private void OnLoginSuccess(LoginResult result)
        {
            var r = result.InfoResultPayload.PlayerProfile;
            var countryCode = "";
            foreach (var location in r.Locations)
            {
                countryCode = location.CountryCode.ToString();
            }

            //var r2 = result.InfoResultPayload.PlayerStatistics;
            LoginResultModel.Init(r.PlayerId, r.DisplayName, countryCode, r.LinkedAccounts.Any(_ => _.Platform == LoginIdentityProvider.Facebook));
            // if (result.NewlyCreated || !AuthService.Instance.IsCompleteSetupName)
            // {
            //     Popup.Show<PopupEnterName>();
            // }
            // else
            // {
            //     // goto menu
            // }
        }
    }
}