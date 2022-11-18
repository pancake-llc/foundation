namespace Pancake.Feedback
{
    internal readonly struct WebResponse
    {
        public readonly string Text;
        public readonly bool IsError;
        public readonly long HTTPStatusCode;

        public static WebResponse GetResponse(AsyncWebRequestData requestData)
        {
            if (requestData.RequestIsError)
                return new WebResponse(
                    requestData.ErrorText,
                    true,
                    requestData.Request.responseCode);

            return new WebResponse(
                requestData.Request.downloadHandler.text,
                false,
                requestData.Request.responseCode);
        }

        public WebResponse(string text, bool isError, long httpStatusCode)
        {
            Text = text;
            IsError = isError;
            HTTPStatusCode = httpStatusCode;
        }
    }
}