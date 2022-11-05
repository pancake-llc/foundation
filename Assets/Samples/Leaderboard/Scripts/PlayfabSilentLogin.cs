using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.GameService
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class PlayfabSilentLogin : MonoBehaviour
    {
        public UnityEvent<LoginResult> onLoginSuccess;

        private void OnEnable()
        {
            AuthService.Instance.infoRequestParams = ServiceSettings.InfoRequestParams;
            AuthService.OnLoginSuccess += AuthServiceOnLoginSuccess;
        }

        private void Start() { AuthService.Instance.Authenticate(EAuthType.Silent); }

        private void AuthServiceOnLoginSuccess(LoginResult success) { onLoginSuccess?.Invoke(success); }

        private void OnDisable() { AuthService.OnLoginSuccess -= AuthServiceOnLoginSuccess; }
    }
}