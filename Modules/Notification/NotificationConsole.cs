using System.Collections;
using Pancake.Apex;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;

namespace Pancake.Notification
{
    /// <summary>
    /// Schedule immediately
    /// </summary>
    [EditorIcon("scriptable_noti")]
    public class NotificationConsole : GameComponent
    {
        [SerializeField, Array] private ScriptableNotificationVariable[] channels;

        public IEnumerator Start()
        {
            var group = new AndroidNotificationChannelGroup() {Id = channels[0].groupId, Name = "Master"};
            AndroidNotificationCenter.RegisterNotificationChannelGroup(group);

            foreach (var channel in channels)
            {
                channel.Init();
            }

#if UNITY_ANDROID
            yield return StartCoroutine(RequestNotificationPermissionAndroid());
#elif UNITY_IOS
            yield return StartCoroutine(RequestNotificationPermissioniOS());
#endif

            foreach (var channel in channels)
            {
                channel.AutoUpdateDeliveryTime();
            }
        }

#if UNITY_ANDROID
        IEnumerator RequestNotificationPermissionAndroid()
        {
            if (AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed)
            {
                var request = new PermissionRequest();
                while (request.Status == PermissionStatus.RequestPending) yield return null;
            }

            yield return null;
        }
#endif

#if UNITY_IOS
        public IEnumerator RequestNotificationPermissioniOS()
        {
            using (var request = new AuthorizationRequest(AuthorizationOption.Badge | AuthorizationOption.Sound | AuthorizationOption.Alert, false))
            {
                while (!request.IsFinished) yield return null;
            }
        }
#endif

        public void CancelAllSchedule() { }
    }
}