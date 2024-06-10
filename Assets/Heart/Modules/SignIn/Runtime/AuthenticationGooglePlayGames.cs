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
        // [SerializeField] private StringVariable serverCode;
        // [SerializeField] private BoolVariable status;
        // [SerializeField] private ScriptableEventNoParam loginEvent;
        // [SerializeField] private ScriptableEventNoParam getNewServerCode;

#if UNITY_ANDROID && PANCAKE_GPGS
        private void Start()
        {
            serverCode.Value = "";
            PlayGamesPlatform.Activate();
            loginEvent.OnRaised += OnGooglePlayGameLogin;
            getNewServerCode.OnRaised += OnGetNewServerCode;
        }

        private async void OnGetNewServerCode()
        {
            if (!PlayGamesPlatform.Instance.IsAuthenticated()) return;
            (serverCode.Value, status.Value) = await GetNewServerCode();
        }

        private Task<(string, bool)> GetNewServerCode()
        {
            var taskSource = new TaskCompletionSource<(string, bool)>();
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => taskSource.SetResult((code, true)));
            return taskSource.Task;
        }

        private void OnDisable()
        {
            loginEvent.OnRaised -= OnGooglePlayGameLogin;
            getNewServerCode.OnRaised -= OnGetNewServerCode;
        }

        private async void OnGooglePlayGameLogin() { (serverCode.Value, status.Value) = await LoginGooglePlayGames(); }

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