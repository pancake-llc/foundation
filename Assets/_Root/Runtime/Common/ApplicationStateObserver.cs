using UnityEngine;

namespace Pancake
{
    public class ApplicationStateObserver : MonoBehaviour
    {
        public event System.Action OnQuit = null;

#if !UNITY_IOS && !UNITY_ANDROID
		private void OnApplicationQuit() => OnQuit?.Invoke();
#else
        private void OnApplicationPause(bool pause)
        {
            if (pause) OnQuit?.Invoke();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) OnQuit?.Invoke();
        }
#endif
    }
}