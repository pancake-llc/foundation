using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PancakeEditor.Common
{
    public abstract class WindowInstallDependency : WindowBase
    {
        protected abstract Dictionary<string, string> Dependencies { get; }

        protected override void OnGUI()
        {
            GUILayout.Label("You need to install the below dependencies for work properly", headerLabel);

            GUILayout.Space(4);


            bool isStillMissingPackage = IsStillMissingPackage(Dependencies.Keys.ToList(), DrawValidateDependency);

            GUILayout.Space(4);

            GUI.enabled = isStillMissingPackage;
            if (GUILayout.Button("Install All"))
            {
                Close();
                Client.AddAndRemove(Dependencies.Values.ToArray());
            }

            GUI.enabled = true;
            GUILayout.Space(4);
            Uniform.DrawLine(Uniform.Jet, 2);
            GUILayout.Space(2);
            if (!isStillMissingPackage)
            {
                GUILayout.Label("<color=#FFB76B>All dependencies was installed.\nPlease turn off `Show Window On Reload` to not show again</color>", htmlLabel);
            }

            OnDrawShowOnStartup();

            GUILayout.FlexibleSpace();
            base.OnGUI();

            return;

            void DrawValidateDependency(bool isInstalled, string dependency)
            {
                GUILayout.Label(isInstalled ? $"• <color=#4FF97A>{dependency}</color>" : $"• <color=#FF3333>{dependency}</color>", htmlLabel);
            }
        }

        protected static bool IsStillMissingPackage(List<string> dependencies, Action<bool, string> validateElementInstalled = null)
        {
            var isStillMissingPackage = false;

            string manifest = File.ReadAllText(Path.Combine(Application.dataPath, "..", "Packages", "manifest.json"));
            foreach (string dependency in dependencies)
            {
                var isInstalled = true;
                if (!manifest.Contains(dependency))
                {
                    isStillMissingPackage = true;
                    isInstalled = false;
                }

                validateElementInstalled?.Invoke(isInstalled, dependency);
            }

            return isStillMissingPackage;
        }

        protected virtual void OnDrawShowOnStartup() { }
    }
}