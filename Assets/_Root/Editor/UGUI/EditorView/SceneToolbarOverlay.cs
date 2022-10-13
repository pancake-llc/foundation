using System;

#if UNITY_EDITOR
namespace Pancake.Toolbar
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Overlays;
    using UnityEditor.SceneManagement;
    using UnityEditor.Toolbars;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;


    // ReSharper disable InconsistentNaming
    public static class SceneToolbar
    {
        public static readonly List<string> ScenePaths = new();

        public static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        public static void LoadScenes()
        {
            // clear scenes 
            ScenePaths.Clear();

            // find all scenes in the Assets folder
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] {"Assets"});

            foreach (var sceneGuid in sceneGuids)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
                ScenePaths.Add(scenePath);
            }
        }
    }

    [Icon("d_SceneAsset Icon")]
    [Overlay(typeof(SceneView), OverlayID, "Extend Tools")]
    public class SceneToolbarOverlay : ToolbarOverlay
    {
        public const string OverlayID = "scene-tool_extend-overlay";

        private SceneToolbarOverlay()
            : base(SceneDropdown.ID, PlayModePlayToggle.ID, PlayModePauseToggle.ID, PlayModeStepButton.ID)
        {
        }

        public override void OnCreated()
        {
            // load the scenes when the toolbar overlay is initially created
            SceneToolbar.LoadScenes();

            // subscribe to the event where scene assets were potentially modified
            EditorApplication.projectChanged += OnProjectChanged;
        }

        // Called when an Overlay is about to be destroyed.
        // Usually this corresponds to the EditorWindow in which this Overlay resides closing. (Scene View in this case)
        public override void OnWillBeDestroyed()
        {
            // unsubscribe from the event where scene assets were potentially modified
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged()
        {
            // reload the scenes whenever scene assets were potentially modified
            SceneToolbar.LoadScenes();
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public class SceneDropdown : EditorToolbarDropdown
    {
        public const string ID = SceneToolbarOverlay.OverlayID + "/scene-dropdown";

        private const string Tooltip = "Switch Scene";

        public SceneDropdown()
        {
            var content = EditorGUIUtility.TrTextContentWithIcon(SceneManager.GetActiveScene().name, Tooltip, "UnityLogo");
            text = content.text;
            tooltip = content.tooltip;
            icon = content.image as Texture2D;

            // hacky: the text element is the second one here so we can set the padding
            //        but this is not really robust I think
            ElementAt(1).style.paddingLeft = 5;
            ElementAt(1).style.paddingRight = 5;

            clicked += ToggleDropdown;

            // keep track of panel events
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
            // subscribe to the event where the play mode has changed
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // subscribe to the event where scene assets were potentially modified
            EditorApplication.projectChanged += OnProjectChanged;

            // subscribe to the event where a scene has been opened
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            // unsubscribe from the event where the play mode has changed
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            // unsubscribe from the event where scene assets were potentially modified
            EditorApplication.projectChanged -= OnProjectChanged;

            // unsubscribe from the event where a scene has been opened
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    SetEnabled(true);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    // don't allow switching scenes while in play mode
                    SetEnabled(false);
                    break;
            }
        }

        private void OnProjectChanged()
        {
            // update the dropdown label whenever the active scene has potentially be renamed
            text = SceneManager.GetActiveScene().name;
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            // update the dropdown label whenever a scene has been opened
            text = scene.name;
        }

        private void ToggleDropdown()
        {
            var menu = new GenericMenu();
            foreach (var scenePath in SceneToolbar.ScenePaths)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                menu.AddItem(new GUIContent(sceneName), text == sceneName, () => OnDropdownItemSelected(sceneName, scenePath));
            }

            menu.DropDown(worldBound);
        }

        private void OnDropdownItemSelected(string sceneName, string scenePath)
        {
            text = sceneName;
            SceneToolbar.OpenScene(scenePath);
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public sealed class PlayModePlayToggle : EditorToolbarToggle
    {
        public const string ID = SceneToolbarOverlay.OverlayID + "/playmode_play-toggle";

        private const string Tooltip = "Play";

        public PlayModePlayToggle()
        {
            var content = EditorGUIUtility.TrTextContentWithIcon("", Tooltip, "PlayButton");
            text = content.text;
            tooltip = content.tooltip;
            icon = content.image as Texture2D;
            
            value = EditorPrefs.GetBool($"{Application.identifier}_PlayModePlay", false);
            this.RegisterValueChangedCallback(Toggle);
            
            EditorApplication.playModeStateChanged  -= OnPlayModeChange;
            EditorApplication.playModeStateChanged  += OnPlayModeChange;
        }

        private void Toggle(ChangeEvent<bool> evt)
        {
            EditorApplication.isPlaying = evt.newValue;
            EditorPrefs.SetBool($"{Application.identifier}_PlayModePlay", evt.newValue);
        }

        private static void OnPlayModeChange(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    EditorPrefs.DeleteKey($"{Application.identifier}_PlayModePlay");
                    EditorPrefs.DeleteKey($"{Application.identifier}_PlayModePause");
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorPrefs.SetBool($"{Application.identifier}_PlayModePlay", true);
                    break;
            }
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public sealed class PlayModePauseToggle : EditorToolbarToggle
    {
        public const string ID = SceneToolbarOverlay.OverlayID + "/playmode_pause-toggle";

        private const string Tooltip = "Pause";

        public PlayModePauseToggle()
        {
            var content = EditorGUIUtility.TrTextContentWithIcon("", Tooltip, "PauseButton");
            text = content.text;
            tooltip = content.tooltip;
            icon = content.image as Texture2D;
            
            value = EditorPrefs.GetBool($"{Application.identifier}_PlayModePause", false);
            this.RegisterValueChangedCallback(Toggle);
        }

        private void Toggle(ChangeEvent<bool> evt)
        {
            EditorApplication.isPaused = evt.newValue;
            EditorPrefs.SetBool($"{Application.identifier}_PlayModePause", evt.newValue);
        }
    }

    [EditorToolbarElement(ID, typeof(SceneView))]
    public sealed class PlayModeStepButton : EditorToolbarButton
    {
        public const string ID = SceneToolbarOverlay.OverlayID + "/playmode_step-toggle";

        private const string Tooltip = "Next Step";

        public PlayModeStepButton()
        {
            SetEnabled(false);
            var content = EditorGUIUtility.TrTextContentWithIcon("", Tooltip, "StepButton");
            text = content.text;
            tooltip = content.tooltip;
            icon = content.image as Texture2D;

            RegisterCallback<ClickEvent>(ButtonCallback);
            EditorApplication.playModeStateChanged  -= OnPlayModeChange;
            EditorApplication.playModeStateChanged  += OnPlayModeChange;
        }

        private void OnPlayModeChange(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    SetEnabled(true);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    SetEnabled(false);
                    break;
            }
        }

        private void ButtonCallback(ClickEvent evt)
        {
            if (!EditorApplication.isPaused) EditorApplication.isPaused = true;
            EditorApplication.Step();
        }
    }

    [InitializeOnLoad]
    internal sealed class EditorQuitHandle
    {
        private static void Quit()
        {
            EditorPrefs.DeleteKey($"{Application.identifier}_PlayModePlay");
            EditorPrefs.DeleteKey($"{Application.identifier}_PlayModePause");
        }

        static EditorQuitHandle()
        {
            EditorApplication.quitting -= Quit;
            EditorApplication.quitting += Quit;
        }
    }
}
#endif