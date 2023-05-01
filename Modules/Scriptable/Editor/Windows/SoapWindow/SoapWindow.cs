using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PancakeEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public class SoapWindow : SoapWindowBase
    {
        protected override string HeaderTitle => $"Soap (version {Version})";

        private Texture[] _icons;
        private TextAsset[] _texts;
        private readonly List<CategoryButton> _leftButtons = new List<CategoryButton>();
        private Action _onCategoryButtonClicked;
        private CategoryButton _currentCategoryButton;
        private FloatVariable _floatVariable;

        private const string Version = "1.5.1";
        private const string DocURL = "https://drive.google.com/file/d/15quBiCzdHTZEV1e1JMIXQV1hLOHs0xg7/view";
        private const string DiscordURL = "https://discord.gg/CVhCNDbxF5";

        private const string StoreURL =
            "https://assetstore.unity.com/packages/tools/utilities/soap-scriptableobject-architecture-pattern-232107#reviews";

        private const string YoutubeURL = "https://www.youtube.com/@obviousgame";
        public const string LastCategoryKey = "SoapWindow_LastCategory";

        private readonly string[] _sceneNames =
        {
            "1_ScriptableVariables", "2_Bindings", "3_ScriptableLists", "4_ScriptableEvents", "5_Save", "6_FastPlayMode"
        };

        [MenuItem("Window/Obvious/Soap/Soap Window", priority = 0)]
        public static void Open()
        {
            var window = GetWindow(typeof(SoapWindow), true, "Soap Window");
            window.minSize = new Vector2(650, 500);
            window.maxSize = new Vector2(650, 500);
            window.Show();
        }

        protected override void Init()
        {
            base.Init();
            LoadAssets();
            SetUpCategoryButtons();
        }

        private void LoadAssets()
        {
            _icons = new Texture[6];
            _icons[0] = Resources.Load<Texture>("Icons/icon_discord");
            _icons[1] = Resources.Load<Texture>("Icons/icon_youtube");
            _icons[2] = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            _icons[3] = Resources.Load<Texture>("Icons/icon_documentation");
            _icons[4] = EditorGUIUtility.IconContent("UnityLogoLarge").image;
            _icons[5] = EditorGUIUtility.IconContent("SceneViewTools").image;
            _texts = new TextAsset[6];
            for (int i = 0; i < 6; i++)
                _texts[i] = Resources.Load<TextAsset>("Texts/" + _sceneNames[i]);
        }

        private void SetUpCategoryButtons()
        {
            //General
            var gettingStarted = new CategoryButton
                { Name = "Getting Started", OnClick = DrawGettingStarted, GroupName = "General" };
            _leftButtons.Add(gettingStarted);
            var settingsButton = new CategoryButton { Name = "Settings", OnClick = DrawSettings };
            _leftButtons.Add(settingsButton);

            //Examples Scenes
            for (int i = 0; i < 6; i++)
            {
                var group = i == 0 ? "Examples Scenes" : string.Empty;
                var groupIcon = i == 0 ? _icons[2] : null;
                var index = i;
                var scene = new CategoryButton
                {
                    Name = _sceneNames[i], OnClick = () => DrawScene(index), GroupName = group, GroupIcon = groupIcon
                };
                _leftButtons.Add(scene);
            }

            //Tools
            var soapWizard = new CategoryButton
                { Name = "Soap Wizard", OnClick = DrawSoapWizard, GroupName = "Tools", GroupIcon = _icons[5] };
            _leftButtons.Add(soapWizard);
            var eventsDebugWindow = new CategoryButton
                { Name = "Events Debug Window", OnClick = DrawEventsDebugWindow };
            _leftButtons.Add(eventsDebugWindow);

            //Select last category
            var lastCategory = EditorPrefs.GetString(LastCategoryKey, string.Empty);
            var button = string.IsNullOrEmpty(lastCategory)
                ? _leftButtons.FirstOrDefault(b => b.Name == gettingStarted.Name)
                : _leftButtons.FirstOrDefault(b => b.Name == lastCategory);
            SelectCategory(button);
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            GUILayout.Space(2);
            DrawLinkButtons();
            Uniform.DrawLine(2);
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
            DrawFooter();
        }

        private void SelectCategory(CategoryButton categoryButton)
        {
            _onCategoryButtonClicked = categoryButton.OnClick;
            _currentCategoryButton = categoryButton;
            EditorPrefs.SetString(LastCategoryKey, categoryButton.Name);
        }

        private void DrawLinkButtons()
        {
            GUILayout.BeginHorizontal();
            var buttonWidth = position.width / 4f;

            var guiContent = new GUIContent("Documentation", _icons[3]);
            if (GUILayout.Button(guiContent, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(20f)))
                Application.OpenURL(DocURL);

            guiContent = new GUIContent("Discord", _icons[0]);
            if (GUILayout.Button(guiContent, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(20f)))
                Application.OpenURL(DiscordURL);

            guiContent = new GUIContent("Asset Store", _icons[4]);
            if (GUILayout.Button(guiContent, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(20f)))
                Application.OpenURL(StoreURL);

            guiContent = new GUIContent("Youtube", _icons[1]);
            if (GUILayout.Button(guiContent, GUILayout.MaxWidth(buttonWidth), GUILayout.MaxHeight(20f)))
                Application.OpenURL(YoutubeURL);

            GUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150f), GUILayout.ExpandHeight(true));

            var index = 0;
            foreach (var categoryButton in _leftButtons)
            {
                if (!string.IsNullOrEmpty(categoryButton.GroupName))
                {
                    GUILayout.Space(index == 0 ? 0 : 10f);
                    if (categoryButton.GroupIcon != null)
                    {
                        var guiStyle = new GUIStyle(GUI.skin.label);
                        guiStyle.alignment = TextAnchor.MiddleLeft;
                        guiStyle.imagePosition = ImagePosition.ImageLeft;
                        guiStyle.fixedWidth = 150f;
                        GUILayout.Label(new GUIContent(categoryButton.GroupName, categoryButton.GroupIcon), guiStyle,
                            GUILayout.MaxHeight(20f));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(categoryButton.GroupName, GUILayout.MaxWidth(150));
                    }
                }

                if (GUILayout.Button(categoryButton.Name))
                    SelectCategory(categoryButton);
                index++;
            }

            EditorGUILayout.EndVertical();
        }

        private string GetScenePath(string sceneName)
        {
            var guid = AssetDatabase.FindAssets("t:Scene " + sceneName).FirstOrDefault();
            return guid != null ? AssetDatabase.GUIDToAssetPath(guid) : null;
        }

        private void DrawRightPanel()
        {
            if (_onCategoryButtonClicked == null)
                return;
            EditorGUILayout.BeginVertical();
            GUILayout.Space(5f);
            EditorGUILayout.LabelField(_currentCategoryButton.Name, EditorStyles.boldLabel);
            _onCategoryButtonClicked?.Invoke();
            EditorGUILayout.EndVertical();
        }

        private void DrawGettingStarted()
        {
            EditorGUILayout.LabelField(
                "Thank you for purchasing Soap : Scriptable Object Architecture Pattern." +
                "\nHow to get started ?",
                EditorStyles.wordWrappedLabel);

            DrawGroup(
                "- Read the user guide. You can learn about why to use Soap and its features.",
                "User Guide", _icons[3], () => Application.OpenURL(DocURL));

            DrawGroup(
                "- Check out the examples scenes and their documentation. They will show you practical example of using Soap in a game.",
                "Example Scenes", _icons[2],
                () => SelectCategory(_leftButtons.FirstOrDefault(b => b.Name == _sceneNames[0])));

            DrawGroup(
                "- Watch the Youtube tutorials serie. They will teach you step-by-step how to make a RogueLike game with Soap.",
                "Youtube", _icons[1], () => Application.OpenURL(YoutubeURL));


            void DrawGroup(string description, string buttonName, Texture icon, Action onButtonClick)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var guiContent = new GUIContent(buttonName, icon);
                if (GUILayout.Button(guiContent, GUILayout.MaxWidth(150f), GUILayout.MaxHeight(20f)))
                    onButtonClick?.Invoke();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSettings()
        {
            var settings = ScriptableEditorSetting.Instance;
            // EditorGUI.BeginChangeCheck();
            // ScriptableEditorSetting.DrawMode =
            //     (EVariableDrawMode)EditorGUILayout.EnumPopup("Variable Display Mode", settings.VariableDisplayMode);
            // if (EditorGUI.EndChangeCheck())
            // {
            //     EditorUtility.SetDirty(settings);
            //     AssetDatabase.SaveAssets();
            // }

            EditorGUILayout.LabelField(
                "Default: displays all the parameters of variables. " +
                "\nMinimal : only displays the value.", EditorStyles.wordWrappedMiniLabel);

            //Example of a float variable
            Uniform.DrawLine(2);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Example:", EditorStyles.boldLabel);
            if (_floatVariable == null)
                _floatVariable = CreateInstance<FloatVariable>();
            var editor = UnityEditor.Editor.CreateEditor(_floatVariable);
            editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }

        private void DrawScene(int i)
        {
            var sceneName = _sceneNames[i];
            EditorGUILayout.LabelField(_texts[i].text, EditorStyles.wordWrappedLabel);

            GUILayout.Space(25f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(50f);
            var guiContent = new GUIContent("Open Example Scene", _icons[2]);
            if (GUILayout.Button(guiContent, GUILayout.MaxHeight(50), GUILayout.ExpandWidth(true)))
                EditorSceneManager.OpenScene(GetScenePath(sceneName + "_Example_Scene"));

            GUILayout.Space(25f);
            guiContent = new GUIContent("Open Documentation", _icons[3]);
            if (GUILayout.Button(guiContent, GUILayout.MaxHeight(50), GUILayout.ExpandWidth(true)))
            {
                var scenePath = GetScenePath(sceneName + "_Example_Scene");
                var docPath = Path.GetDirectoryName(scenePath) + "/" + sceneName + "_Documentation.pdf";
                Application.OpenURL(docPath);
            }

            GUILayout.Space(50f);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSoapWizard()
        {
            EditorGUILayout.LabelField(
                "The Soap Wizard allows you to manage all the Scriptable Objects from Soap in one place. It also provides convenient quality of life features such as the ability to:" +
                "\n- create new custom Soap types for your classes" +
                "\n- filter all Soap Scriptable Objects by category" +
                "\n- find and select a specific Scriptable Object" +
                "\n- rename a Scriptable Object" +
                "\n- duplicate a Scriptable Object" +
                "\n- delete a Scriptable Object", EditorStyles.wordWrappedLabel);
            GUILayout.Space(50);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Soap Wizard", GUILayout.MaxHeight(50), GUILayout.MaxWidth(200)))
                SoapWizardWindow.Show();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawEventsDebugWindow()
        {
            EditorGUILayout.LabelField(
                "The Events Debug Window allows you to search for a specific method (by name) to check from which Scriptable Event and EventListener it is called from.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField(
                "If you are using Rider, this is already handled by the IDE, but not with Visual studio.",
                EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(50);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Events Debug Window", GUILayout.MaxHeight(50), GUILayout.MaxWidth(200)))
                EventsDebugWindow.Show();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        private void DrawFooter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                "For any suggestions or questions, please reach out on discord or by email : obviousgame.contact@gmail.com",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    public class CategoryButton
    {
        public string Name;
        public Action OnClick;
        public string GroupName;
        public Texture GroupIcon;
    }
}