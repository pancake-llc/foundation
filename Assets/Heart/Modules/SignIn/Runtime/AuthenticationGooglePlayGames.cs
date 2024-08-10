using System;
using System.Threading.Tasks;
#if UNITY_ANDROID && PANCAKE_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using UnityEngine;

namespace Pancake.SignIn
{
    public class AuthenticationGooglePlayGames : MonoBehaviour
    {
#if UNITY_ANDROID && PANCAKE_GPGS
        private void Start()
        {
            SignInEvent.ServerCode = "";
            PlayGamesPlatform.Activate();
        }

        private void OnEnable()
        {
            SignInEvent.LoginEvent += OnGooglePlayGameLogin;
            SignInEvent.GetNewServerCodeEvent += OnGetNewServerCode;
        }

        private async void OnGetNewServerCode()
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated()) return;
            (SignInEvent.ServerCode, SignInEvent.status) = await GetNewServerCode();
        }

        private Task<(string, bool)> GetNewServerCode()
        {
            var taskSource = new TaskCompletionSource<(string, bool)>();
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => taskSource.SetResult((code, true)));
            return taskSource.Task;
        }

        private void OnDisable()
        {
            SignInEvent.LoginEvent -= OnGooglePlayGameLogin;
            SignInEvent.GetNewServerCodeEvent -= OnGetNewServerCode;
        }

        private async void OnGooglePlayGameLogin() { (SignInEvent.ServerCode, SignInEvent.status) = await LoginGooglePlayGames(); }

        private Task<(string, bool)> LoginGooglePlayGames()
        {
            var taskSource = new TaskCompletionSource<(string, bool)>();
            PlayGamesPlatform.Instance.Authenticate(status =>
            {
                if (status == SignInStatus.Success)
                {
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => taskSource.SetResult((code, true)));
                }
                else
                {
                    PlayGamesPlatform.Instance.ManuallyAuthenticate(success =>
                    {
                        if (success == SignInStatus.Success)
                        {
                            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => taskSource.SetResult((code, true)));
                        }
                        else
                        {
                            taskSource.SetResult(("", true));
                        }
                    });
                }
            });
            return taskSource.Task;
        }

#endif
        public static bool IsSignIn()
        {
#if UNITY_ANDROID && PANCAKE_GPGS
            return PlayGamesPlatform.Instance.IsAuthenticated();
#else
            return false;
#endif
        }
    }
}