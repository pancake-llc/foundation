using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace PancakeEditor.Common
{
    using UnityEngine;

    internal sealed class HeartWindowInstaller : WindowInstallDependency
    {
        private static readonly Dictionary<string, string> InternalDependencies =
            new()
            {
                {"com.cysharp.unitask", "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.4"},
                {"com.annulusgames.alchemy","https://github.com/AnnulusGames/Alchemy.git?path=/Alchemy/Assets/Alchemy#v2.0.1"},
                {"com.annulusgames.lit-motion","https://github.com/AnnulusGames/LitMotion.git?path=src/LitMotion/Assets/LitMotion"}
            };

        protected override Dictionary<string, string> Dependencies => InternalDependencies;

        private new static void Show()
        {
            var window = GetWindow<HeartWindowInstaller>("Install Dependencies");
            window.minSize = new Vector2(450, 180);
            window.MoveToCenter();
        }

        protected override void OnDrawShowOnStartup()
        {
            GUILayout.Space(4);
            showOnReload = Editor.GetEditorBool(nameof(HeartWindowInstaller), true);
            EditorGUI.BeginChangeCheck();
            showOnReload = EditorGUILayout.Toggle("Show Window On Reload", showOnReload);
            if (EditorGUI.EndChangeCheck()) Editor.SetEditorBool(nameof(HeartWindowInstaller), showOnReload);
        }

        [InitializeOnLoadMethod]
        private static void ShowInstallerOnReload() { QueryReload(); }

        private static void QueryReload()
        {
            waitFramesTillReload = LOAD_TIME_IN_FRAMES;
            EditorApplication.update += Reload;
        }

        private static void Reload()
        {
            if (waitFramesTillReload > 0)
            {
                --waitFramesTillReload;
            }
            else
            {
                EditorApplication.update -= Reload;
                if (Editor.GetEditorBool(nameof(HeartWindowInstaller), true) && IsStillMissingPackage(InternalDependencies.Keys.ToList())) Show();
            }
        }
    }
}