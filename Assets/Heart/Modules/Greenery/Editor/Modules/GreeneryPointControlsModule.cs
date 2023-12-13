using System;
using System.Collections.Generic;
using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [Serializable]
    public class GreeneryPointControlsModule : GreeneryEditorModule
    {
        private const string POINT_CONTROLS_SETTINGS_KEY = "GREENERY_POINT_CONTROLS_SETTINGS";

        [SerializeField] private float reprojectionHeight = 100;
        private GreeneryToolEditor toolEditor;

        public override void Initialize(GreeneryToolEditor toolEditor)
        {
            this.toolEditor = toolEditor;
            if (EditorPrefs.HasKey(POINT_CONTROLS_SETTINGS_KEY))
            {
                reprojectionHeight = EditorPrefs.GetFloat(POINT_CONTROLS_SETTINGS_KEY);
            }
            else
            {
                reprojectionHeight = 100;
            }

            toolEditor.OnGUI += OnGUI;
        }

        public override void Release() { toolEditor.OnGUI -= OnGUI; }

        public override void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Update"))
            {
                var scatteringModuleSettings = toolEditor.GetScatteringModuleSettings();
                UpdateAllSpawnData(toolEditor.GetSelectedItems(), scatteringModuleSettings);
            }

            //TODO: Implement item replacement
            // if (GUILayout.Button("Replace")) {
            //     var itemSelectionMenu = new GenericMenu();
            //     foreach (var item in toolEditor.GetLoadedItems()) {
            //         itemSelectionMenu.AddItem(new GUIContent(item.name), false, ReplaceSpawnData, item);
            //     }
            //     itemSelectionMenu.ShowAsContext();
            // }

            if (GUILayout.Button("Clear points"))
            {
                ClearSpawnData(toolEditor.GetSelectedItems());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reproject"))
            {
                var scatteringModuleSettings = toolEditor.GetScatteringModuleSettings();
                ReprojectSpawnData(toolEditor.GetSelectedItems(), this.reprojectionHeight, scatteringModuleSettings);
            }

            EditorGUILayout.LabelField("Reprojection Height", GUILayout.Width(120));
            EditorGUI.BeginChangeCheck();
            float reprojectionHeight = EditorGUILayout.FloatField(this.reprojectionHeight);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(toolEditor, "Changed reprojection height");
                this.reprojectionHeight = reprojectionHeight;
                SaveSettings();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }


        public override float GetHeight() { return EditorGUIUtility.singleLineHeight * 3; }

        public override void SaveSettings() { EditorPrefs.SetFloat(POINT_CONTROLS_SETTINGS_KEY, reprojectionHeight); }

        #region Functionality

        private void UpdateAllSpawnData(List<GreeneryItem> selectedItems, GreeneryScatteringModule.ScaterringModuleSettings scaterringModuleSettings)
        {
            GreeneryManager greeneryManager = GreeneryEditorUtilities.GetActiveManager();
            Undo.RegisterCompleteObjectUndo(greeneryManager, "Updated all points of selected item(s)");
            foreach (GreeneryItem item in selectedItems)
            {
                greeneryManager.UpdateAllSpawnData(item,
                    scaterringModuleSettings.sizeFactorLock,
                    scaterringModuleSettings.colorLock,
                    scaterringModuleSettings.sizeFactor,
                    scaterringModuleSettings.colorGradient);
            }

            EditorUtility.SetDirty(greeneryManager);
        }

        private void ClearSpawnData(List<GreeneryItem> greeneryItems)
        {
            GreeneryManager greeneryManager = GreeneryEditorUtilities.GetActiveManager();
            Undo.RegisterCompleteObjectUndo(greeneryManager, "Cleared all points of selected item(s)");
            foreach (GreeneryItem item in greeneryItems)
            {
                greeneryManager.ClearSpawnPoints(item);
            }

            EditorUtility.SetDirty(greeneryManager);
        }

        private void ReprojectSpawnData(
            List<GreeneryItem> selectedItems,
            float reprojectionHeight,
            GreeneryScatteringModule.ScaterringModuleSettings scaterringModuleSettings)
        {
            GreeneryManager greeneryManager = GreeneryEditorUtilities.GetActiveManager();
            Undo.RegisterCompleteObjectUndo(greeneryManager, "Reprojected points");

            Texture2D surfaceColorTexture = null;
            Color surfaceColor = Color.clear;
            foreach (GreeneryItem item in selectedItems)
            {
                List<SpawnData> spawnDataList = greeneryManager.GetSpawnDataList(item);
                for (int i = 0; i < spawnDataList.Count; i++)
                {
                    SpawnData spawnData = spawnDataList[i];
                    Vector3 pos = greeneryManager.transform.TransformPoint(spawnData.position);
                    if (Physics.Raycast(pos + new Vector3(0, reprojectionHeight, 0),
                            Vector3.down,
                            out RaycastHit reprojectionHit,
                            Mathf.Infinity,
                            scaterringModuleSettings.drawingLayerMask))
                    {
                        if (scaterringModuleSettings.getSurfaceColor)
                        {
                            surfaceColor = GreenerySurfaceColorSampling.GetSurfaceColor(reprojectionHit, ref surfaceColorTexture);
                        }

                        greeneryManager.ReprojectSpawnData(item,
                            i,
                            reprojectionHit.point,
                            reprojectionHit.normal,
                            surfaceColor);
                    }
                }
            }

            EditorUtility.SetDirty(greeneryManager);
        }

        private void ReplaceSpawnData(object itemObject)
        {
            GreeneryItem newItem = (GreeneryItem) itemObject;
            List<GreeneryItem> selectedItems = toolEditor.GetSelectedItems();
            GreeneryManager greeneryManager = GreeneryEditorUtilities.GetActiveManager();
            foreach (var item in selectedItems)
            {
                greeneryManager.ReplaceSpawnData(item, newItem);
                greeneryManager.ClearSpawnPoints(item);
            }
        }

        #endregion
    }
}