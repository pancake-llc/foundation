using UnityEngine;
using UnityEngine.Networking;

namespace Pancake.Feedback
{
    internal readonly struct AsyncWebRequestData
    {
        /// <summary>
        /// The underlying UnityWebRequest
        /// </summary>
        public UnityWebRequest Request { get; }

        /// <summary>
        /// The AsyncOperation for this request
        /// </summary>
        public AsyncOperation Operation { get; }

        /// <summary>
        /// Whether or not the async operation is finished
        /// </summary>
        public bool OperationIsDone => Operation.isDone;

        /// <summary>
        /// Whether or not the request has resulted in an error
        /// </summary>
        public bool RequestIsError => Request.result == UnityWebRequest.Result.ProtocolError || Request.result == UnityWebRequest.Result.ConnectionError;

        public string ErrorText
        {
            get
            {
                if (Request.result == UnityWebRequest.Result.ProtocolError)
                    return Request.downloadHandler.text;

                if (RequestIsError)
                    return Request.error;

                return string.Empty;
            }
        }

        public AsyncWebRequestData(UnityWebRequest request, AsyncOperation operation)
        {
            Request = request;
            Operation = operation;
        }
    }
}