using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Pancake.Common;
using Pancake.SignIn;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;

namespace Pancake.Game
{
    public static class BackupDataHelper
    {
        public static async Task SaveFileBytes(string key, byte[] bytes)
        {
            try
            {
                await CloudSaveService.Instance.Files.Player.SaveAsync(key, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static async Task<byte[]> LoadFileBytes(string key)
        {
            try
            {
                byte[] results = await CloudSaveService.Instance.Files.Player.LoadBytesAsync(key);
                return results;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }

        public static async Task Backup(string bucket = "masterdata", Action onCompleted = null)
        {
#if UNITY_ANDROID
            if (AuthenticationGooglePlayGames.IsSignIn())
            {
                SignInEvent.status = false;
                SignInEvent.GetNewServerCode();
                await UniTask.WaitUntil(() => SignInEvent.status);
            }
            else return;

#elif UNITY_IOS
            if (string.IsNullOrEmpty(SignInEvent.ServerCode)) return;
#endif

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // signin cached
            }
            else
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(SignInEvent.ServerCode);
            }

            await PushData();
            onCompleted?.Invoke();
            return;

            async Task PushData()
            {
                byte[] inputBytes = Data.Backup();
                await SaveFileBytes(bucket, inputBytes);
            }
        }

        public static async Task Restore(string bucket = "masterdata", Action onCompleted = null)
        {
#if UNITY_ANDROID
            if (AuthenticationGooglePlayGames.IsSignIn())
            {
                SignInEvent.status = false;
                SignInEvent.GetNewServerCode();
                await UniTask.WaitUntil(() => SignInEvent.status);
            }
            else return;
#elif UNITY_IOS
            if (string.IsNullOrEmpty(SignInEvent.ServerCode)) return;
#endif

            if (AuthenticationService.Instance.SessionTokenExists)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(SignInEvent.ServerCode);
            }

            await FetchData();
            onCompleted?.Invoke();

            return;

            async Task FetchData()
            {
                byte[] inputBytes = await LoadFileBytes(bucket);
                Data.Restore(inputBytes);
            }
        }
    }
}