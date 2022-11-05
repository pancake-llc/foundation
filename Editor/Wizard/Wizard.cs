using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    internal class Wizard : EditorWindow
    {
        private enum EWizardPage
        {
        }

        private const string TITLE = "Wizard";
        private const int PAGE_COUNT = 2;
        private readonly string[] _headers = new[] {""};

        private readonly string[] _descriptions = new[] {""};

        private static readonly Vector2 WindowSize = new Vector2(400f, 280f);

        private static EWizardPage Page
        {
            get => (EWizardPage) EditorPrefs.GetInt($"wizard_{PlayerSettings.productGUID}_page", 0);
            set => EditorPrefs.SetInt($"wizard_{PlayerSettings.productGUID}_page", (int) value);
        }

        public static void Open()
        {
            var window = EditorWindow.GetWindow<Wizard>(true, TITLE, true);
            window.minSize = WindowSize;
            window.maxSize = WindowSize;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            Styles.Init();
            GUILayout.Label(_headers[(int) Page], Styles.title);
            GUILayout.Label(_descriptions[(int) Page], Styles.description);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install", GUILayout.Height(28f), GUILayout.Width(100)))
            {
                if ((int) Page + 1 < PAGE_COUNT)
                {
                    Page++;
                }
                else
                {
                    EditorPrefs.SetBool($"wizard_{PlayerSettings.productGUID}", true);
                    EditorApplication.update -= AutoRunWizard.OnUpdate;
                    Close();
                }
            }

            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Page {(int) Page + 1}/{PAGE_COUNT}");
        }

        private static class Styles
        {
            private static bool initialized;
            public static GUIStyle title;
            public static GUIStyle description;

            public static void Init()
            {
                if (initialized) return;

                title = new GUIStyle(GUI.skin.label) {richText = true, fontSize = 18};
                SetTextColor(title, new Color(0.58f, 0.87f, 0.35f));

                description = new GUIStyle(GUI.skin.label) {fontSize = 12, wordWrap = true, richText = true};
                SetTextColor(description, new Color(0.93f, 0.93f, 0.93f));
            }

            private static void SetTextColor(GUIStyle style, Color color) =>
                style.normal.textColor = style.active.textColor = style.focused.textColor = style.hover.textColor =
                    style.onNormal.textColor = style.onActive.textColor = style.onFocused.textColor = style.onHover.textColor = color;
        }
    }
}