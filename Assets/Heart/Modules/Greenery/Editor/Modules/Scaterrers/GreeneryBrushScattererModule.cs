using System;
using System.Collections.Generic;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryBrushScattererModule : GreeneryScattererModule
    {
        [Serializable]
        public class BrushScattererModuleSettings
        {
            public float brushRadius;
            public float brushDensity;
        }

        private const string BRUSH_SCATTERER_SETTINGS_KEY = "GREENERY_BRUSH_SCATTERER_SETTINGS";

        [SerializeField] private BrushScattererModuleSettings brushScattererModuleSettings;

        private double _lastSpawnTime;


        public override void Initialize(GreeneryScatteringModule scatteringModule)
        {
            base.Initialize(scatteringModule);
            brushScattererModuleSettings = new BrushScattererModuleSettings();
            if (EditorPrefs.HasKey(BRUSH_SCATTERER_SETTINGS_KEY))
            {
                EditorJsonUtility.FromJsonOverwrite(EditorPrefs.GetString(BRUSH_SCATTERER_SETTINGS_KEY), brushScattererModuleSettings);
            }

            Undo.undoRedoPerformed += SaveSettings;
        }

        public override void Release() { Undo.undoRedoPerformed -= SaveSettings; }


        public override void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            float brushRadius = Mathf.Max(0.01f, EditorGUILayout.FloatField("Brush radius", brushScattererModuleSettings.brushRadius));
            float brushDensity = EditorGUILayout.Slider("Brush density", brushScattererModuleSettings.brushDensity, 0.01f, 1);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(scatteringModule.toolEditor, "Changed brush scatterer settings");
                brushScattererModuleSettings.brushRadius = brushRadius;
                brushScattererModuleSettings.brushDensity = brushDensity;

                SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }

        public override void ToolHandles(Rect guiRect)
        {
            base.ToolHandles(guiRect);

            Vector2 mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, scatteringModule.scatteringModuleSettings.drawingLayerMask))
            {
                Color discColor = Event.current.control ? (Event.current.shift ? Color.yellow : Color.red) : Color.cyan;
                Handles.color = discColor;
                Handles.DrawWireDisc(hit.point, hit.normal, brushScattererModuleSettings.brushRadius);
                discColor.a = 0.1f;
                Handles.color = discColor;
                Handles.DrawSolidDisc(hit.point, hit.normal, brushScattererModuleSettings.brushRadius);
            }

            if (!guiRect.Contains(mousePos))
            {
                PaintBrush(hit);
            }
        }

        public void PaintBrush(RaycastHit hit)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0)
            {
                GreeneryManager greeneryManager = GreeneryEditorUtilities.GetFirstNonLocalManager();
                if (hit.collider != null && hit.transform.TryGetComponent(out LocalGreeneryManager localGreeneryManager))
                {
                    greeneryManager = localGreeneryManager;
                }

                List<GreeneryItem> selectedItems = scatteringModule.toolEditor.GetSelectedItems();
                if (selectedItems.Count == 0) return;

                if (currentEvent.control)
                {
                    if (currentEvent.shift)
                    {
                        UpdatePoints(greeneryManager,
                            selectedItems,
                            hit.point,
                            brushScattererModuleSettings.brushRadius,
                            scatteringModule.scatteringModuleSettings);
                    }
                    else
                    {
                        RemovePoints(greeneryManager, selectedItems, hit.point, brushScattererModuleSettings.brushRadius);
                    }
                }
                else
                {
                    if (EditorApplication.timeSinceStartup > _lastSpawnTime + Mathf.Lerp(0.1f, 0.0f, brushScattererModuleSettings.brushDensity))
                    {
                        AddPoints(greeneryManager,
                            selectedItems,
                            hit,
                            brushScattererModuleSettings.brushRadius,
                            brushScattererModuleSettings.brushDensity,
                            scatteringModule.scatteringModuleSettings);

                        _lastSpawnTime = EditorApplication.timeSinceStartup;
                    }
                }
            }
        }

        public override float GetHeight() { return EditorGUIUtility.singleLineHeight * 3; }

        public override GUIContent GetIcon() { return EditorGUIUtility.IconContent("ClothInspector.PaintTool"); }

        protected override void SaveSettings() { EditorPrefs.SetString(BRUSH_SCATTERER_SETTINGS_KEY, EditorJsonUtility.ToJson(brushScattererModuleSettings)); }

        #region Functionality

        private void AddPoints(
            GreeneryManager greeneryManager,
            List<GreeneryItem> selectedItems,
            RaycastHit hit,
            float brushRadius,
            float brushDensity,
            GreeneryScatteringModule.ScaterringModuleSettings scaterringModuleSettings)
        {
            Undo.RegisterCompleteObjectUndo(greeneryManager, "Added spawn points");
            Color surfaceColor = Color.clear;
            Texture2D surfaceColorTexture = null;
            for (int i = 0; i < Mathf.Ceil(brushRadius * brushDensity); i++)
            {
                Vector3 position = hit.point + UnityEngine.Random.insideUnitSphere * brushRadius;
                if (Physics.Raycast(position + hit.normal * brushRadius,
                        -hit.normal,
                        out RaycastHit spawnHit,
                        10.0f,
                        scaterringModuleSettings.drawingLayerMask))
                {
                    if (scaterringModuleSettings.getSurfaceColor)
                    {
                        surfaceColor = GreenerySurfaceColorSampling.GetSurfaceColor(spawnHit, ref surfaceColorTexture);
                    }

                    GreeneryScatteringUtilities.AddSpawnPoint(greeneryManager,
                        selectedItems,
                        spawnHit,
                        surfaceColor,
                        scaterringModuleSettings.slopeThreshold,
                        scaterringModuleSettings.sizeFactor,
                        scaterringModuleSettings.colorGradient);
                }
            }

            EditorUtility.SetDirty(greeneryManager);
        }

        private void UpdatePoints(
            GreeneryManager greeneryManager,
            List<GreeneryItem> selectedItems,
            Vector3 hitPoint,
            float brushRadius,
            GreeneryScatteringModule.ScaterringModuleSettings scaterringModuleSettings)
        {
            foreach (GreeneryItem greeneryItem in selectedItems)
            {
                Undo.RegisterCompleteObjectUndo(greeneryManager, "Updated spawn points");
                List<SpawnData> spawnDataList = greeneryManager.GetSpawnDataList(greeneryItem);
                if (spawnDataList != null && spawnDataList.Count > 0)
                {
                    List<int> indices = new List<int>();
                    for (int i = 0; i < spawnDataList.Count; i++)
                    {
                        if (Vector3.Distance(hitPoint, greeneryManager.transform.TransformPoint(spawnDataList[i].position)) <= brushRadius)
                        {
                            indices.Add(i);
                        }
                    }

                    greeneryManager.MassUpdateSpawnData(greeneryItem,
                        indices,
                        scaterringModuleSettings.sizeFactorLock,
                        scaterringModuleSettings.colorLock,
                        scaterringModuleSettings.sizeFactor,
                        scaterringModuleSettings.colorGradient);
                }
            }

            EditorUtility.SetDirty(greeneryManager);
        }

        private void RemovePoints(GreeneryManager greeneryManager, List<GreeneryItem> selectedItems, Vector3 hitPoint, float brushRadius)
        {
            foreach (GreeneryItem item in selectedItems)
            {
                Undo.RegisterCompleteObjectUndo(greeneryManager, "Removed spawn points");
                List<SpawnData> spawnDataList = greeneryManager.GetSpawnDataList(item);
                if (spawnDataList != null && spawnDataList.Count > 0)
                {
                    for (int i = 0; i < spawnDataList.Count; i++)
                    {
                        if (Vector3.Distance(hitPoint, greeneryManager.transform.TransformPoint(spawnDataList[i].position)) <= brushRadius)
                        {
                            greeneryManager.RemoveSpawnPoint(item, i);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(greeneryManager);
        }

        #endregion
    }
}