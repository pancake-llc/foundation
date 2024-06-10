using System.Text;
using System.Threading.Tasks;
#if UNITY_IOS && PANCAKE_APPLE_SIGNIN
using AppleAuth;
using AppleAuth.Native;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
#endif

namespace Pancake.AppleSignIn
{
    using UnityEngine;

    public class AuthenticationApple : MonoBehaviour
    {
        //[SerializeField] private StringVariable serverCode;
        //[SerializeField] private StringVariable userId;
        //[SerializeField] private BoolVariable status;
        //[SerializeField] private ScriptableEventNoParam loginEvent;
#if UNITY_IOS && PANCAKE_APPLE_SIGNIN
        private IAppleAuthManager _appleAuthManager;
#endif

#if UNITY_IOS && PANCAKE_APPLE_SIGNIN
        private void Start()
        {
            serverCode.Value = "";
            if (AppleAuthManager.IsCurrentPlatformSupported)
            {
                var deserializer = new PayloadDeserializer();
                _appleAuthManager = new AppleAuthManager(deserializer);
                loginEvent.OnRaised += OnAppleLogin;
            }
        }

        private async void OnAppleLogin() { (serverCode.Value, userId.Value, status.Value) = await LoginApple(); }

        private void Update() { _appleAuthManager?.Update(); }

        private Task<(string, string, bool)> LoginApple()
        {
            var taskSource = new TaskCompletionSource<(string, string, bool)>();
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            _appleAuthManager.LoginWithAppleId(loginArgs,
                credential =>
                {
                    if (credential is IAppleIDCredential appleIdCredential)
                    {
                        taskSource.SetResult((Encoding.UTF8.GetString(appleIdCredential.IdentityToken, 0, appleIdCredential.IdentityToken.Length), appleIdCredential.User,
                            true));
                    }
                    else taskSource.SetResult(("", "", true));
                },
                error => { taskSource.SetResult(("", "", true)); });
            return taskSource.Task;
        }
#endif
    }
}