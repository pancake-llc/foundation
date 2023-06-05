using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pancake.Apex;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

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
            public string title;
            public string message;

            public NotificationData(string title, string message)
            {
                this.title = title;
                this.message = message;
            }
        }

        [SerializeField, Guid] private string identifier;
        [SerializeField] private int minute;
        [SerializeField] private bool repeat;
        [SerializeField] private bool bigPicture;

        [ShowIf(nameof(bigPicture)), Message("File bigpicture muest be place in folder StreamingAsset"), Label("  Name Picture")] [SerializeField]
        private string namePicture;

        [Array, SerializeField] private NotificationData[] datas;

        public async void Send()
        {
            var data = datas.PickRandom();
            if (!state)
            {
                PrepareImages();
                await UniTask.WaitUntil(() => state);
                namePicture = picturePath;
                Debug.Log("PICTURE PATH:" + picturePath);
            }
            NotificationConsole.Send(identifier,
                data.title,
                data.message,
                bigPicture: bigPicture,
                namePicture: namePicture);
        }

        public async void Schedule()
        {
            var data = datas.PickRandom();
            if (!state)
            {
                PrepareImages();
                await UniTask.WaitUntil(() => state);
                namePicture = picturePath;
            }

            NotificationConsole.Schedule(identifier,
                data.title,
                data.message,
                TimeSpan.FromMinutes(minute),
                bigPicture: bigPicture,
                namePicture: namePicture,
                repeat: repeat);
        }


        string picturePath;
        private bool state;

        void PrepareImages()
        {
            string filename = "channel_manual_picture.jpg";
            picturePath = Path.Combine(Application.persistentDataPath, filename);
            App.RunCoroutine(PrepareImage(Application.persistentDataPath, filename));
        }

        IEnumerator PrepareImage(string destDir, string filename)
        {
            string path = Path.Combine(destDir, filename);
            if (File.Exists(path))
                yield break;
            using var uwr = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, filename));
            yield return uwr.SendWebRequest();
            File.WriteAllBytes(path, uwr.downloadHandler.data);
            state = true;
        }
    }
}