using System;
using System.Threading.Tasks;
using Pancake.Apex;
#if PANCAKE_REMOTE_CONFIG
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
#endif
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Tracking
{
    public class RemoteConfig : GameComponent
    {
        [SerializeField, Label("Status")] private BoolVariable remoteConfigIsFetchCompleted;
        [SerializeField] private RemoteConfigData remoteData;

#if PANCAKE_REMOTE_CONFIG
        private void Start()
        {
            remoteConfigIsFetchCompleted.Value = false;
            FirebaseApp.CheckDependenciesAsync()
                .ContinueWith(task =>
                {
                    var status = task.Result;
                    if (status == DependencyStatus.Available)
                    {
                        var app = FirebaseApp.DefaultInstance;
                        FetchAsync();
                    }
                });
        }

        /// <summary>
        /// Start a fetch request.
        /// FetchAsync only fetches new data if the current data is older than the provided
        /// timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
        /// By default the timespan is 12 hours, and for production apps, this is a good
        /// number. For this example though, it's set to a timespan of zero, so that
        /// changes in the console will always show up immediately.
        /// </summary>
        /// <returns></returns>
        private Task FetchAsync()
        {
            var task = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
            return task.ContinueWithOnMainThread(FetchComplete);
        }

        private void FetchComplete(Task task)
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("Retrieval hasn't finished.");
                return;
            }

            var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
            var info = remoteConfig.Info;
            if (info.LastFetchStatus != LastFetchStatus.Success)
            {
                Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
                return;
            }

            // Fetch successful. Parameter values must be activated to use.
            remoteConfig.ActivateAsync()
                .ContinueWithOnMainThread(task =>
                {
                    foreach (string key in remoteData.Keys)
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            string result = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
                            remoteData[key].Value = result;
                        }
                    }

                    remoteConfigIsFetchCompleted.Value = true;
                });
        }
#endif
    }
}