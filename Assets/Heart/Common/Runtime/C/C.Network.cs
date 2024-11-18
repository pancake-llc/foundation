using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Pancake.Common
{
    /// <summary>
    /// Enum representing the response type the internet check will use to determine internet status
    /// </summary>
    public enum ENetworkResponseType
    {
        /// <summary>
        /// Check is performed using the response HTTP status code
        /// </summary>
        HttpStatusCode,

        /// <summary>
        /// Check is performed using the response content
        /// </summary>
        ResponseContent,

        /// <summary>
        /// Check is performed using part of the response content
        /// </summary>
        ResponseContainContent
    }

    /// <summary>
    /// Enum representing the internet connection status
    /// </summary>
    public enum ENetworkStatus
    {
        /// <summary>
        /// A network check has not being performed yet, or it is currently in progress for the first time
        /// </summary>
        PendingCheck,

        /// <summary>
        /// No connection could be established to a valid DNS destination
        /// </summary>
        NoDnsConnection,

        /// <summary>
        /// General network connection was established, but target destination could not be reached due to restricted internet access.
        /// </summary>
        WalledGarden,

        /// <summary>
        /// Network connection was established succesfully
        /// </summary>
        Connected
    }

    public static partial class C
    {
        public static class Network
        {
            private static Task activeTask;
            private static CancellationTokenSource tokenSource;

            public static void CheckConnection(Action<ENetworkStatus> onCompleted)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.WebGLPlayer:
                    default:
                        CheckNetworkAndroid(onCompleted);
                        break;
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        CheckNetworkiOS(onCompleted);
                        break;
                }
            }

            public static void StopCheckConnection()
            {
                if (tokenSource != null && activeTask is {IsCompleted: false})
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }
            }

            private static void CheckNetworkAndroid(Action<ENetworkStatus> onCompleted)
            {
                tokenSource ??= new CancellationTokenSource();

                if (activeTask is {IsCompleted: false})
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }

                activeTask = Check_HttpStatusCode("https://clients3.google.com/generate_204", HttpStatusCode.NoContent, onCompleted, tokenSource.Token);
            }

            private static void CheckNetworkiOS(Action<ENetworkStatus> onCompleted)
            {
                tokenSource ??= new CancellationTokenSource();

                if (activeTask is {IsCompleted: false})
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }

                activeTask = Check_ResponseContain("https://captive.apple.com/hotspot-detect.html",
                    "<HTML><HEAD><TITLE>Success</TITLE></HEAD><BODY>Success</BODY></HTML>",
                    onCompleted,
                    tokenSource.Token);
            }
            
            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            private static async Task Check_HttpStatusCode(string url, HttpStatusCode statusCode, Action<ENetworkStatus> onCompleted, CancellationToken token)
            {
                using var www = UnityWebRequest.Get(url);
                ENetworkStatus status;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Awaitable.FromAsyncOperation(www.SendWebRequest(), token);
                    }
                    catch (Exception)
                    {
                        status = ENetworkStatus.PendingCheck;
                        onCompleted?.Invoke(status);
                        return;
                    }
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    return;
                }

                status = (int) www.responseCode == (int) statusCode ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;

                onCompleted?.Invoke(status);
            }

            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            private static async Task Check_ResponseContain(string url, string expectedContent, Action<ENetworkStatus> onCompleted, CancellationToken token)
            {
                using var www = UnityWebRequest.Get(url);
                ENetworkStatus status;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Awaitable.FromAsyncOperation(www.SendWebRequest(), token);
                    }
                    catch (Exception)
                    {
                        status = ENetworkStatus.PendingCheck;
                        onCompleted?.Invoke(status);
                        return;
                    }
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    return;
                }

                status = www.downloadHandler.text.Trim().Equals(expectedContent.Trim()) ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;

                onCompleted?.Invoke(status);
            }

            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            private static async Task Check_ResponseContainContent(string url, string expectedContent, Action<ENetworkStatus> onCompleted, CancellationToken token)
            {
                using var www = UnityWebRequest.Get(url);
                ENetworkStatus status;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Awaitable.FromAsyncOperation(www.SendWebRequest(), token);
                    }
                    catch (Exception)
                    {
                        status = ENetworkStatus.PendingCheck;
                        onCompleted?.Invoke(status);
                        return;
                    }
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    return;
                }

                status = www.downloadHandler.text.Trim().Contains(expectedContent.Trim()) ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;

                onCompleted?.Invoke(status);
            }
            
            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            public static async Task Check(
                string url,
                ENetworkResponseType responseType,
                HttpStatusCode statusCode,
                string expectedContent,
                Action<ENetworkStatus> onCompleted,
                CancellationToken token)
            {
                using var www = UnityWebRequest.Get(url);
                ENetworkStatus status;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Awaitable.FromAsyncOperation(www.SendWebRequest(), token);
                    }
                    catch (Exception)
                    {
                        status = ENetworkStatus.PendingCheck;
                        onCompleted?.Invoke(status);
                        return;
                    }
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    return;
                }

                switch (responseType)
                {
                    case ENetworkResponseType.HttpStatusCode:
                        status = (int) www.responseCode == (int) statusCode ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;
                        break;
                    case ENetworkResponseType.ResponseContent:
                        status = www.downloadHandler.text.Trim().Equals(expectedContent.Trim()) ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;
                        break;
                    case ENetworkResponseType.ResponseContainContent:
                        status = www.downloadHandler.text.Trim().Contains(expectedContent.Trim()) ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                onCompleted?.Invoke(status);
            }
        }
    }
}