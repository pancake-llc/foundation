using System;
using System.IO;
using Pancake.Apex;
using Pancake.Common;
using Pancake.Localization;
using UnityEngine;

namespace Pancake.Notification
{
    [Serializable]
    [Searchable]
    [EditorIcon("scriptable_notification")]
    [CreateAssetMenu(fileName = "notification_channel_data.asset", menuName = "Pancake/Misc/Notification Channel")]
    public class ScriptableNotificationVariable : ScriptableObject
    {
        [Serializable]
        public class NotificationData
        {
            [HideIf(nameof(enableLocalization))] public string title;
            [HideIf(nameof(enableLocalization))] public string message;
            public bool enableLocalization;
            [ShowIf(nameof(enableLocalization))] public LocaleText titleLocale;
            [ShowIf(nameof(enableLocalization))] public LocaleText messageLocale;

            public NotificationData(string title, string message)
            {
                this.title = title;
                this.message = message;
            }
        }

        [SerializeField, Guid] private string identifier;
        public int minute;
        [SerializeField] private bool repeat;
        [SerializeField] internal bool bigPicture;

        [ShowIf(nameof(bigPicture)), Message("File bigpicture must be place in folder StreamingAsset", Height = 24),
         Message("Name Picture must contains file extension ex .jpg", Height = 24), Label("  Name Picture")]
        [SerializeField]
        internal string namePicture;

        [SerializeField] internal bool overrideIcon;

        [SerializeField, ShowIf(nameof(overrideIcon)), Label("  Small Icon")]
        internal string smallIcon = "icon_0";

        [SerializeField, ShowIf(nameof(overrideIcon)), Label("  Large Icon")]
        internal string largeIcon = "icon_1";


        [Array, SerializeField] private NotificationData[] datas;


        public void Send()
        {
            if (!Application.isMobilePlatform) return;
            var data = datas.PickRandom();
            string pathPicture = Path.Combine(Application.persistentDataPath, namePicture);
            string title;
            string message;
            if (data.enableLocalization)
            {
                title = data.titleLocale.Value;
                message = data.messageLocale.Value;
            }
            else
            {
                title = data.title;
                message = data.message;
            }

            NotificationConsole.Send(identifier,
                title,
                message,
                smallIcon: smallIcon,
                largeIcon: largeIcon,
                bigPicture: bigPicture,
                namePicture: pathPicture);
        }

        public void Schedule()
        {
            if (!Application.isMobilePlatform) return;
            var data = datas.PickRandom();

            string pathPicture = Path.Combine(Application.persistentDataPath, namePicture);
            string title;
            string message;
            if (data.enableLocalization)
            {
                title = data.titleLocale.Value;
                message = data.messageLocale.Value;
            }
            else
            {
                title = data.title;
                message = data.message;
            }

            NotificationConsole.Schedule(identifier,
                title,
                message,
                TimeSpan.FromMinutes(minute),
                smallIcon: smallIcon,
                largeIcon: largeIcon,
                bigPicture: bigPicture,
                namePicture: pathPicture,
                repeat: repeat);
        }
    }
}