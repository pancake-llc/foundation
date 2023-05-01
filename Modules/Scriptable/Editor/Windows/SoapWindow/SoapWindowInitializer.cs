using UnityEditor;

namespace Obvious.Soap.Editor
{
    [InitializeOnLoad]
    public class SoapWindowInitializer
    {
        public const string HasShownWindowKey = "SoapWindow_HasShownWindow";

        static SoapWindowInitializer()
        {
            var hasShownWindow = EditorPrefs.GetBool(HasShownWindowKey, false);
            if (hasShownWindow)
                return;
            EditorApplication.update += OnEditorApplicationUpdate;
        }

        private static void OnEditorApplicationUpdate()
        {
            EditorApplication.update -= OnEditorApplicationUpdate;
            SoapWindow.Open();
            EditorPrefs.SetBool(HasShownWindowKey, true);
        }
    }
}