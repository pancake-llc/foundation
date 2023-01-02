using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [InitializeOnLoad]
    public class AutoRunWizard
    {
        static AutoRunWizard()
        {
            if (!Wizard.Status) Show();
        }
        
        private static void Show()
        {
            if (WizardIsOpen()) return;
            Wizard.Open();
        }

        private static bool WizardIsOpen() => Resources.FindObjectsOfTypeAll<Wizard>().Length != 0;
    }
}