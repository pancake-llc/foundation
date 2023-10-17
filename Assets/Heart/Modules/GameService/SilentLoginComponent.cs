using Pancake.Scriptable;
using Pancake.Threading.Tasks;
#if PANCAKE_AUTH
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif
using UnityEngine;

namespace Pancake.Services
{
    [EditorIcon("csharp")]
    public class SilentLoginComponent : GameComponent
    {
        [SerializeField] private BoolVariable isServiceInitialized;

        private void Start() { Init(); }

        private async void Init()
        {
            await UniTask.WaitUntil(() => isServiceInitialized.Value);

#if PANCAKE_AUTH
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                AuthenticationService.Instance.SignInFailed += OnSignInFailed;
                await AuthenticationService.Instance.SignInAnonymouslyAsync(new SignInOptions() {CreateAccount = true});
            }
#endif
        }

#if PANCAKE_AUTH
        private void OnSignInFailed(RequestFailedException e) { Debug.Log(e.Message); }

        private void OnSignedIn() { }
#endif
    }
}