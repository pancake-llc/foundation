namespace PancakeEditor
{
    using UnityEngine;

    internal static class Editime
    {
        private static bool IsEditimeInitialized { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (IsEditimeInitialized) return;
            new BaseMonoExceptionChecker().CheckForExceptions();
            IsEditimeInitialized = true;
        }
    }
}