using System;
using System.Collections.Generic;
using Pancake.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    public class AssetNameWindow : EditorWindow
    {
        public Action<string> onConfirm;
        public List<string> usedAssetsName;
        private string _assetName = string.Empty;

        public static void Show(List<string> usedAssetName, Action<string> onConfirm)
        {
            var window = GetWindow<AssetNameWindow>();
            window.minSize = window.maxSize = new Vector2(250, 100);
            window.titleContent = new GUIContent("New Asset");
            window.onConfirm = onConfirm;
            window.usedAssetsName = usedAssetName;
            window.ShowModalUtility();
        }

        private void OnGUI()
        {
            GUI.enabled = true;
            _assetName = EditorGUILayout.TextField(_assetName, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
            if (!DrawAssetNameValidation(_assetName) || !DrawTempNameValidation() || !DrawDuplicateValidation())
            {
                GUI.enabled = false;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Ok"))
            {
                onConfirm?.Invoke(_assetName.TrimStartAndEnd());
                Close();
            }
        }

        private bool DrawTempNameValidation()
        {
            if (IsTempReservedName(_assetName))
            {
                string text = string.Format($"[{{0}}] has been reserved for temp asset", _assetName);
                EditorGUILayout.HelpBox(text, MessageType.Error);
                return false;
            }

            return true;

            bool IsTempReservedName(string name)
            {
                const string temp = "Temp";
                return name.Length == temp.Length || name.Length > temp.Length && char.IsNumber(name[temp.Length]) && name.StartsWith(temp, StringComparison.Ordinal);
            }
        }

        private bool DrawDuplicateValidation()
        {
            if (usedAssetsName != null && usedAssetsName.Contains(_assetName))
            {
                EditorGUILayout.HelpBox("Name already exists!", MessageType.Error);
                return false;
            }

            return true;
        }

        public bool DrawAssetNameValidation(string assetName)
        {
            if (Editor.IsInvalidName(assetName, out var code))
            {
                switch (code)
                {
                    case EValidationErrorCode.IsNullOrEmpty:
                        EditorGUILayout.HelpBox("Please enter an asset name", MessageType.Info);
                        return false;
                    case EValidationErrorCode.StartWithNumber:
                        EditorGUILayout.HelpBox("Name starts with number is not recommended", MessageType.Error);
                        return false;
                    case EValidationErrorCode.ContainsInvalidWord:
                        EditorGUILayout.HelpBox("Contains invalid words!", MessageType.Error);
                        return false;
                    case EValidationErrorCode.ContainsWhiteSpace:
                        EditorGUILayout.HelpBox("Name with whitespace is not recommended", MessageType.Error);
                        return false;
                }
            }

            return true;
        }
    }
}