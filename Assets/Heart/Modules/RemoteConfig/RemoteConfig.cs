using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alchemy.Serialization;
using Pancake.Monetization;
#if PANCAKE_REMOTE_CONFIG
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
#endif
using UnityEngine;

namespace Pancake.Tracking
{
    [AlchemySerialize]
    public partial class RemoteConfig : MonoBehaviour
    {
        [AlchemySerializeField, NonSerialized] private Dictionary<StringConstant, string> remoteData = new();
        [SerializeField] private StringConstant currentAdNetwork;

        public static bool IsFetchCompleted { get; set; }

#if PANCAKE_REMOTE_CONFIG
        private void Start()
        {
            IsFetchCompleted = false;
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
        /// timespan.  Otherwise, it assumes the data is "recent enough", and does nothing.
        /// By default, the timespan is 12 hours, and for production apps, this is a good
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
                    foreach (var key in remoteData.Keys)
                    {
                        if (!string.IsNullOrEmpty(key.Value))
                        {
                            ConfigValue configValue = FirebaseRemoteConfig.DefaultInstance.GetValue(key.Value);
                            if (configValue.Source == ValueSource.RemoteValue)
                            {
                                remoteData[key] = configValue.StringValue;
                            }
                        }
                    }

                    if (currentAdNetwork != null && currentAdNetwork.Value != string.Empty)
                    {
                        Advertising.ChangeNetwork(remoteData[currentAdNetwork]);
                    }

                    IsFetchCompleted = true;
                });
        }

#endif
    }
}