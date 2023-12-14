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

        private Vector2 _orMousePos = Vector2.zero;
        private int _spawnPointIndex = -1;
        private GreeneryItem _selectedItem;
        private GreeneryManager _greeneryManager;

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
            float scale = Mathf.Max(1, EditorGUILayout.FloatField("Scaling sensitivity", scalingSensitivity));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(scatteringModule.toolEditor, "Changed individual placement scaling sensitivity");
                scalingSensitivity = scale;

                SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }


        public override void ToolHandles(Rect guiRect)
        {
            base.ToolHandles(guiRect);

            var mousePos = Event.current.mousePosition;
            var ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, scatteringModule.scatteringModuleSettings.drawingLayerMask))
            {
                if (_spawnPointIndex < 0)
                {
                    var discColor = Color.green;
                    Handles.color = discColor;
                    Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);
                    discColor.a = 0.1f;
                    Handles.color = discColor;
                    Handles.DrawSolidDisc(hit.point, hit.normal, 0.5f);
                }
            }

            if (!guiRect.Contains(mousePos))
            {
                IndividualPlacement(hit);
            }
        }

        public void IndividualPlacement(RaycastHit hit)
        {
            var currentEvent = Event.current;

            if (currentEvent.button == 0)
            {
                if (_spawnPointIndex < 0 && currentEvent.type == EventType.MouseDown)
                {
                    _greeneryManager = GreeneryEditorUtilities.GetFirstNonLocalManager();
                    if (hit.collider != null && hit.transform.TryGetComponent(out LocalGreeneryManager localGreeneryManager))
                    {
                        _greeneryManager = localGreeneryManager;
                    }

                    _orMousePos = currentEvent.mousePosition;
                    var surfaceColor = Color.clear;
                    Texture2D surfaceColorTexture = null;
                    if (scatteringModule.scatteringModuleSettings.getSurfaceColor)
                    {
                        surfaceColor = GreenerySurfaceColorSampling.GetSurfaceColor(hit, ref surfaceColorTexture);
                    }

                    Undo.RegisterCompleteObjectUndo(_greeneryManager, "Added spawn point");
                    _spawnPointIndex = GreeneryScatteringUtilities.AddSpawnPoint(_greeneryManager,
                        scatteringModule.toolEditor.GetSelectedItems(),
                        hit,
                        surfaceColor,
                        scatteringModule.scatteringModuleSettings.slopeThreshold,
                        scatteringModule.scatteringModuleSettings.sizeFactor,
                        scatteringModule.scatteringModuleSettings.colorGradient,
                        out _selectedItem);
                    EditorUtility.SetDirty(_greeneryManager);
                }
                else if (currentEvent.type == EventType.MouseDrag)
                {
                    var mousePos = currentEvent.mousePosition;
                    float mousePosDistance = Vector2.Distance(Camera.current.ScreenToViewportPoint(_orMousePos), Camera.current.ScreenToViewportPoint(mousePos));
                    if (_spawnPointIndex >= 0)
                    {
                        _greeneryManager.UpdateSpawnData(_selectedItem,
                            _spawnPointIndex,
                            scatteringModule.scatteringModuleSettings.sizeFactor + scalingSensitivity * mousePosDistance,
                            scatteringModule.scatteringModuleSettings.colorGradient.Evaluate(UnityEngine.Random.value));
                    }
                }
                else if (currentEvent.type == EventType.MouseUp)
                {
                    _spawnPointIndex = -1;
                }
            }
        }

        public override float GetHeight() { return EditorGUIUtility.singleLineHeight * 2; }

        public override GUIContent GetIcon() { return EditorGUIUtility.IconContent("d_ScaleTool On"); }

        protected override void SaveSettings() { EditorPrefs.SetFloat(INDIVIDUAL_PLACEMENT_SETTINGS_KEY, scalingSensitivity); }
    }
}