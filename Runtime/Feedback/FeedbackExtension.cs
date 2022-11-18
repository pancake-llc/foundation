namespace Pancake.Feedback
{
    internal static class FeedbackExtension
    {
        /// <summary>
        /// Wraps a class around a json array so that it can be deserialized by JsonUtility
        /// </summary>
        /// <param name="source"></param>
        /// <param name="topClass"></param>
        /// <returns></returns>
        public static string WrapToClass(this string source, string topClass) => $"{{\"{topClass}\": {source}}}";
    }

    
}