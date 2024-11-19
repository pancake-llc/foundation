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
            private static CancellationTokenSource tokenSource;

            public static void CheckConnection(Action<ENetworkStatus> onCompleted)
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.WebGLPlayer:
                    default:
                        _ = CheckNetworkAndroid(onCompleted);
                        break;
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        _ = CheckNetworkiOS(onCompleted);
                        break;
                }
            }

            public static void StopCheckConnection()
            {
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }

            private static async Task CheckNetworkAndroid(Action<ENetworkStatus> onCompleted)
            {
                StopCheckConnection(); // cancel previous task
                tokenSource = new CancellationTokenSource();

                await PerformCheck(url: "https://clients3.google.com/generate_204",
                    responseType: ENetworkResponseType.HttpStatusCode,
                    statusCode: HttpStatusCode.NoContent,
                    expectedContent: null,
                    onCompleted: onCompleted,
                    token: tokenSource.Token);
            }

            private static async Task CheckNetworkiOS(Action<ENetworkStatus> onCompleted)
            {
                StopCheckConnection(); // cancel previous task
                tokenSource = new CancellationTokenSource();

                await PerformCheck(url: "https://captive.apple.com/hotspot-detect.html",
                    responseType: ENetworkResponseType.ResponseContent,
                    statusCode: HttpStatusCode.OK,
                    expectedContent: "<HTML><HEAD><TITLE>Success</TITLE></HEAD><BODY>Success</BODY></HTML>",
                    onCompleted: onCompleted,
                    token: tokenSource.Token);
            }

            private static async Task PerformCheck(
                string url,
                ENetworkResponseType responseType,
                HttpStatusCode statusCode,
                string expectedContent,
                Action<ENetworkStatus> onCompleted,
                CancellationToken token)
            {
                using var www = UnityWebRequest.Get(url);

                try
                {
                    await Awaitable.FromAsyncOperation(www.SendWebRequest(), token);
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                    return;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    onCompleted?.Invoke(ENetworkStatus.NoDnsConnection);
                    return;
                }

                ENetworkStatus status;
                switch (responseType)
                {
                    case ENetworkResponseType.HttpStatusCode:
                        status = (int) www.responseCode == (int) statusCode ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;
                        break;

                    case ENetworkResponseType.ResponseContent:
                        status = www.downloadHandler.text.Trim().Equals(expectedContent.Trim(), StringComparison.Ordinal)
                            ? ENetworkStatus.Connected
                            : ENetworkStatus.WalledGarden;
                        break;

                    case ENetworkResponseType.ResponseContainContent:
                        status = www.downloadHandler.text.Trim().Contains(expectedContent.Trim(), StringComparison.Ordinal)
                            ? ENetworkStatus.Connected
                            : ENetworkStatus.WalledGarden;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                onCompleted?.Invoke(status);
            }
        }
    }
}