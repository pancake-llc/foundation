using System;

namespace Pancake.Notification
{
    internal static class NotificationConsole
    {
        internal static void Send(
            string identifier,
            string title,
            string text,
            string largeIcon = null,
            string channelName = "Nova",
            string channelDescription = "Newsletter Announcement",
            string smallIcon = null,
            bool bigPicture = false,
            string namePicture = "")
        {
            Schedule(identifier,
                title,
                text,
                DateTime.Now + TimeSpan.FromMilliseconds(250),
                largeIcon,
                channelName,
                channelDescription,
                smallIcon,
                bigPicture,
                namePicture);
        }

        internal static void Schedule(
            string identifier,
            string title,
            string text,
            TimeSpan timeOffset,
            string largeIcon = null,
            string channelName = "Nova",
            string channelDescription = "Newsletter Announcement",
            string smallIcon = null,
            bool bigPicture = false,
            string namePicture = "", bool repeat = false)
        {
            Schedule(identifier,
                title,
                text,
                DateTime.Now + timeOffset,
                largeIcon,
                channelName,
                channelDescription,
                smallIcon,
                bigPicture,
                namePicture, repeat);
        }

        private static void Schedule(
            string identifier,
            string title,
            string text,
            DateTime fireTime,
            string largeIcon = null,
            string channelName = "Nova",
            string channelDescription = "Newsletter Announcement",
            string smallIcon = null,
            bool bigPicture = false,
            string namePicture = "",
            bool repeat = false)
        {
            if (string.IsNullOrEmpty(smallIcon)) smallIcon = "icon_0";
            if (string.IsNullOrEmpty(largeIcon)) largeIcon = "icon_1";

#if UNITY_ANDROID

            Unity.Notifications.Android.BigPictureStyle? bigPictureStyle = null;
            if (bigPicture)
            {
                bigPictureStyle = new Unity.Notifications.Android.BigPictureStyle() {Picture = namePicture, ContentTitle = "Picture"};
            }

            NotificationAndroid.Schedule(identifier,
                title,
                text,
                fireTime,
                largeIcon,
                channelName,
                channelDescription,
                smallIcon,
                bigPictureStyle, repeat);
#elif UNITY_IOS
			NotificationIOS.Schedule(identifier, title, "", text, fireTime, repeat);
#endif
        }

        internal static void CancelAllScheduled()
        {
#if UNITY_ANDROID
            NotificationAndroid.CancelAllScheduled();
#elif UNITY_IOS
			NotificationIOS.CancelAllScheduled();
#endif
        }

        internal static void ClearBadgeCounteriOS()
        {
#if UNITY_IOS
			NotificationIOS.ClearBadgeCounter();
#endif
        }
    }
}