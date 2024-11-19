using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Pancake.Notification
{
    [EditorIcon("icon_notification")]
    public class NotificationPrepare : GameComponent
    {
#if UNITY_ANDROID
        [SerializeField] private ScriptableNotification[] notificationVariables;

        private CancellationToken _token;

        private void Start()
        {
            _token = destroyCancellationToken;
            var strs = new List<string>();

            foreach (var variable in notificationVariables)
            {
                if (!variable.bigPicture) continue;
                if (!strs.Contains(variable.namePicture)) strs.Add(variable.namePicture);
            }

            foreach (string s in strs)
            {
                PrepareImage(Application.persistentDataPath, s);
            }
        }

        private async void PrepareImage(string destDir, string filename)
        {
            string path = Path.Combine(destDir, filename);
            if (File.Exists(path)) return;
            using var www = UnityWebRequest.Get(Path.Combine(Application.streamingAssetsPath, filename));
            try
            {
                await Awaitable.FromAsyncOperation(www.SendWebRequest(), _token);
                await File.WriteAllBytesAsync(path, www.downloadHandler.data, _token);
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }
#endif
    }
}