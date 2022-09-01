using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [InitializeOnLoad]
    internal static class AutoRunWizard
    {
        static AutoRunWizard()
        {
            if (!EditorPrefs.GetBool($"wizard_{PlayerSettings.productGUID}", false))
                EditorApplication.update += AutoRunWizard.OnUpdate;
        }

        public static void OnUpdate()
        {
            if (WizardIsOpen()) return;
            Wizard.Open();
        }

        private static bool WizardIsOpen() => Resources.FindObjectsOfTypeAll<Wizard>().Length != 0;
    }
}