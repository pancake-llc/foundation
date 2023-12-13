using System;
using System.Collections.Generic;
using Pancake.Greenery;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryPoissonDiscScattererModule : GreeneryScattererModule
    {
        [Serializable]
        public class PoissonDiscScattererModuleSettings
        {
            public float samplingRadius = 1;
            public int rejectionSamples = 30;

            public Vector3 rectPosition = new Vector3(0, 0, 0);
            public Vector3 rectSize = new Vector3(10, 0, 10);
        }

        private const string POISSON_DISC_SCATTERER_SETTINGS_KEY = "GREENERY_POISSON_DISC_SCATTERER_SETTINGS";

        [SerializeField] private PoissonDiscScattererModuleSettings poissonDiscScattererModuleSettings;
        private BoxBoundsHandle boxBounds;

        public override void Initialize(GreeneryScatteringModule scatteringModule)
        {
            base.Initialize(scatteringModule);
            poissonDiscScattererModuleSettings = new PoissonDiscScattererModuleSettings();
            poissonDiscScattererModuleSettings.rectSize = new Vector3(10, 0, 10);
            if (EditorPrefs.HasKey(POISSON_DISC_SCATTERER_SETTINGS_KEY))
            {
                EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(POISSON_DISC_SCATTERER_SETTINGS_KEY), poissonDiscScattererModuleSettings);
            }

            boxBounds = new BoxBoundsHandle();
            boxBounds.midpointHandleSizeFunction = (Vector3 pos) => { return PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction(pos) * 2.0f; };
            Undo.undoRedoPerformed += SaveSettings;
        }

        public override void Release() { Undo.undoRedoPerformed -= SaveSettings; }

        public override void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            float samplingRadius = EditorGUILayout.FloatField("Sampling radius", poissonDiscScattererModuleSettings.samplingRadius);
            int rejectionSamples = EditorGUILayout.IntField("Rejection samples", poissonDiscScattererModuleSettings.rejectionSamples);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(scatteringModule.toolEditor, "Changed poisson scatterer settings");
                poissonDiscScattererModuleSettings.samplingRadius = samplingRadius;
                poissonDiscScattererModuleSettings.rejectionSamples = rejectionSamples;

                SaveSettings();
            }

            if (GUILayout.Button("Scatter"))
            {
                ScatterPoints(scatteringModule.toolEditor.GetSelectedItems(),
                    poissonDiscScattererModuleSettings.samplingRadius,
                    poissonDiscScattererModuleSettings.rejectionSamples,
                    poissonDiscScattererModuleSettings.rectPosition,
                    poissonDiscScattererModuleSettings.rectSize,
                    scatteringModule.scatteringModuleSettings.drawingLayerMask,
                    scatteringModule.scatteringModuleSettings.getSurfaceColor,
                    scatteringModule.scatteringModuleSettings.slopeThreshold,
                    scatteringModule.scatteringModuleSettings.sizeFactor,
                    scatteringModule.scatteringModuleSettings.colorGradient);
            }

            EditorGUILayout.EndVertical();
        }

        public override void ToolHandles(Rect GUIRect)
        {
            base.ToolHandles(GUIRect);

            Color colTransparent = Color.green;
            colTransparent.a = 0.2f;
            if (boxBounds == null)
            {
                boxBounds = new BoxBoundsHandle();
                boxBounds.midpointHandleSizeFunction = (Vector3 pos) => { return PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction(pos) * 2.0f; };
            }

            boxBounds.SetColor(Color.green);
            boxBounds.center = poissonDiscScattererModuleSettings.rectPosition;
            boxBounds.size = poissonDiscScattererModuleSettings.rectSize;
            using (var handleScope = new Handles.DrawingScope(Handles.matrix))
            {
                using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    boxBounds.DrawHandle();
                    Vector3 rectPosition = Handles.PositionHandle(poissonDiscScattererModuleSettings.rectPosition, Quaternion.identity);
                    if (changeCheckScope.changed)
                    {
                        Undo.RecordObject(scatteringModule.toolEditor, "Changed poisson handles");
                        poissonDiscScattererModuleSettings.rectSize = boxBounds.size;
                        poissonDiscScattererModuleSettings.rectSize.y = 0;
                        poissonDiscScattererModuleSettings.rectPosition = rectPosition;
                        SaveSettings();
                    }
                }
            }
        }

        public override float GetHeight() { return EditorGUIUtility.singleLineHeight * 4; }

        public override GUIContent GetIcon() { return EditorGUIUtility.IconContent("RectTool On"); }

        protected override void SaveSettings()
        {
            EditorPrefs.SetString(POISSON_DISC_SCATTERER_SETTINGS_KEY, EditorJsonUtility.ToJson(poissonDiscScattererModuleSettings));
        }

        #region Functionality

        private void ScatterPoints(
            List<GreeneryItem> selectedItems,
            float radius,
            int rejectionSamples,
            Vector3 rectPosition,
            Vector3 rectSize,
            LayerMask layerMask,
            bool getSurfaceColor,
            float slopeThreshold,
            float sizeFactor,
            Gradient colorGradient)
        {
            List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, new Vector2(rectSize.x, rectSize.z), rejectionSamples);
            Texture2D surfaceColorTexture = null;
            foreach (Vector2 point in points)
            {
                Color surfaceColor = Color.clear;
                Vector3 pos = rectPosition + new Vector3(point.x, 0, point.y) - new Vector3(rectSize.x, 0, rectSize.z) * 0.5f;
                if (Physics.Raycast(pos,
                        Vector3.down,
                        out RaycastHit spawnHit,
                        Mathf.Infinity,
                        layerMask))
                {
                    GreeneryManager greeneryManager = GreeneryEditorUtilities.GetFirstNonLocalManager();
                    if (spawnHit.collider != null && spawnHit.transform.TryGetComponent(out LocalGreeneryManager localGreeneryManager))
                    {
                        greeneryManager = localGreeneryManager;
                    }

                    Undo.RegisterCompleteObjectUndo(greeneryManager, "Scattered points");
                    if (getSurfaceColor)
                    {
                        surfaceColor = GreenerySurfaceColorSampling.GetSurfaceColor(spawnHit, ref surfaceColorTexture);
                    }

                    GreeneryScatteringUtilities.AddSpawnPoint(greeneryManager,
                        selectedItems,
                        spawnHit,
                        surfaceColor,
                        slopeThreshold,
                        sizeFactor,
                        colorGradient);

                    EditorUtility.SetDirty(greeneryManager);
                }
            }
        }

        #endregion
    }
}