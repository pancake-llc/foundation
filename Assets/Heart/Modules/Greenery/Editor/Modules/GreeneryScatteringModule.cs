using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryScatteringModule : GreeneryEditorModule
    {
        [Serializable]
        public class ScaterringModuleSettings
        {
            public LayerMask drawingLayerMask;
            public float slopeThreshold;
            public bool getSurfaceColor;
            public float sizeFactor;
            public bool sizeFactorLock;
            public Gradient colorGradient = new();
            public bool colorLock;
            public int activeScaterrerIndex;
        }

        private const string SCATTERING_SETTINGS_KEY = "GREENERY_SCATTERER_SETTINGS";

        public ScaterringModuleSettings scatteringModuleSettings;
        public GreeneryToolEditor toolEditor;

        [SerializeReference] private GreeneryScattererModule[] scattererModules;

        public override void Initialize(GreeneryToolEditor toolEditor)
        {
            this.toolEditor = toolEditor;
            scatteringModuleSettings = new ScaterringModuleSettings();
            if (EditorPrefs.HasKey(SCATTERING_SETTINGS_KEY))
            {
                EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(SCATTERING_SETTINGS_KEY), scatteringModuleSettings);
            }

            //Add your new scatterer modules here!
            scattererModules = new GreeneryScattererModule[]
            {
                new GreeneryBrushScattererModule(), new GreeneryPoissonDiscScattererModule(), new GreeneryIndividualPlacementModule(),
                new GreeneryMeshFillScattererModule()
            };

            foreach (var scattererModule in scattererModules)
            {
                scattererModule.Initialize(this);
            }

            toolEditor.OnGUI += OnGUI;
            Undo.undoRedoPerformed += SaveSettings;
        }

        public override void Release()
        {
            toolEditor.OnGUI -= OnGUI;
            Undo.undoRedoPerformed -= SaveSettings;

            foreach (var scattererModule in scattererModules)
            {
                scattererModule.Release();
            }
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            int activeScaterrerIndex = GUILayout.Toolbar(scatteringModuleSettings.activeScaterrerIndex, GetScatterrerIcons());
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(toolEditor, "Changed scatterer");
                scatteringModuleSettings.activeScaterrerIndex = activeScaterrerIndex;

                SaveSettings();
            }

            scattererModules[activeScaterrerIndex].OnGUI();
            toolEditor.OnToolHandles = scattererModules[activeScaterrerIndex].ToolHandles;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            LayerMask drawingLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(EditorGUILayout.MaskField("Drawing mask",
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(scatteringModuleSettings.drawingLayerMask),
                InternalEditorUtility.layers));
            float slopeThreshold = EditorGUILayout.Slider("Slope threshold", scatteringModuleSettings.slopeThreshold, 0, 1);
            bool getSurfaceColor = EditorGUILayout.Toggle("Get surface color", scatteringModuleSettings.getSurfaceColor);

            EditorGUILayout.BeginHorizontal();
            float sizeFactor = Mathf.Max(0.01f, EditorGUILayout.FloatField("Size factor", scatteringModuleSettings.sizeFactor));
            bool sizeFactorLock = GUILayout.Toggle(scatteringModuleSettings.sizeFactorLock,
                scatteringModuleSettings.sizeFactorLock ? EditorGUIUtility.IconContent("LockIcon-On") : EditorGUIUtility.IconContent("LockIcon"),
                "Button",
                GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            Gradient color = EditorGUILayout.GradientField("Color", scatteringModuleSettings.colorGradient);
            bool colorLock = GUILayout.Toggle(scatteringModuleSettings.colorLock,
                scatteringModuleSettings.colorLock ? EditorGUIUtility.IconContent("LockIcon-On") : EditorGUIUtility.IconContent("LockIcon"),
                "Button",
                GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(toolEditor, "Changed common scattering settings");
                scatteringModuleSettings.drawingLayerMask = drawingLayerMask;
                scatteringModuleSettings.slopeThreshold = slopeThreshold;
                scatteringModuleSettings.getSurfaceColor = getSurfaceColor;
                scatteringModuleSettings.sizeFactor = sizeFactor;
                scatteringModuleSettings.colorGradient = color;
                scatteringModuleSettings.sizeFactorLock = sizeFactorLock;
                scatteringModuleSettings.colorLock = colorLock;

                SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }

        public override void SaveSettings()
        {
            EditorPrefs.SetString(SCATTERING_SETTINGS_KEY, EditorJsonUtility.ToJson(scatteringModuleSettings));
            foreach (var scattererModule in scattererModules)
            {
                scattererModule.UpdateScatteringSettings(this);
            }
        }

        public override float GetHeight()
        {
            float height = EditorGUIUtility.singleLineHeight * 7;
            height += scattererModules[scatteringModuleSettings.activeScaterrerIndex].GetHeight();
            return height;
        }

        public GUIContent[] GetScatterrerIcons()
        {
            GUIContent[] scattererIcons = new GUIContent[scattererModules.Length];
            for (int i = 0; i < scattererModules.Length; i++)
            {
                scattererIcons[i] = scattererModules[i].GetIcon();
            }

            return scattererIcons;
        }
    }
}