using System;
using System.Collections;
using Unity.Notifications.Android;
using Unity.Notifications.iOS;
using UnityEngine;

namespace Pancake.Notification
{
    [EditorIcon("scriptable_noti")]
    public class NotificationConsole : GameComponent
    {
        private const string DEFAULT_FILE_NAME = "notifications.bin";

        [SerializeField] private ScriptableNotificationVariable[] channels;

        private void Start() { }

        public IEnumerator Initialize()
        {
#if UNITY_ANDROID
            yield return RequestNotificationPermissionAndroid();
#elif UNITY_IOS
            yield return RequestNotificationPermissioniOS();
#endif
            yield return null;
        }

        IEnumerator RequestNotificationPermissionAndroid()
        {
            if (AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed)
            {
                var request = new PermissionRequest();
                while (request.Status == PermissionStatus.RequestPending) yield return null;
            }

            yield return null;
        }

        public IEnumerator RequestNotificationPermissioniOS()
        {
            using (var request = new AuthorizationRequest(AuthorizationOption.Badge | AuthorizationOption.Sound | AuthorizationOption.Alert, false))
            {
                while (!request.IsFinished) yield return null;
            }
        }
    }
}