using System;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryIndividualPlacementModule : GreeneryScattererModule
    {
        public float scalingSensitivity = 0.1f;

        private const string INDIVIDUAL_PLACEMENT_SETTINGS_KEY = "GREENERY_INDIVIDUAL_PLACEMENT_SETTINGS";

        private Vector2 orMousePos = Vector2.zero;
        private int spawnPointIndex = -1;
        private GreeneryItem selectedItem = null;
        private GreeneryManager greeneryManager;

        public override void Initialize(GreeneryScatteringModule scatteringModule)
        {
            base.Initialize(scatteringModule);
            if (EditorPrefs.HasKey(INDIVIDUAL_PLACEMENT_SETTINGS_KEY))
            {
                scalingSensitivity = EditorPrefs.GetFloat(INDIVIDUAL_PLACEMENT_SETTINGS_KEY);
            }

            Undo.undoRedoPerformed += SaveSettings;
        }

        public override void Release() { Undo.undoRedoPerformed -= SaveSettings; }

        public override void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            float scalingSensitivity = Mathf.Max(1, EditorGUILayout.FloatField("Scaling sensitivity", this.scalingSensitivity));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(scatteringModule.toolEditor, "Changed individual placement scaling sensitivity");
                this.scalingSensitivity = scalingSensitivity;

                SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }


        public override void ToolHandles(Rect GUIRect)
        {
            base.ToolHandles(GUIRect);

            Vector2 mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, scatteringModule.scatteringModuleSettings.drawingLayerMask))
            {
                if (spawnPointIndex < 0)
                {
                    Color discColor = Color.green;
                    Handles.color = discColor;
                    Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);
                    discColor.a = 0.1f;
                    Handles.color = discColor;
                    Handles.DrawSolidDisc(hit.point, hit.normal, 0.5f);
                }
            }

            if (!GUIRect.Contains(mousePos))
            {
                IndividualPlacement(hit);
            }
        }

        public void IndividualPlacement(RaycastHit hit)
        {
            Event currentEvent = Event.current;

            if (currentEvent.button == 0)
            {
                if (spawnPointIndex < 0 && currentEvent.type == EventType.MouseDown)
                {
                    greeneryManager = GreeneryEditorUtilities.GetFirstNonLocalManager();
                    if (hit.collider != null && hit.transform.TryGetComponent(out LocalGreeneryManager localGreeneryManager))
                    {
                        greeneryManager = localGreeneryManager;
                    }

                    orMousePos = currentEvent.mousePosition;
                    Color surfaceColor = Color.clear;
                    Texture2D surfaceColorTexture = null;
                    if (scatteringModule.scatteringModuleSettings.getSurfaceColor)
                    {
                        surfaceColor = GreenerySurfaceColorSampling.GetSurfaceColor(hit, ref surfaceColorTexture);
                    }

                    Undo.RegisterCompleteObjectUndo(greeneryManager, "Added spawn point");
                    spawnPointIndex = GreeneryScatteringUtilities.AddSpawnPoint(greeneryManager,
                        scatteringModule.toolEditor.GetSelectedItems(),
                        hit,
                        surfaceColor,
                        scatteringModule.scatteringModuleSettings.slopeThreshold,
                        scatteringModule.scatteringModuleSettings.sizeFactor,
                        scatteringModule.scatteringModuleSettings.colorGradient,
                        out selectedItem);
                    EditorUtility.SetDirty(greeneryManager);
                }
                else if (currentEvent.type == EventType.MouseDrag)
                {
                    Vector2 mousePos = currentEvent.mousePosition;
                    float mousePosDistance = Vector2.Distance(Camera.current.ScreenToViewportPoint(orMousePos), Camera.current.ScreenToViewportPoint(mousePos));
                    if (spawnPointIndex >= 0)
                    {
                        greeneryManager.UpdateSpawnData(selectedItem,
                            spawnPointIndex,
                            scatteringModule.scatteringModuleSettings.sizeFactor + this.scalingSensitivity * mousePosDistance,
                            scatteringModule.scatteringModuleSettings.colorGradient.Evaluate(UnityEngine.Random.value));
                    }
                }
                else if (currentEvent.type == EventType.MouseUp)
                {
                    spawnPointIndex = -1;
                }
            }
        }

        public override float GetHeight() { return EditorGUIUtility.singleLineHeight * 2; }

        public override GUIContent GetIcon() { return EditorGUIUtility.IconContent("d_ScaleTool On"); }

        protected override void SaveSettings() { EditorPrefs.SetFloat(INDIVIDUAL_PLACEMENT_SETTINGS_KEY, scalingSensitivity); }
    }
}