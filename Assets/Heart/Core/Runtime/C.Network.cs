using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace Pancake
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
            private static AsyncProcessHandle handle;

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
                if (handle != null) App.StopCoroutine(handle);
            }

            private static void CheckNetworkAndroid(Action<ENetworkStatus> onCompleted)
            {
                if (handle != null) App.StopCoroutine(handle);
                handle = App.StartCoroutine(Check_HttpStatusCode("https://clients3.google.com/generate_204", HttpStatusCode.NoContent, onCompleted));
            }

            private static void CheckNetworkiOS(Action<ENetworkStatus> onCompleted)
            {
                if (handle != null) App.StopCoroutine(handle);
                handle = App.StartCoroutine(Check_ResponseContain("https://captive.apple.com/hotspot-detect.html",
                    "<HTML><HEAD><TITLE>Success</TITLE></HEAD><BODY>Success</BODY></HTML>",
                    onCompleted));
            }


            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            private static IEnumerator Check_HttpStatusCode(string url, HttpStatusCode statusCode, Action<ENetworkStatus> onCompleted)
            {
                var www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                ENetworkStatus status;
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    yield break;
                }

                status = (int) www.responseCode == (int) statusCode ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;

                onCompleted?.Invoke(status);
            }

            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            private static IEnumerator Check_ResponseContain(string url, string expectedContent, Action<ENetworkStatus> onCompleted)
            {
                var www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                ENetworkStatus status;
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    yield break;
                }

                status = www.downloadHandler.text.Trim().Equals(expectedContent.Trim()) ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;

                onCompleted?.Invoke(status);
            }

            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            private static IEnumerator Check_ResponseContainContent(string url, string expectedContent, Action<ENetworkStatus> onCompleted)
            {
                var www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                ENetworkStatus status;
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    yield break;
                }

                status = www.downloadHandler.text.Trim().Contains(expectedContent.Trim()) ? ENetworkStatus.Connected : ENetworkStatus.WalledGarden;

                onCompleted?.Invoke(status);
            }


            /// <summary>
            /// Check internet connection status
            /// </summary>
            /// <returns></returns>
            public static IEnumerator Check(
                string url,
                ENetworkResponseType responseType,
                HttpStatusCode statusCode,
                string expectedContent,
                Action<ENetworkStatus> onCompleted)
            {
                var www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                ENetworkStatus status;
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError || www.responseCode == 0)
                {
                    status = ENetworkStatus.NoDnsConnection;
                    onCompleted?.Invoke(status);
                    yield break;
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